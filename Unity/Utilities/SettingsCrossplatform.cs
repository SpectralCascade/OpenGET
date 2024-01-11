using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace OpenGET.Bootstrap
{

    /// <summary>
    /// Defines basic settings you'd expect most games to support.
    /// To use this class you should inherit using the CRTP pattern, e.g.
    /// <code>public sealed class MySettings : SettingsCrossplatform&lt;MySettings&gt;</code>
    /// </summary>
    [Serializable]
    public class SettingsCrossplatform<Derived> : Settings<Derived> where Derived : SettingsCrossplatform<Derived>, new()
    {

        /// <summary>
        /// All video related settings.
        /// </summary>
        [SettingsGroup("Graphics", "Display preferences and visual quality. These settings impact game performance.")]
        public Video video = new Video();

        [Serializable]
        public class Video
        {

            /// <summary>
            /// VSync mode - whether to turn off VSync or set to a particular interval.
            /// </summary>
            public enum VSyncMode
            {
                Off = 0, // No VSync. Framerate may still be capped using Application.targetFrameRate.
                Full, // Every VBlank. For most platforms this should default to 60 FPS.
                Half, // Every other VBlank. For most platforms this should default to 30 FPS.
                Third, // Every 3rd VBlank. For most platforms this should default to 20 FPS.
                Quarter // Every 4th VBlank. For 60 Hz monitors, this is 15 FPS.
            }

            /// <summary>
            /// Default to standard VSync (every VBlank).
            /// </summary>
            public Setting<VSyncMode> vsyncMode = new Setting<VSyncMode>(
                VSyncMode.Full,
                v => QualitySettings.vSyncCount = (int)v,
                name: () => "V-Sync",
                desc: () => "Vertical synchronisation mode, synchronises rendering with the screen refresh rate."
            );

            /// <summary>
            /// Texture resolution limit. Essentially limits the mipmap levels.
            /// </summary>
            public enum TextureResolution
            {
                Full = 0,
                Half = 1,
                Quarter = 2
            }

            /// <summary>
            /// Texture resolution (i.e. the biggest mipmaps the game is allowed to use).
            /// </summary>
            public Setting<TextureResolution> textureResolution = new Setting<TextureResolution>(
                TextureResolution.Half,
#if UNITY_2022_2_OR_NEWER
                v => QualitySettings.globalTextureMipmapLimit = (int)v,
#else
                v => QualitySettings.masterTextureLimit = (int)v,
#endif
                name: () => "Texture Quality",
                desc: () => "Determines the maximum fidelity (specifically, resolution) of all textures."
            );

        };

        /// <summary>
        /// All audio related settings.
        /// </summary>
        [SettingsGroup("Audio", "Volume levels for different types of audio playback in the game.")]
        public Audio audio = new Audio();

        [Serializable]
        public class Audio
        {
            /// <summary>
            /// Volume of sound effects.
            /// </summary>
            [Slider]
            public Setting<float> volumeSFX = 1.0f;

            /// <summary>
            /// Volume of ambient environment sounds.
            /// </summary>
            [Slider]
            public Setting<float> volumeAmbient = 1.0f;

            /// <summary>
            /// Volume of background music.
            /// </summary>
            [Slider]
            public Setting<float> volumeMusic = 1.0f;

        }

        /// <summary>
        /// All input related settings.
        /// </summary>
        [SettingsGroup("Input", "Global input settings.")]
        public Input input = new Input();

        [Serializable]
        public class Input
        {
            /// <summary>
            /// Invert Y-axis mouse (or joystick) look.
            /// </summary>
            public Setting<bool> invertLookY = new Setting<bool>(
                false,
                name: () => "Invert Look Y-axis",
                desc: () => "Flip the look input upside-down, so moving input down makes the camera look up and vice-versa."
            );

#if UNITY_STANDALONE || UNITY_EDITOR
            /// <summary>
            /// Mouse sensitivity level.
            /// </summary>
            [Slider]
            public Setting<float> mouseSensitivity = new Setting<float>(
                0.5f,
                name: () => "Mouse Sensitivity",
                desc: () => "Multiplies the distance travelled by mouse inputs."
            );
#endif

        }

    }

}
