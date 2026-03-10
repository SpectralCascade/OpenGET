using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;


namespace OpenGET
{

    /// <summary>
    /// Allows creation of pause/play markers in the timeline where the timeline should wait for a trigger before continuing.
    /// </summary>
    public class PausePlayable : PlayableBehaviour
    {
        public UnityEngine.Events.UnityEvent<PausePlayable> onPause = new();

        public bool isPaused { get; private set; }

        private Playable paused;

        private bool triggered = false;

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            base.ProcessFrame(playable, info, playerData);

            if (!triggered && Application.isPlaying)
            {
                triggered = true;
                paused = playable;
                paused.GetGraph().GetRootPlayable(0).SetSpeed(0);
                isPaused = true;

                if (onPause != null)
                {
                    onPause.Invoke(this);
                }
            }
        }

        // Optional usage. You can also "resume" manually by just setting the speed on the root playable to 1f.
        public void Resume()
        {
            if (isPaused)
            {
                paused.GetGraph().GetRootPlayable(0).SetSpeed(1);
                isPaused = false;
            }
        }

    }

}
