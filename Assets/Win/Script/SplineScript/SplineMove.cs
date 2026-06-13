using UnityEngine;
using Unity.Cinemachine;

public class SplineMove : MonoBehaviour
{
    public CinemachineSplineDolly dolly;
    public float speed = 0.1f;

    void Update()
    {
        dolly.CameraPosition += speed * Time.deltaTime;
    }
}