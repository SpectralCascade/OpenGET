using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace OpenGET.UI
{

    /// <summary>
    /// Playback a specified audio clip.
    /// Only recommended for use with UI events, from code you should use AudioController.Play().
    /// </summary>
    public class Playback : AutoBehaviour
    {

        /// <summary>
        /// Audio clip to play.
        /// </summary>
        [Auto.NullCheck]
        public AudioClip clip;

        /// <summary>
        /// Audio bus/channel.
        /// </summary>
        [Auto.NullCheck]
        public AudioMixerGroup mixerGroup;

        /// <summary>
        /// Play the clip on start.
        /// </summary>
        public bool playOnStart = false;

        /// <summary>
        /// Play the audio clip.
        /// </summary>
        public void Play()
        {
            AudioController.Channel(mixerGroup).Play(clip, loop: mixerGroup == AudioController.Instance.music.group);
        }

        protected virtual void Start()
        {
            if (playOnStart)
            {
                Play();
            }
        }

    }

}
