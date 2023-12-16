#define USE_REWIRED

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace OpenGET.Input
{

#if !USE_REWIRED
    // Some Rewired input functions are identical in function to the legacy input system,
    // allowing us to use an alias.
    using Input = UnityEngine.Input;
#endif

    /// <summary>
    /// Helper class for dealing with different types of input devices.
    /// Includes callback event for gamepad connect/disconnect.
    /// If you are using Rewired (Unity plugin) you should enable the USE_REWIRED preprocessor.
    /// Note: Vanilla Unity support for gamepad connect/disconnect is rudimentary, Rewired is recommended instead.
    /// </summary>
    public class InputHelper
    {


        /// <summary>
        /// Maps a game action to input bindings.
        /// </summary>
        public class Action : ScriptableObject
        {

            public enum Type
            {
                Bool = 0,
                Float
            }

            public Action() { }
            public Action(string id, Type type = Type.Bool, params Bind[] bindings)
            {
                this.id = id;
                this.type = type;
                if (bindings != null && bindings.Length > 0)
                {
                    this.bindings = new Bind[bindings.Length];
                    for (int i = 0, counti = bindings.Length; i < counti; i++)
                    {
                        this.bindings[i] = bindings[i];
                    }
                }
            }

            /// <summary>
            /// Custom identifier used to refer to the action in the game.
            /// </summary>
            public string id;

            /// <summary>
            /// Whether this input action corresponds to a boolean or floating point value.
            /// </summary>
            public Type type;

            /// <summary>
            /// Individual controller inputs that can be used to trigger this action.
            /// </summary>
            public Bind[] bindings = new Bind[0];

            /// <summary>
            /// Is this action enabled at present?
            /// </summary>
            public bool enabled = true;

        }

        /// <summary>
        /// Built-in actions, used as defaults for certain UI such as buttons.
        /// </summary>
        public class BuiltIn
        {
            public static readonly Action Submit = new Action(
                "Submit",
                Action.Type.Bool,
                new Bind(Bind.Controller.Keyboard, Bind.Type.Button, (int)KeyCode.Return),
                new Bind(Bind.Controller.Gamepad, Bind.Type.Button, (int)Bind.GamepadButton.A),
                new Bind(Bind.Controller.Mouse, Bind.Type.Button, (int)Bind.MouseButton.PRIMARY)
            );
            public static readonly Action Cancel = new Action(
                "Cancel",
                Action.Type.Bool,
                new Bind(Bind.Controller.Keyboard, Bind.Type.Button, (int)KeyCode.Escape),
                new Bind(Bind.Controller.Gamepad, Bind.Type.Button, (int)Bind.GamepadButton.B)
            );
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
#if USE_REWIRED
            Rewired.ReInput.controllers.AddLastActiveControllerChangedDelegate(this.UpdateCursor);
#endif
        }

#if USE_REWIRED
        private void UpdateCursor(Rewired.Controller changed)
        {
            bool usingGamepad = changed != null && changed.ImplementsTemplate<Rewired.IGamepadTemplate>();
            Cursor.visible = !usingGamepad;
            Cursor.lockState = usingGamepad ? CursorLockMode.Locked : CursorLockMode.None;
        }
#endif

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

#if !USE_REWIRED
            private void UpdateCursor(bool usingGamepad)
            {
                Cursor.visible = !usingGamepad;
                Cursor.lockState = usingGamepad ? CursorLockMode.Locked : CursorLockMode.None;
            }
#endif

            /// <summary>
            /// Constructor takes an index as an identifier.
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
#if USE_REWIRED
                // Setup Rewired references and callbacks
                Input = Rewired.ReInput.players.GetPlayer(index);
                Rewired.Controller lastActive = Input.controllers.GetLastActiveController();
                usingGamepad = lastActive != null ? lastActive.ImplementsTemplate<Rewired.IGamepadTemplate>() : false;
                Input.controllers.AddLastActiveControllerChangedDelegate(this.RewiredControllerChange);
#else
                onSwitchGamepad += UpdateCursor;
                Coroutines.Start(PollController());
#endif
            }

            /// <summary>
            /// When done the instance, mark as invalid to stop input polling.
            /// </summary>
            public void Invalidate()
            {
                if (index >= 0)
                {
#if !USE_REWIRED
                    onSwitchGamepad -= UpdateCursor;
#endif
                    this.index = -1;
                }
            }

#if USE_REWIRED
            /// <summary>
            /// Rewired event handler that invokes the onSwitchGamepad event.
            /// </summary>
            private void RewiredControllerChange(Rewired.Player rewired, Rewired.Controller changed)
            {
                if (changed != null && usingGamepad != changed.ImplementsTemplate<Rewired.IGamepadTemplate>())
                {
                    usingGamepad = !usingGamepad;
                    Log.Debug("Controller changed, usingGamepad = " + usingGamepad.ToString());
                    onSwitchGamepad?.Invoke(usingGamepad);
                }
            }
#else
            /// <summary>
            /// Using the legacy input system, we have to poll to check for connected controllers.
            /// Warning: This method does not distinguish between gamepads and other controllers (e.g. joysticks).
            /// </summary>
            private IEnumerator PollController()
            {
                while (index >= 0)
                {
                    string[] controllers = Input.GetJoystickNames();
                    bool wasUsingGamepad = usingGamepad;
                    usingGamepad = controllers.Length > 0 && !string.IsNullOrEmpty(controllers[0]);
                    if (usingGamepad != wasUsingGamepad)
                    {
                        Log.Debug("Controller changed, usingGamepad = " + usingGamepad.ToString());
                        onSwitchGamepad?.Invoke(usingGamepad);
                    }
                    yield return null;
                }
            }
#endif

            /// <summary>
            /// Index of this player.
            /// </summary>
            public int index { get; private set; }

#if USE_REWIRED
            /// <summary>
            /// Rewired player instance.
            /// </summary>
            protected Rewired.Player Input = null;
#endif

            /// <summary>
            /// Returns a controller agnostic input identifier for a given binding.
            /// A negative value indicates failure to find the id.
            /// </summary>
            public int GetBindingElement(Action binding)
            {
#if USE_REWIRED
                var activeController = Input.controllers.GetLastActiveController();
                if (activeController == null)
                {
                    activeController = Input.controllers.Keyboard;
                }
                var action = Rewired.ReInput.mapping.GetAction(binding.id);
                IList<Rewired.ControllerTemplateElementTarget> targets = null;
                Rewired.ActionElementMap map = Input.controllers.maps.GetFirstElementMapWithAction(activeController, action.id, true);
                if (map == null || activeController.templateCount == 0 || 
                    activeController.Templates[0].GetElementTargets(map, targets) < 1)
                {
                    return -1;
                }

                return targets[0].element.id;
#else
                return -1;
#endif
            }

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
            public delegate void OnSwitchGamepad(bool usingGamepad);

            /// <summary>
            /// Callback for handling switching between a gamepad and some other control scheme.
            /// </summary>
            public event OnSwitchGamepad onSwitchGamepad;

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
            public bool HasInputControl(GameObject requestGroup, bool exclusive = false)
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

            /// <summary>
            /// Make sure the input system is valid/working correctly.
            /// </summary>
            private bool IsInputNominal()
            {
#if USE_REWIRED
                return Input != null;
#else
                return true;
#endif
            }

            public bool GetButtonDown(Action action, GameObject requester)
            {
                return GetButtonDown(action.id, requester);
            }

            public bool GetButtonUp(Action action, GameObject requester)
            {
                return GetButtonUp(action.id, requester);
            }

            public bool GetButton(Action action, GameObject requester)
            {
                return GetButton(action.id, requester);
            }

            public bool GetButtonDown(string action, GameObject requester)
            {
                return HasInputControl(requester) && Input.GetButtonDown(action);
            }

            public bool GetButtonUp(string action, GameObject requester)
            {
                return HasInputControl(requester) && Input.GetButtonUp(action);
            }

            public bool GetButton(string action, GameObject requester)
            {
                return HasInputControl(requester) && Input.GetButton(action);
            }

            /// <summary>
            /// Get the value of a specific axis. Optionally specify pos/neg actions that map to the desired axis.
            /// The pos/neg actions are added to the axis value but the result is clamped between -1 and 1.
            /// </summary
            public float GetAxis(string axis, GameObject requester, string actionPositive = "", string actionNegative = "")
            {
                if (!HasInputControl(requester) || !IsInputNominal())
                {
                    return 0f;
                }
                return Mathf.Clamp(
                    Input.GetAxis(axis) + 
                        (string.IsNullOrEmpty(actionPositive) ? 0 : (Input.GetButton(actionPositive) ? 1 : 0)) + 
                        (string.IsNullOrEmpty(actionNegative) ? 0 : (Input.GetButton(actionNegative) ? -1 : 0)),
                    -1,
                    1
                );
            }

            /// <summary>
            /// Get pure x and y axis values. Does not clamp or consider alternative actions used for the axes.
            /// Note: Y is mapped to Z in the returned Vector3.
            /// </summary>
            public Vector3 GetRawAxes(string axisX, string axisY, GameObject requester)
            {
                if (!HasInputControl(requester) || !IsInputNominal())
                {
                    return Vector3.zero;
                }

                return new Vector3(
                    Input.GetAxis(axisX),
                    0f,
                    Input.GetAxis(axisY)
                );
            }

            /// <summary>
            /// For each axis, returns the more extreme input value rather than summing together inputs.
            /// Note: Y axis is mapped to Z in the returned Vector3.
            /// </summary>
            public Vector3 GetExtremeAxes(string axisX, string axisY, string actionPositiveX, string actionNegativeX, string actionPositiveY, string actionNegativeY, GameObject requester)
            {
                if (!HasInputControl(requester) || !IsInputNominal())
                {
                    return Vector2.zero;
                }

                return new Vector3(
                    MathUtils.Extreme(Input.GetAxis(axisX), Input.GetButton(actionPositiveX) ? 1.0f : (Input.GetButton(actionNegativeX) ? -1.0f : 0.0f)),
                    0,
                    MathUtils.Extreme(Input.GetAxis(axisY), Input.GetButton(actionPositiveY) ? 1.0f : (Input.GetButton(actionNegativeY) ? -1.0f : 0.0f))
                );
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