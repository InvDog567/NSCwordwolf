using UnityEngine;
using UnityEngine.AI; // add at top

namespace DoorScript
{
    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(BoxCollider))]
    public class DoubleDoor : MonoBehaviour
    {
        [Header("Door Leaves")]
        public Transform leftPivot;
        public Transform rightPivot;

        [Header("Angles")]
        public float smooth = 1.0f;
        public float DoorOpenAngle = 90.0f;
        public float DoorCloseAngle = 0.0f;

        [Header("Audio")]
        public AudioClip openDoor;
        public AudioClip closeDoor;

        public NavMeshObstacle doorObstacle; // assign in Inspector, on the door leaf

        private AudioSource asource;

        private bool open = false;
        private float leftTargetAngle = 0f;
        private float rightTargetAngle = 0f;
        private int insideCount = 0;

        void Start()
        {
            asource = GetComponent<AudioSource>();
            GetComponent<BoxCollider>().isTrigger = true;

            if (leftPivot == null || rightPivot == null)
                Debug.LogWarning("DoubleDoor: leftPivot or rightPivot not assigned.");
        }

        void Update()
        {
            if (leftPivot != null)
            {
                Quaternion leftTarget = Quaternion.Euler(0, leftTargetAngle, 0);
                leftPivot.localRotation = Quaternion.Slerp(
                    leftPivot.localRotation,
                    leftTarget,
                    Time.deltaTime * smooth * 5f);
            }

            if (rightPivot != null)
            {
                Quaternion rightTarget = Quaternion.Euler(0, rightTargetAngle, 0);
                rightPivot.localRotation = Quaternion.Slerp(
                    rightPivot.localRotation,
                    rightTarget,
                    Time.deltaTime * smooth * 5f);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player") && !other.CompareTag("NPC"))
                return;

            insideCount++;

            if (insideCount == 1)
            {
                Vector3 localPos = transform.InverseTransformPoint(other.transform.position);

                // Swing away from entering side, mirrored on each leaf
                bool enteringFromFront = localPos.z >= 0;

                leftTargetAngle = enteringFromFront ? -DoorOpenAngle : DoorOpenAngle;
                rightTargetAngle = enteringFromFront ? DoorOpenAngle : -DoorOpenAngle;

                if (!open)
{
    open = true;
    if (doorObstacle != null) doorObstacle.enabled = false; // let NPCs through

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
    leftTargetAngle = DoorCloseAngle;
    rightTargetAngle = DoorCloseAngle;
    if (doorObstacle != null) doorObstacle.enabled = true; // block path again

    if (closeDoor != null)
    {
        asource.clip = closeDoor;
        asource.Play();
    }
}
        }
    }
}