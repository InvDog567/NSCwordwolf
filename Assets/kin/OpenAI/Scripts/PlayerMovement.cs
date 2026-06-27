using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("การเดิน")]
    public float walkSpeed = 4f;
    public float runSpeed = 7f;

    [Header("การกระโดด")]
    public float jumpHeight = 1.2f;
    public float gravity = -9.81f;

    [Header("การหันกล้อง")]
    public float mouseSensitivity = 120f;
    public float maxLookAngle = 80f;

    private CharacterController _controller;
    private Camera _cam;
    private Vector3 _velocity;
    private float _xRotation = 0f;
    private bool _isGrounded;
    private bool _isChatOpen = false;

    void Start()
    {
        _controller = GetComponent<CharacterController>();
        _cam = GetComponentInChildren<Camera>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleGroundCheck();
        HandleMovement();
        HandleMouseLook();
        HandleCursorLock();
    }

    void HandleGroundCheck()
    {
        _isGrounded = _controller.isGrounded;

        // รีเซ็ต velocity ตกเมื่อแตะพื้น
        if (_isGrounded && _velocity.y < 0f)
            _velocity.y = -2f;
    }

    void HandleMovement()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        bool isRunning = Input.GetKey(KeyCode.LeftShift) && v > 0;
        float speed = isRunning ? runSpeed : walkSpeed;

        Vector3 move = transform.right * h + transform.forward * v;
        _controller.Move(move * speed * Time.deltaTime);

        // กระโดด
        if (Input.GetButtonDown("Jump") && _isGrounded)
            _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        // แรงโน้มถ่วง
        _velocity.y += gravity * Time.deltaTime;
        _controller.Move(_velocity * Time.deltaTime);
    }

    void HandleMouseLook()
    {
        // ถ้า Chat UI เปิดอยู่ ไม่หันกล้อง
        if (Cursor.lockState != CursorLockMode.Locked) return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        transform.Rotate(Vector3.up * mouseX);

        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, -maxLookAngle, maxLookAngle);
        _cam.transform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
    }

    void HandleCursorLock()
    {
        // หากเปิดหน้าต่างแชทอยู่ ห้ามทำการล็อกเมาส์กลับมาเมื่อมีการคลิกหน้าจอเด็ดขาด
        if (_isChatOpen) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // คลิกซ้ายเพื่อล็อค cursor กลับมา (หลังกด Escape)
        if (Input.GetMouseButtonDown(0) && Cursor.lockState == CursorLockMode.None)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    // เรียกจาก NPCInteraction เมื่อเปิด Chat UI
    public void SetCursorFree(bool free)
    {
        _isChatOpen = free;
        Cursor.lockState = free ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = free;
    }
}