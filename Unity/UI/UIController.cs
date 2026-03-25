using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenGET.Input;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace OpenGET.UI
{

    public abstract class UIController : AutoBehaviour
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

#if ENABLE_INPUT_SYSTEM
        /// <summary>
        /// You must set this manually; otherwise code will default to whatever the instance on InputActionReferences points to.
        /// This is necessary because InputAction instances can differ even though they might be from the same original asset;
        /// unfortunately Unity treats instances of InputActionAsset as being completely different when it comes to input handling.
        /// </summary>
        public InputActionAsset[] inputActionAssets { get; set; }
        private InputActionAsset[] _inputActionAssets = new InputActionAsset[0];

        protected InputActionAsset PrimeInputActionAsset => _inputActionAssets != null && _inputActionAssets.Length > 0 ? _inputActionAssets[0] : null;
#endif
        /// <summary>
        /// The canvas for this UI.
        /// </summary>
        [Auto.NullCheck]
        [Auto.Hookup(Auto.Mode.Self)]
        public Canvas canvas;

        [Tooltip("Associated UI camera (not required for canvas \"Screenspace - Overlay\").")]
        public Camera cam;

        /// <summary>
        /// World camera. Used for converting world space to canvas space.
        /// If there is no world camera available, falls back to the UI camera if any, finally falling back to the main camera.
        /// </summary>
        public Camera worldCam
        {
            get
            {
                return _worldCam != null ? _worldCam : (cam != null ? cam : Camera.main);
            }
            protected set
            {
                _worldCam = value;
            }
        }

        [SerializeField]
        [Tooltip("Optional world camera. You may set this dynamically in code.")]
        protected Camera _worldCam;

        /// <summary>
        /// The true pixel width of the UI.
        /// </summary>
        public float width => cam != null ? cam.rect.width * Screen.width : Screen.width;

        /// <summary>
        /// The true pixel height of the UI.
        /// </summary>
        public float height => cam != null ? cam.rect.height * Screen.height : Screen.height;

        [Tooltip("Recommended - This is the parent transform used for modal popups.")]
        public Transform modalsRoot;

        [Tooltip("Recommended - This is the parent transform used for tooltips.")]
        public Transform tooltipsRoot;

        [Tooltip("Recommended - Shared tooltip used when no custom tooltip is specified.")]
        public TooltipPanel tooltipShared;

        [Tooltip("Recommended - Required for setting scroll rect sensitivity via settings.")]
        [SerializeField]
        [Auto.NullCheck]
        [Auto.Hookup]
        protected ScrollRect[] scrollRects = new ScrollRect[0];

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

        protected override void Awake()
        {
            base.Awake();

            if (tooltipShared != null)
            {
                RectTransform root = (RectTransform)(tooltipsRoot != null ? tooltipsRoot : transform);
                tooltipShared = Instantiate(tooltipShared, root);
                tooltipShared.Init(this, root);
            }
        }

        protected void OnDestroy()
        {
            if (tooltipShared != null)
            {
                Destroy(tooltipShared.gameObject);
                tooltipShared = null;
            }
        }

        /// <summary>
        /// Update scrolling sensitivity on all UI.
        /// </summary>
        public virtual void UpdateScrollSensitivity()
        {
        }

        public virtual void UpdateScaling()
        {
        }

        /// <summary>
        /// Convert a worldspace position to canvas space.
        /// Note: When camera render mode is worldspace or ScreenSpaceCamera, this is calculated as a world position based on the viewport & canvas.
        /// Corresponds to transform.position, NOT transform.localPosition.
        /// </summary>
        public Vector3 WorldToCanvasPoint(Vector3 point, bool withZ = false)
        {
            if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                return worldCam.WorldToScreenPoint(point);
            }
            else
            {
                Vector2 viewportPos = worldCam.WorldToViewportPoint(point);

                RectTransform canvasRect = canvas.transform as RectTransform;

                Vector3 pivotPos = (Vector3)viewportPos - new Vector3(canvasRect.pivot.x, canvasRect.pivot.y, 0);
                Vector2 canvasPixelDimensions = canvasRect.sizeDelta;
                return Vector3.Scale(pivotPos, canvasPixelDimensions * canvasRect.localScale.x);
            }
        }

        /// <summary>
        /// Cached reference to the current player input for this UI.
        /// </summary>
        public InputHelper.Player input => _input = (_input == null ? InputHelper.Get(currentPlayer) : _input);
        private InputHelper.Player _input;

#if ENABLE_INPUT_SYSTEM
        [Auto.NullCheck]
        [SerializeField]
        private InputActionReference actionSubmit;
        [Auto.NullCheck]
        [SerializeField]
        private InputActionReference actionCancel;
        [Auto.NullCheck]
        [SerializeField]
        private InputActionReference actionMoveSelection;

        public InputAction ActionSubmit => PrimeInputActionAsset != null ? PrimeInputActionAsset.FindAction(actionSubmit.action.id) : actionSubmit.action;
        public InputAction ActionCancel => PrimeInputActionAsset != null ? PrimeInputActionAsset.FindAction(actionCancel.action.id) : actionCancel.action;
        public InputAction ActionMoveSelection => PrimeInputActionAsset != null ? PrimeInputActionAsset.FindAction(actionMoveSelection.action.id) : actionMoveSelection.action;

#endif

    }

}
