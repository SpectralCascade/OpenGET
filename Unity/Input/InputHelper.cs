using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace OpenGET.Input
{

    /// <summary>
    /// Helper class for dealing with various input devices.
    /// Includes callback event for gamepad connect/disconnect.
    /// </summary>
    public class InputHelper
    {
        [System.Serializable]
        public struct InputSpriteMap
        {
            [Tooltip("Name of the device layout. You can see layout names in the Input Debugger (under Window->Anaylsis).")]
            public string layoutName;

            [Tooltip("Sprite atlas to associate with the device layout.")]
            public TMP_SpriteAsset spriteAtlas;

            /// <summary>
            /// Sprite name prefix to use when looking up sprites in the associated sprite atlas.
            /// </summary>
            public string prefix;

            /// <summary>
            /// Sprite name suffix to use when looking up sprites in the associated sprite atlas.
            /// </summary>
            public string suffix;

            /// <summary>
            /// The text format to display for this layout. If blank, no text will be displayed!
            /// Set to "{0}" for inserting without any formatting.
            /// See string.Format() for formatting rules; only the display string is used as an argument.
            /// </summary>
            public string displayFormat;

            /// <summary>
            /// Whether this layout prefers to always show text, or only when there is no glyph available.
            /// </summary>
            public bool showTextAlways;
        }

        /// <summary>
        /// Player lookup.
        /// </summary>
        private readonly List<Player> players = new List<Player>();

        /// <summary>
        /// Setup instance.
        /// </summary>
        private InputHelper()
        {
        }

        /// <summary>
        /// Normalise control paths into compliance with file naming conventions.
        /// </summary>
        public static string NormaliseControlPath(string controlPath)
        {
            return controlPath.Replace("/", "_");
        }

        /// <summary>
        /// Represents the input from a particular player.
        /// </summary>
        public class Player
        {
            public Player()
            {
                // Remains invalid until an index has been assigned
                index = -1;
            }

            private void UpdateCursor(bool usingGamepad)
            {
                Cursor.visible = !usingGamepad;
                Cursor.lockState = usingGamepad ? CursorLockMode.Locked : CursorLockMode.None;
            }

            /// <summary>
            /// Index is stored and used as an identifier.
            /// </summary>
            public void Init(int index)
            {
                if (this.index >= 0)
                {
                    Log.Warning(
                        "InputHelper.Player instance {0} is already initialised, did you invalidate {1} before?",
                        index,
                        this.index
                    );
                    Invalidate();
                }

                this.index = index;
                onUsingGamepad += UpdateCursor;
                UnityEngine.InputSystem.InputSystem.onActionChange += OnInputAction;
                UnityEngine.InputSystem.InputSystem.onDeviceChange += OnInputDeviceChange;
            }

            /// <summary>
            /// Update the usingGamepad flag and notify listeners if changed.
            /// </summary>
            private void UpdateGamepadStatus(bool usingGamepad)
            {
                bool wasUsingGamepad = this.usingGamepad;
                this.usingGamepad = usingGamepad;

                if (usingGamepad != wasUsingGamepad)
                {
                    // Update all input prompts, and listeners

                    if (Application.isPlaying)
                    {
                        /// Find all enabled InputPrompt components and update their text.
                        OpenGET.UI.TextFormatter[] allText = 
                            GameObject.FindObjectsOfType<OpenGET.UI.InputPrompt>().Select(x => x.GetComponent<OpenGET.UI.TextFormatter>()).ToArray();
                        for (int i = 0, counti = allText.Length; i < counti; i++)
                        {
                            if (allText[i].enabled)
                            {
                                allText[i].AutoFormat();
                            }
                        }
                    }

                    onUsingGamepad?.Invoke(usingGamepad);
                }
            }

            /// <summary>
            /// Handle switching between keyboard/mouse input and gamepad.
            /// </summary>
            private void OnInputAction(object obj, UnityEngine.InputSystem.InputActionChange change) {
                if (change == UnityEngine.InputSystem.InputActionChange.ActionPerformed)
                {
                    UnityEngine.InputSystem.InputDevice controller = 
                        ((UnityEngine.InputSystem.InputAction)obj).activeControl?.device;

                    if (controller != null)
                    {
                        UpdateGamepadStatus(controller is UnityEngine.InputSystem.Gamepad);
                    }
                }
            }

            /// <summary>
            /// Handle gamepad connection/disconnection.
            /// </summary>
            private void OnInputDeviceChange(UnityEngine.InputSystem.InputDevice device, UnityEngine.InputSystem.InputDeviceChange change)
            {
                if (device is UnityEngine.InputSystem.Gamepad)
                {
                    switch (change)
                    {
                        case UnityEngine.InputSystem.InputDeviceChange.Added:
                        case UnityEngine.InputSystem.InputDeviceChange.Reconnected:
                            _previousInputDevice = device != _activeInputDevice ? _activeInputDevice : _previousInputDevice;
                            _activeInputDevice = device;
                            UpdateGamepadStatus(true);
                            break;
                        case UnityEngine.InputSystem.InputDeviceChange.Removed:
                        case UnityEngine.InputSystem.InputDeviceChange.Disconnected:
                        default:
                            _activeInputDevice = _previousInputDevice;
                            UpdateGamepadStatus(false);
                            break;
                    }
                }
            }

            /// <summary>
            /// When done the instance, mark as invalid to unhook event listeners.
            /// </summary>
            public void Invalidate()
            {
                if (index >= 0)
                {
                    onUsingGamepad -= UpdateCursor;
                    UnityEngine.InputSystem.InputSystem.onActionChange -= OnInputAction;
                    UnityEngine.InputSystem.InputSystem.onDeviceChange -= OnInputDeviceChange;
                    this.index = -1;
                }
            }

            /// <summary>
            /// Get an input prompt string for an input action based on the current active control scheme.
            /// Optionally specify the name of a specific TMPro sprite sheet containing your input prompt glyphs.
            /// </summary>
            public string GetActionPrompt(InputAction action, out Sprite glyph, out string deviceLayoutName, out string controlPath, InputBinding.DisplayStringOptions displayOptions = 0, bool tint = false)
            {
                string sprites = "";
                TMP_SpriteAsset promptsAsset = null;
                glyph = null;
                deviceLayoutName = null;
                controlPath = null;

                for (int i = 0, counti = action.bindings.Count; i < counti; i++)
                {
                    InputBinding binding = action.bindings[i];
                    if (binding.groups.Contains(controlScheme) && !binding.isComposite)
                    {
                        // Get a text display string using Unity's built-in method
                        string bindString = action.GetBindingDisplayString(
                            i,
                            out deviceLayoutName,
                            out controlPath,
                            displayOptions
                        );
                        string spriteName = binding.path;
                        string textDisplayFormat = "{0}";
                        bool showTextAlways = false;

                        // Map device layout name to TMPro sprite asset, matching in first-last order.
                        for (int mapIndex = 0, mapCount = spriteMaps.Count; mapIndex < mapCount; mapIndex++)
                        {
                            //Log.Debug("Checking layout \"{0}\" against map \"{1}\"", deviceLayoutName, spriteMaps[mapIndex].layoutName);
                            if (deviceLayoutName == spriteMaps[mapIndex].layoutName || InputSystem.IsFirstLayoutBasedOnSecond(deviceLayoutName, spriteMaps[mapIndex].layoutName))
                            {
                                promptsAsset = spriteMaps[mapIndex].spriteAtlas;
                                textDisplayFormat = spriteMaps[mapIndex].displayFormat;
                                spriteName = (spriteMaps[mapIndex].prefix ?? "") + controlPath + (spriteMaps[mapIndex].suffix ?? "");
                                showTextAlways = spriteMaps[mapIndex].showTextAlways;
                                break;
                            }
                        }

                        //Log.Debug(
                        //    "Getting binding for control scheme \"{0}\" yields layout: \"{1}\", path: \"{2}\", sprite: \"{3}\", promptsAsset = \"{4}\"",
                        //    controlScheme,
                        //    deviceLayoutName,
                        //    controlPath,
                        //    spriteName,
                        //    promptsAsset?.name
                        //);

                        TMP_SpriteAsset found = null;
                        // Locate sprite sheet and sprite
                        int glyphIndex = -1;
                        if (promptsAsset == null)
                        {
                            found = TMP_Settings.GetSpriteAsset();
                        }
                        else
                        {
                            found = promptsAsset;
                        }
                        glyphIndex = found != null ? found.GetSpriteIndexFromName(spriteName) : -1;

                        if (found != null && glyphIndex >= 0 && glyphIndex < found.spriteGlyphTable.Count)
                        {
                            glyph = found.spriteGlyphTable[glyphIndex].sprite;
                        }
                        else
                        {
                            //Log.Warning("Failed to obtain sprite glyph with name \"{0}\" from found atlas \"{1}\"", spriteName, found?.name);
                            found = null;
                        }

                        string sprite = found == null ? "" : $"<size=150%><sprite{(promptsAsset != null ? $"=\"{promptsAsset?.name}\"" : "")} name=\"{spriteName}\"{(tint ? " tint=\"1\"" : "")}></size>";
                        string actionPrompt = "";

                        if (bindString == null)
                        {
                            bindString = "";
                        }

                        // Only set display string for valid format and in cases where there is no glyph/text is always shown
                        string displayString = 
                            (string.IsNullOrEmpty(textDisplayFormat) || !textDisplayFormat.Contains("{0}"))
                            || string.IsNullOrEmpty(bindString)
                            || (!showTextAlways && glyphIndex >= 0)
                            ? "" : string.Format(textDisplayFormat, bindString);

                        // Combine display string with the sprite
                        actionPrompt = displayString + (string.IsNullOrEmpty(displayString) || string.IsNullOrEmpty(sprite) ? "" : " ") + sprite;

                        // Show all bindings for the current control scheme, including all parts of composites, but not the composite itself
                        sprites = !string.IsNullOrEmpty(sprites) ? string.Join(
                            "/",
                            sprites,
                            actionPrompt
                        ) : actionPrompt;

                        promptsAsset = null;
                    }
                }

                return sprites;
            }

            /// <summary>
            /// Last activated input device associated with this player. Input prompts for that player are based on this device.
            /// </summary>
            public InputDevice activeInputDevice => _activeInputDevice;
            private InputDevice _activeInputDevice = null;
            private InputDevice _previousInputDevice = null;

            /// <summary>
            /// Get the control scheme for this player.
            /// </summary>
            public string controlScheme => PlayerInput.GetPlayerByIndex(index).currentControlScheme;

            /// <summary>
            /// Index of this player.
            /// </summary>
            public int index { get; private set; }

            /// <summary>
            /// Is this player using a gamepad or some other input device (such as keyboard, mouse, touch etc.)?
            /// </summary>
            public bool usingGamepad { get; private set; }

            /// <summary>
            /// Mappings of device layouts to sprite atlases.
            /// </summary>
            [SerializeField]
            private List<InputSpriteMap> spriteMaps = new List<InputSpriteMap>();

            /// <summary>
            /// The current selected GameObject.
            /// Note: For local multiplayer, this may have to use the appropriate EventSystem for a particular player.
            /// </summary>
            public GameObject selected {
                get { return _selected; }
                set {
                    GameObject current = _selected;
                    if (current != value)
                    {
                        lastSelected = current;
                    }
                    _selected = value;
                    EventSystem.current.SetSelectedGameObject(null);
                    EventSystem.current.SetSelectedGameObject(_selected);
                }
            }

            /// <summary>
            /// The last selected GameObject. Can be null if the previous selected value was null.
            /// </summary>
            public GameObject lastSelected { get; private set; }
            private GameObject _selected = null;

            /// <summary>
            /// There are cases where multiple elements in the same group may request input control,
            /// so we need to make sure requests are cancelled out before we can pop the stack.
            /// </summary>
            private Dictionary<GameObject, int> requestCounts = new Dictionary<GameObject, int>();

            /// <summary>
            /// A stack of monobehaviour references that have requested input control.
            /// The top of the stack has full control.
            /// </summary>
            private Stack<GameObject> controlStack = new Stack<GameObject>();

            /// <summary>
            /// Callback delegate which indicates whether the player has switched to or from the gamepad.
            /// </summary>
            public delegate void OnUsingGamepad(bool usingGamepad);

            /// <summary>
            /// Callback for handling switching between a gamepad and some other control scheme.
            /// </summary>
            public event OnUsingGamepad onUsingGamepad;

            /// <summary>
            /// Add a sprite atlas mapping. Note this will only add unique layouts; you can't map multiple atlases to the same layout.
            /// Returns true on first add, false if the layout has already been added.
            /// Please note, the order of addition is important for mapping specific layouts to specific sprite sheets.
            /// You should add "default"/"fallback" layouts (i.e. base layouts) AFTER more-specific layouts,
            /// e.g. "Gamepad" should be added after "DualShockGamepad".
            /// Use a display format of "{0}" to support text for the layout; follows string.Format() rules with 1 argument (the binding display text).
            /// </summary>
            public bool AddSpriteMap(string deviceLayoutName, TMPro.TMP_SpriteAsset spriteAtlas, string prefix = "", string suffix = "", bool showTextAlways = false, string displayFormat = "[{0}]")
            {
                if (spriteMaps.Where(x => x.layoutName == deviceLayoutName).Count() <= 0)
                {
                    spriteMaps.Add(new InputSpriteMap {
                        layoutName = deviceLayoutName,
                        spriteAtlas = spriteAtlas,
                        prefix = prefix,
                        suffix = suffix,
                        displayFormat = displayFormat,
                        showTextAlways = showTextAlways
                    });
                    return true;
                }
                return false;
            }

            /// <summary>
            /// Clear all sprite atlas mappings.
            /// </summary>
            public void ClearSpriteMaps()
            {
                spriteMaps.Clear();
            }

            /// <summary>
            /// Push a particular group of input handlers onto the priority stack.
            /// The reference is pushed onto a stack with the number of associated calls tracked,
            /// such that requesting the same group for different elements (e.g. multiple buttons) is accounted for.
            /// You must always pair this with a corresponding FreeInputControl() call.
            /// Typically, you should request input when you show a request group and and free it when the request group is hidden.
            /// </summary>
            public void RequestInputControl(GameObject requestGroup)
            {
                if (requestCounts.ContainsKey(requestGroup))
                {
                    requestCounts[requestGroup]++;
                }
                else
                {
                    controlStack.Push(requestGroup);
                    requestCounts.Add(requestGroup, 1);
                    Log.Debug("Request group {0} was pushed to the top of the input control stack.", requestGroup.gameObject.name);
                }
            }

            /// <summary>
            /// Only request input control the once for a given request group until the next FreeInputControl() call.
            /// Convenience method.
            /// </summary>
            public void RequestInputControlOnce(GameObject requestGroup)
            {
                if (!requestCounts.ContainsKey(requestGroup) || requestCounts[requestGroup] < 1)
                {
                    RequestInputControl(requestGroup);
                }
                else
                {
                    // Do nothing
                }
            }

            /// <summary>
            /// Free input control from a particular group that has previously been requested.
            /// Note this does not free control until all elements of the group have been cleared, so beware.
            /// Always pair this with a corresponding prior RequestInputControl() call.
            /// </summary>
            public void FreeInputControl(GameObject requestGroup)
            {
                if (requestCounts.TryGetValue(requestGroup, out int count))
                {
                    if (count > 0)
                    {
                        requestCounts[requestGroup]--;
                        if (count == 1)
                        {
                            CleanControlStack();
                        }
                    }
                    else
                    {
                        Log.Warning("Attempted to free more elements of input control group {0} than originally requested!", requestGroup != null ? requestGroup.gameObject.name : null);
                    }
                }
                else
                {
                    Log.Warning("Attempted to free input control group {0}, but it does not exist on the stack!", requestGroup != null ? requestGroup.gameObject.name : null);
                }
            }

            /// <summary>
            /// Removes elements through the control stack from top to bottom,
            /// but stops as soon as a group is reached that hasn't been fully freed of input control.
            /// Also removes groups that no longer need tracking in the requestCounts dictionary.
            /// </summary>
            private void CleanControlStack()
            {
                while (controlStack.Count > 0 && requestCounts[controlStack.Peek()] == 0)
                {
                    GameObject group = controlStack.Pop();
                    requestCounts.Remove(group);
                    Log.Debug("Request group [{0}] was popped off the top of the input control stack.", group != null ? group.name : "");
                }
            }

            /// <summary>
            /// Has the given request group got input control currently?
            /// Returns true if control is available. By default this includes cases where there are no groups in the control stack.
            /// Optionally specify exclusive = false if you wish to check that the group has exclusive control currently.
            /// </summary>
            public bool HasControl(GameObject requestGroup, bool exclusive = false)
            {
                return (controlStack.Count > 0 && controlStack.Peek() == requestGroup) || (!exclusive && controlStack.Count == 0);
            }

            /// <summary>
            /// Get the group input control reference at the top of the stack (i.e. the current group in control).
            /// </summary>
            public GameObject PeekStack()
            {
                return controlStack.Count > 0 ? controlStack.Peek() : null;
            }

            /// <summary>
            /// Has anything requested input control, or is it freely available?
            /// </summary>
            public bool IsInputLocked()
            {
                return controlStack.Count > 0;
            }
        }

        /// <summary>
        /// TODO: Move elsewhere/make configurable.
        /// </summary>
        public const int MAX_PLAYERS = 1;

        /// <summary>
        /// Retrieve a particular player input.
        /// If the player instance with the provided index doesn't exist, a new one is created.
        /// </summary>
        public static Player Get(int index)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                _shared = null;
                return null;
            }
#endif
            if (_shared == null)
            {
                _shared = new InputHelper();
            }
            int playerCount = _shared.players.Count;
            Player player = null;
            if (playerCount <= index)
            {
                if (index < MAX_PLAYERS)
                {
                    // Ensure there is always a valid entry
                    for (int i = _shared.players.Count; i <= index; i++)
                    {
                        Player entry = new Player();
                        _shared.players.Add(entry);
                        entry.Init(i);
                    }
                    player = _shared.players[index];
                }
                else
                {
                    Debug.LogError(string.Format("WARNING: Player index {0} exceeds the maximum number of players: {1}", index, _shared.players.Count));
                }
            }
            else if (index >= 0)
            {
                player = _shared.players[index];
            }
            else
            {
                Debug.LogError("WARNING: Player index out of range, cannot provide InputHelper.Player instance.");
            }
            return player;
        }
        private static InputHelper _shared = null;

    }

}