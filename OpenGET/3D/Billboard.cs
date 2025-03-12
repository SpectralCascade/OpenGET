using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// "Billboard" a 3D object such that it always faces a camera on the desired axes.
/// </summary>
public class Billboard : MonoBehaviour
{
    /// <summary>
    /// Target transform to look at. If null, defaults to main camera.
    /// </summary>
    public Transform target = null;

    [SerializeField]
    [Tooltip("Point at target on the X axis.")]
    private bool x = true;

    [SerializeField]
    [Tooltip("Point at target on the Y axis.")]
    private bool y = true;

    [SerializeField]
    [Tooltip("Point at target on the Z axis.")]
    private bool z = true;

    private void Update()
    {
        if (target == null)
        {
            target = Camera.main != null ? Camera.main.transform : null;
        }

        if (target != null)
        {
            Vector3 rot = transform.eulerAngles;
            transform.forward = target.forward;
            rot.x = x ? transform.rotation.eulerAngles.x : rot.x;
            rot.y = y ? transform.rotation.eulerAngles.y : rot.y;
            rot.z = z ? transform.rotation.eulerAngles.z : rot.z;
            transform.rotation = Quaternion.Euler(rot);
        }
    }
}
