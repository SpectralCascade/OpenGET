using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OpenGET.Input
{

    [System.Serializable]
    public struct Bind
    {
        public enum Type
        {
            Button,
            Axis
        }

        public enum Controller
        {
            Custom,
            Keyboard,
            Mouse,
            Gamepad
        }

        /// <summary>
        /// Based on SDL_GameControllerButton from SDL.
        /// </summary>
        public enum GamepadButton
        {
            A,
            B,
            X,
            Y,
            BACK,
            GUIDE,
            START,
            LEFTSTICK,
            RIGHTSTICK,
            LEFTSHOULDER,
            RIGHTSHOULDER,
            DPAD_UP,
            DPAD_DOWN,
            DPAD_LEFT,
            DPAD_RIGHT,
            MISC1,      /* Xbox Series X share button, PS5 microphone button, Nintendo Switch Pro capture button */
            PADDLE1,    /* Xbox Elite paddle P1 */
            PADDLE2,    /* Xbox Elite paddle P3 */
            PADDLE3,    /* Xbox Elite paddle P2 */
            PADDLE4,    /* Xbox Elite paddle P4 */
            TOUCHPAD    /* PS4/PS5 touchpad button */
        }

        /// <summary>
        /// Based on SDL_GameControllerAxis from SDL.
        /// </summary>
        public enum GamepadAxis
        {
            LEFTX,
            LEFTY,
            RIGHTX,
            RIGHTY,
            TRIGGERLEFT,
            TRIGGERRIGHT
        }

        public enum MouseAxis
        {
            SCROLL_X,
            SCROLL_Y
        }

        public enum MouseButton
        {
            PRIMARY,
            SECONDARY,
            SCROLL_WHEEL
        }

        public Bind(Controller source, Type type, int id)
        {
            this.source = source;
            this.type = type;
            this.id = id;
        }

        /// <summary>
        /// Get the type associated with the identifier.
        /// </summary>
        public System.Type GetIdType()
        {
            switch (source)
            {
                case Controller.Gamepad:
                    return type == Type.Button ? typeof(GamepadButton) : typeof(GamepadAxis);
                case Controller.Keyboard:
                    return type == Type.Button ? typeof(KeyCode) : typeof(int);
                case Controller.Mouse:
                    return type == Type.Button ? typeof(MouseButton) : typeof(MouseAxis);
                case Controller.Custom:
                default:
                    return typeof(int);
            }
        }

        /// <summary>
        /// Which controller type this binding pertains to.
        /// </summary>
        public Controller source;

        /// <summary>
        /// Whether this is a button or axis input.
        /// </summary>
        public Type type;

        /// <summary>
        /// Identifier that corresponds to a relevant enum value.
        /// </summary>
        public int id;
    }

}

