using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenGET.Input;

namespace OpenGET.UI
{

    public abstract class UIController : MonoBehaviour
    {

        /// <summary>
        /// Selectable UI elements should inherit this interface.
        /// </summary>
        public interface ISelectable
        {
        }

        /// <summary>
        /// Global settings for this UI. Default values are used when unassigned.
        /// </summary>
        public UIConfig settings {
            get {
                if (_settings == null)
                {
                    _settings = new UIConfig();
                }
                return _settings;
            }
        }

        [SerializeField]
        [Tooltip("Optional - define custom settings to use for all UI.")]
        private UIConfig _settings;

        [Tooltip("Recommended - This is the parent transform used for modal popups.")]
        public Transform modalsRoot;

        /// <summary>
        /// Current player input index.
        /// </summary>
        public int currentPlayer {
            get {
                return _currentPlayer;
            }
            private set {
                if (_currentPlayer != value)
                {
                    // Reset input
                    _input = null;
                }
                _currentPlayer = value;
            }
        }
        private int _currentPlayer = 0;

        /// <summary>
        /// Cached reference to the current player input for this UI.
        /// </summary>
        public InputHelper.Player input => _input = (_input == null ? InputHelper.Get(currentPlayer) : _input);
        private InputHelper.Player _input;

        [Auto.NullCheck]
        public UnityEngine.InputSystem.InputActionReference actionSubmit;
        [Auto.NullCheck]
        public UnityEngine.InputSystem.InputActionReference actionCancel;
        [Auto.NullCheck]
        public UnityEngine.InputSystem.InputActionReference actionMoveSelection;

    }

}
