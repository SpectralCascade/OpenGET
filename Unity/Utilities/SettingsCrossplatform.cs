using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenGET.UI;
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

        [Serializable]
        public class Audio
        {
            /// <summary>
            /// Set the volume of an audio bus.
            /// </summary>
            protected static void ApplyVolume(string channel, float v)
            {
                AudioController.Bus bus = AudioController.Channel(channel);
                if (bus != null)
                {
                    bus.volume = v;
                }
            }

            /// <summary>
            /// Master volume.
            /// </summary>
            [Slider]
            public Setting<float> volumeMaster = new Setting<float>(
                1,
                v => ApplyVolume("Master", v),
                true,
                name: () => "Master Volume",
                desc: () => "Overall volume of all audio in the game."
            );

            /// <summary>
            /// Volume of sound effects such as ambient environment sounds or NPCs interacting with objects.
            /// </summary>
            [Slider]
            public Setting<float> volumeSFX = new Setting<float>(
                1,
                v => ApplyVolume("SFX", v),
                true,
                name: () => "Sound Effects Volume",
                desc: () => "Volume level of non-UI sound effects in the game."
            );

            /// <summary>
            /// Volume of UI sounds.
            /// </summary>
            [Slider]
            public Setting<float> volumeUI = new Setting<float>(
                1,
                v => ApplyVolume("UI", v),
                true,
                name: () => "UI Volume",
                desc: () => "Volume level of UI sounds in the game."
            );

            /// <summary>
            /// Volume of music.
            /// </summary>
            [Slider]
            public Setting<float> volumeMusic = new Setting<float>(
                1,
                v => ApplyVolume("Music", v),
                true,
                name: () => "Music Volume",
                desc: () => "Volume level of music in the game."
            );
            
        }

        [Serializable]
        public class Input
        {
            /// <summary>
            /// Force text to be shown alongside glyphs when displaying input prompts.
            /// </summary>
            public Setting<bool> alwaysDisplayPromptText = new Setting<bool>(
                false,
                name: () => "Always Display Text for Input Prompts",
                desc: () => "When enabled, the name of input bindings associated with an input prompt are always displayed alongside the icon."
            );

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

            [Slider]
            public Setting<float> scrollSensitivity = new Setting<float>(
                0.5f,
                name: () => "Scoll Sensitivity",
                desc: () => "How much content moves when scrolling."
            );
#endif

        }

    }

}
