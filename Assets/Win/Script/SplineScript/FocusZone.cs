using UnityEngine;
using Unity.Cinemachine;

public class FocusZone : MonoBehaviour
{
    public Transform focusPoint;
    public CinemachineRotationComposer composer;

    public float focusDistance = 10f;

    void Update()
    {
        float d = Vector3.Distance(transform.position, focusPoint.position);

        composer.enabled = d < focusDistance;
    }
}