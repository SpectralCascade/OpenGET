using System.Collections.Generic;
using UnityEngine;

namespace OpenGET
{

    /// <summary>
    /// A cheap way to implement light rays using a baked mesh.
    /// Note: Baked mesh is always 1 unit length in the forward direction.
    /// The vertex shader does all the heavy lifting (modifies vertices based on angle and distance).
    /// </summary>
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class MeshLightRay : AutoBehaviour
    {
        /// <summary>
        /// The mesh filter to generate a mesh for.
        /// </summary>
        [Auto.Hookup(Auto.Mode.Self)]
        [Auto.NullCheck]
        public MeshFilter generated;

        /// <summary>
        /// The mesh renderer to use.
        /// </summary>
        [Auto.Hookup(Auto.Mode.Self)]
        [Auto.NullCheck]
        public MeshRenderer render;

        [Tooltip("XY plane shape that defines the mesh.")]
        public List<Vector2> shape = new List<Vector2>();

        [Tooltip("The maximum distance light ray mesh should extend.")]
        public float distance = 1;

        [Tooltip("How much the light ray appears to spread out.")]
        public float spread = 1f;

        [Tooltip("What colour to use for the effect.")]
        public Color color = Color.white;

        [Tooltip("Worldspace direction vector to use for the angle (normalised).")]
        public Vector3 direction = new Vector3(0, 0, 1);

        /// <summary>
        /// Maximum angle from forward direction before cutting out.
        /// </summary>
        [Range(0f, 360f)]
        public float maxAngle = 90f;

        /// <summary>
        /// The threshold angle after which the effect is faded out until max angle.
        /// </summary>
        [Range(0f, 360f)]
        public float fadeAngle = 45f;

        protected static readonly int PropColour = Shader.PropertyToID("_Color");
        protected static readonly int PropDistance = Shader.PropertyToID("_Distance");
        protected static readonly int PropSpread = Shader.PropertyToID("_Spread");
        protected static readonly int PropDirection = Shader.PropertyToID("_Direction");

        protected override void Awake()
        {
            base.Awake();

            // Bake the mesh
            // TODO: Bake in editor and serialise instead of on Awake!
            generated.mesh = new Mesh();
            GenerateMesh();
        }

        protected void Update()
        {
            if (render != null && render.material != null)
            {
                Material mat = render.material;
                Vector3 worldSpaceDirection = Quaternion.Inverse(transform.rotation) * direction;

                // Adjust opacity based on angle between rays and forward direction
                float angle = Mathf.Clamp(Vector3.Angle(transform.forward, direction), 0, maxAngle);
                float min = Mathf.Min(fadeAngle, maxAngle);
                float angleFactor = angle < fadeAngle ? 1f : (
                    angle >= maxAngle ? 0f : Mathf.Lerp(1, 0, (angle - min) / Mathf.Max(float.Epsilon, maxAngle - min))
                );

                mat.SetColor(PropColour, Colors.Alpha(color, color.a * angleFactor));
                mat.SetFloat(PropDistance, distance);
                mat.SetFloat(PropSpread, spread);
                // Set the direction to world space
                mat.SetVector(PropDirection, worldSpaceDirection);
            }
        }

#if UNITY_EDITOR
        protected void OnGUI()
        {
            if (generated != null && generated.mesh == null)
            {
                generated.mesh = new Mesh();
            }
            GenerateMesh();
        }
#endif

        [ContextMenu("Generate Mesh")]
        protected void GenerateMesh()
        {
            // Base shape must have two or more vertices to make at least one valid face
            if (shape.Count < 2)
            {
                return;
            }

            if (generated.mesh == null)
            {
                generated.mesh = new Mesh();
            }

            // Calculate the steps for each triangle and face
            const int stepTriangle = 3;
            const int stepFace = stepTriangle * 2;

            // Vertex count
            int vertCount = shape.Count * 2;

            // Setup verts, UV coords and triangles
            Vector3[] vertices = new Vector3[vertCount];
            Vector2[] uvs = new Vector2[vertCount];
            int[] triangles = new int[stepFace * shape.Count];

            // Generate the mesh data from the shape
            for (int i = 0, counti = shape.Count; i < counti; i++)
            {
                // Compute vertex pair (base and far)
                int next = i + 1;
                vertices[i] = shape[i];
                vertices[counti + i] = new Vector3(shape[i].x, shape[i].y, 1f);

                // Compute UV coord pair to wrap texture around the shape uniformly
                float uv = (float)i / counti - 1;
                uvs[i] = new Vector2(0, uv); // Min
                uvs[counti + i] = new Vector2(1, uv); // Max

                //
                // Outer faces
                //

                // Triangle 1
                int root = i * stepFace;
                triangles[root] = i; // Base vertex 1
                triangles[root + 2] = (i + 1) % counti; // Base vertex 2
                triangles[root + 1] = (counti + i); // Far vertex 1

                // Triangle 2
                triangles[root + 3] = (i + 1) % counti; // Base vertex 2
                triangles[root + 4] = (counti + i); // Far vertex 1
                triangles[root + 5] = i == counti - 1 ? counti : (counti + i + 1) % vertCount; // Far vertex 2
            }

            // Now update the existing mesh
            generated.mesh.Clear();

            generated.mesh.vertices = vertices;
            generated.mesh.uv = uvs;
            generated.mesh.triangles = triangles;

            generated.mesh.RecalculateNormals();
        }

    }

}
