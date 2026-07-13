using UnityEngine;

public class DoorAnimator : MonoBehaviour
{
    public Animator animator;

    private int insideCount = 0;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") && !other.CompareTag("NPC"))
            return;

        insideCount++;

        if (insideCount == 1)
        {
            Vector3 local = transform.InverseTransformPoint(other.transform.position);

            if (local.z > 0)
                animator.SetTrigger("OpenForward");
            else
                animator.SetTrigger("OpenBackward");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player") && !other.CompareTag("NPC"))
            return;

        insideCount--;

        if (insideCount <= 0)
        {
            insideCount = 0;
            animator.SetTrigger("Close");
        }
    }
}