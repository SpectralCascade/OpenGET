using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace OpenGET
{
    public class PauseTimeline : PlayableAsset
    {
        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            ScriptPlayable<PausePlayable> player = ScriptPlayable<PausePlayable>.Create(graph);
            return player;
        }
    }

}
