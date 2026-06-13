using UnityEngine;
using UnityEngine.AI;
 
public class MovementAnimator : MonoBehaviour
{
    public Animator animator;
    public NavMeshAgent agent;
 
    [Header("Settings")]
    [Tooltip("Speed below this is considered 'stopped'. Keep it low.")]
    public float moveThreshold = 0.05f;
 
    [Tooltip("How fast the animation blends in/out. Lower = smoother.")]
    public float animationDampTime = 0.1f;
 
    void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
 
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();
 
        // FIX 1: Turn off auto-braking so the agent doesn't slow down
        // before reaching a waypoint. It will now maintain full speed
        // right up until it arrives.
        agent.autoBraking = false;
 
        // FIX 3: Tell the NavMeshAgent to update the transform position
        // itself. If your Animator has "Apply Root Motion" ON, it fights
        // the agent and shifts the model forward. We disable root motion
        // here in code so the agent stays in control of movement.
        animator.applyRootMotion = false;
    }
 
    void Update()
    {
        // FIX 2: Use desiredVelocity instead of velocity.
        // agent.velocity is the ACTUAL physics velocity — it lags behind
        // and drops to zero a moment before/after movement.
        // agent.desiredVelocity is what the agent WANTS to do right now,
        // which stays accurate the whole time it has a destination.
        float speed = agent.desiredVelocity.magnitude;
 
        bool isMoving = speed > moveThreshold;
 
        // Using SetBool with a small damp so it doesn't flicker on/off
        // if you have a blend tree, swap SetBool for SetFloat:
        // animator.SetFloat("Speed", speed, animationDampTime, Time.deltaTime);
        animator.SetBool("isWalking", isMoving);
    }
}