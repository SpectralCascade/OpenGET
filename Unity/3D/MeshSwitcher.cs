using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OpenGET
{

    /// <summary>
    /// See MeshSwitcherEditor for the implementation of the dropdown.
    /// </summary>
    [AddComponentMenu("OpenGET/MeshSwitcher")]
    public class MeshSwitcher : AutoBehaviour
    {
        [Auto.Hookup]
        public MeshFilter meshFilter;

        [Auto.Hookup]
        public MeshRenderer meshRenderer;

        public List<Mesh> meshes = new List<Mesh>();
        public List<Material> materials = new List<Material>();

    }

}
