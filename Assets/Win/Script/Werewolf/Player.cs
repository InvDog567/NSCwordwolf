using UnityEngine;

[System.Serializable]
public class FootstepSurface
{
    public string tagName;
    public AudioClip walkSound;
}

public class Player : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 5f;
    public float jumpHeight = 1.5f;
    public float gravity = -9.81f;

    [Header("Mouse Look")]
    public float mouseSensitivityX = 2f;
    public float mouseSensitivityY = 2f;
    public Transform cameraPivot;

    [Header("Footsteps")]
    public AudioSource footstepSource;
    public FootstepSurface[] surfaces;
    public AudioClip defaultWalkSound;

    private CharacterController controller;
    private float yVelocity;
    private float xRotation;
    private bool isCursorFree;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        SetCursorFree(false);

        if (footstepSource != null)
        {
            footstepSource.playOnAwake = false;
            footstepSource.loop = true;
        }

        ApplySensitivity();
    }

    void Update()
    {
        if (Time.timeScale == 0f)
            return;

        if (isCursorFree)
        {
            StopFootsteps();
            return;
        }

        HandleMouseLook();
        HandleMovement();
        HandleFootsteps();
    }

    public void SetCursorFree(bool free)
    {
        isCursorFree = free;
        Cursor.lockState = free ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = free;
    }

    public void UnlockCursorForChat()
    {
        SetCursorFree(true);
    }

    public void LockCursorForGameplay()
    {
        SetCursorFree(false);
    }

    private void ApplySensitivity()
    {
        mouseSensitivityX = GameSettings.SensitivityX;
        mouseSensitivityY = GameSettings.SensitivityY;
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * GameSettings.SensitivityX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * GameSettings.SensitivityY;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);

        if (cameraPivot != null)
            cameraPivot.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        transform.Rotate(Vector3.up * mouseX);
    }

    void HandleMovement()
    {
        if (controller == null)
            return;

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 move = transform.right * x + transform.forward * z;

        if (controller.isGrounded && yVelocity < 0)
            yVelocity = -2f;

        if (Input.GetButtonDown("Jump") && controller.isGrounded)
            yVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);

        yVelocity += gravity * Time.deltaTime;
        Vector3 velocity = move * speed;
        velocity.y = yVelocity;
        controller.Move(velocity * Time.deltaTime);
    }

    void HandleFootsteps()
    {
        if (controller == null || footstepSource == null)
            return;

        bool isMoving =
            controller.isGrounded &&
            (
                Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.01f ||
                Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.01f
            );

        if (!isMoving)
        {
            StopFootsteps();
            return;
        }

        AudioClip targetClip = GetCurrentSurfaceSound();
        if (targetClip == null) return;

        if (footstepSource.clip != targetClip)
        {
            footstepSource.Stop();
            footstepSource.clip = targetClip;
            footstepSource.Play();
        }
        else if (!footstepSource.isPlaying)
        {
            footstepSource.Play();
        }
    }

    private void StopFootsteps()
    {
        if (footstepSource != null && footstepSource.isPlaying)
            footstepSource.Stop();
    }

    AudioClip GetCurrentSurfaceSound()
    {
        if (Physics.Raycast(transform.position + Vector3.up * 0.2f, Vector3.down, out RaycastHit hit, 2f))
        {
            foreach (FootstepSurface surface in surfaces)
            {
                if (surface != null && hit.collider.CompareTag(surface.tagName))
                    return surface.walkSound;
            }
        }
        return defaultWalkSound;
    }
}
