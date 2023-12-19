using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace OpenGET.Input
{

    /// <summary>
    /// Helper class for dealing with different types of input devices.
    /// Includes callback event for gamepad connect/disconnect.
    /// If you are using Rewired (Unity plugin) you should enable the USE_REWIRED preprocessor.
    /// Note: Vanilla Unity support for gamepad connect/disconnect is rudimentary, Rewired is recommended instead.
    /// </summary>
    public class InputHelper
    {

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
                            UpdateGamepadStatus(true);
                            break;
                        case UnityEngine.InputSystem.InputDeviceChange.Removed:
                        case UnityEngine.InputSystem.InputDeviceChange.Disconnected:
                        default:
                            UpdateGamepadStatus(false);
                            break;
                    }
                }
            }

            /// <summary>
            /// When done the instance, mark as invalid to stop input polling.
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
            /// Index of this player.
            /// </summary>
            public int index { get; private set; }

            /// <summary>
            /// Is this player using a gamepad or some other input device (such as keyboard, mouse, touch etc.)?
            /// </summary>
            public bool usingGamepad { get; private set; }

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
                        Log.Warning("Attempted to free more elements of input control group {0} than originally requested!", requestGroup?.gameObject.name);
                    }
                }
                else
                {
                    Log.Warning("Attempted to free input control group {0}, but it does not exist on the stack!", requestGroup?.gameObject.name);
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
                    Log.Debug("Request group {0} was popped off the top of the input control stack.", group.gameObject.name);
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