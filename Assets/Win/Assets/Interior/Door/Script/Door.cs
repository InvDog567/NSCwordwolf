using UnityEngine;
using UnityEngine.AI;

namespace DoorScript
{
    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(BoxCollider))]
    public class Door : MonoBehaviour
    {
        public Transform doorPivot;

        public float smooth = 1.0f;
        public float DoorOpenAngle = 90.0f;
        public float DoorCloseAngle = 0.0f;

        public AudioClip openDoor;
        public AudioClip closeDoor;

        public NavMeshObstacle doorObstacle; // assign in Inspector

        private AudioSource asource;

        private bool open = false;
        private float currentTargetAngle = 0f;
        private int insideCount = 0;

        void Start()
        {
            asource = GetComponent<AudioSource>();
            GetComponent<BoxCollider>().isTrigger = true;

            if (doorPivot == null)
                Debug.LogWarning("Door: doorPivot not assigned, rotating self instead.");
        }

        void Update()
        {
            Transform t = doorPivot != null ? doorPivot : transform;
            Quaternion target = Quaternion.Euler(0, currentTargetAngle, 0);

            t.localRotation = Quaternion.Slerp(
                t.localRotation,
                target,
                Time.deltaTime * smooth * 5f);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player") && !other.CompareTag("NPC"))
                return;

            insideCount++;

            if (insideCount == 1)
            {
                Vector3 localPos = transform.InverseTransformPoint(other.transform.position);
                currentTargetAngle = (localPos.z >= 0) ? -DoorOpenAngle : DoorOpenAngle;

                if (!open)
                {
                    open = true;
                    if (doorObstacle != null) doorObstacle.enabled = false;

                    if (openDoor != null)
                    {
                        asource.clip = openDoor;
                        asource.Play();
                    }
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player") && !other.CompareTag("NPC"))
                return;

            insideCount = Mathf.Max(insideCount - 1, 0);

            if (insideCount == 0 && open)
            {
                open = false;
                currentTargetAngle = DoorCloseAngle;
                if (doorObstacle != null) doorObstacle.enabled = true;

                if (closeDoor != null)
                {
                    asource.clip = closeDoor;
                    asource.Play();
                }
            }
        }
    }
}