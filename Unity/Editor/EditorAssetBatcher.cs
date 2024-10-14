using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OpenGET
{

    public abstract class EditorAssetBatcher : ScriptableObject
    {
        public abstract string filter { get; }

        public abstract Object[] ReadAssets(EditorConfig config);

        public abstract void WriteAssets(EditorConfig config, Object[] assets);

    }

}
