using UnityEngine;

public class NPCInteraction : MonoBehaviour
{
    [Header("Settings")]
    public float interactRange = 3f;
    public KeyCode interactKey = KeyCode.E;

    [Header("UI")]
    public GameObject pressEPrompt; // ลากเฉพาะ "ป้ายกด E" มาใส่ตรงนี้

    private Camera _cam;
    private NPCChatController _hoveredNPC;
    private NPCChatController _activeNPC;
    private PlayerMovement _playerMovement;

    void Start()
    {
        _cam = Camera.main;
        _playerMovement = GetComponent<PlayerMovement>();

        // ค้นหาป้ายกด E อัตโนมัติใน Scene หากไม่ได้ลากใส่ใน Inspector
        if (pressEPrompt == null)
        {
            pressEPrompt = GameObject.Find("PromptContainer");
            if (pressEPrompt == null)
            {
                pressEPrompt = GameObject.Find("press E");
            }
            if (pressEPrompt != null)
            {
                Debug.Log($"[NPCInteraction] Automatically found pressEPrompt: {pressEPrompt.name}");
            }
        }

        // เริ่มต้นให้ปิดป้ายกด E ไว้ก่อน
        if (pressEPrompt) pressEPrompt.SetActive(false);

        // บังคับปิดกล่อง ChatPanel ทันทีตอนรันเกมเพื่อกันบั๊ก UI เปิดค้างตั้งแต่ต้น
        GameObject chatPanel = GameObject.Find("ChatPanel");
        if (chatPanel != null)
        {
            chatPanel.SetActive(false);
            Debug.Log("[NPCInteraction] Automatically closed ChatPanel on Start.");
        }
    }

    void Update()
    {
        // 1. ปุ่ม Escape (ESC) - ปิดแชททั้งหมดใน Scene ทันที ไม่ว่าสถานะสนทนาจะเป็นอย่างไร (กันบั๊ก UI เปิดค้าง)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseAllChatsInScene();
            return;
        }

        // 2. ตรวจสอบอัตโนมัติ: หากแชทถูกปิดจากปุ่ม UI (ไม่ได้ปิดผ่าน ESC) ให้ล้างสถานะและล็อกเมาส์กลับคืน
        if (_activeNPC != null && !_activeNPC.isChatActive)
        {
            CloseActiveChat();
        }

        // 3. ยิง Raycast เพื่อตรวจจับ NPC
        RaycastHit hit;
        bool hitNPC = Physics.Raycast(_cam.transform.position, _cam.transform.forward, out hit, interactRange);

        if (hitNPC && hit.collider.CompareTag("NPC"))
        {
            _hoveredNPC = hit.collider.GetComponent<NPCChatController>();

            if (_hoveredNPC == null) return;

            // แสดงป้าย "กด E" เฉพาะเวลาที่ยังไม่มีการเปิดแชทกับ NPC ตัวใดเลย
            if (pressEPrompt != null)
            {
                pressEPrompt.SetActive(_activeNPC == null);
            }

            // กดปุ่ม E เพื่อเริ่มต้นเปิดแชท (จะเปิดแชทได้ก็ต่อเมื่อไม่มีหน้าต่างแชทอื่นเปิดค้างอยู่)
            if (Input.GetKeyDown(interactKey) && _activeNPC == null)
            {
                OpenChat(_hoveredNPC);
            }
        }
        else
        {
            _hoveredNPC = null;
            // ถ้าไม่ได้เล็ง NPC ให้ปิดป้ายกด E
            if (pressEPrompt != null)
                pressEPrompt.SetActive(false);
        }
    }

    private void OpenChat(NPCChatController npc)
    {
        _activeNPC = npc;
        _activeNPC.OpenChat();
        if (_playerMovement) _playerMovement.SetCursorFree(true);
        if (pressEPrompt != null) pressEPrompt.SetActive(false);
    }

    public void CloseActiveChat()
    {
        if (_activeNPC != null)
        {
            if (_activeNPC.isChatActive)
            {
                _activeNPC.CloseChat();
            }
            _activeNPC = null;
        }
        if (_playerMovement) _playerMovement.SetCursorFree(false);
    }
    public void CloseAllChatsInScene()
    {
        // สั่งปิดหน้าต่างแชทของทุกตัวใน Scene เผื่อกรณีเริ่มเกมแล้วแชทเปิดอยู่ก่อน
        NPCChatController[] allNPCs = GameObject.FindObjectsOfType<NPCChatController>(true);
        foreach (var npc in allNPCs)
        {
            npc.CloseChat();
        }
        
        // ค้นหาและสั่งปิด ChatPanel โดยตรงเพื่อความปลอดภัย
        GameObject chatPanel = GameObject.Find("ChatPanel");
        if (chatPanel != null)
        {
            chatPanel.SetActive(false);
        }
        else
        {
            // ค้นหาแบบค้นหานอกกล่อง Canvas
            var canvas = GameObject.FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                Transform panel = canvas.transform.Find("ChatPanel");
                if (panel != null) panel.gameObject.SetActive(false);
            }
        }
        
        _activeNPC = null;
        if (_playerMovement) _playerMovement.SetCursorFree(false);
    }
}
