using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ตัวอย่างการใช้งาน Random Role Selector System
/// 🔴 สำคัญ: ไฟล์นี้เป็นตัวอย่างเท่านั้น
/// ถ้าคุณมี GameManager แล้ว ให้เพิ่มเมธอดเหล่านี้เข้าไปในคลาสของคุณ
/// </summary>
public class GameManagerExample : MonoBehaviour
{
    [SerializeField] private int playerCount = 4;
    [SerializeField] private RoleSelector roleSelector;

    private RoleManager roleManager;
    private Dictionary<int, Role> playerRoles = new Dictionary<int, Role>();

    private void Start()
    {
        roleManager = RoleManager.Instance;

        if (roleManager == null)
        {
            Debug.LogError("RoleManager ไม่พบ!");
            return;
        }

        // ตัวอย่าง 1: สุ่มบทบาทให้ผู้เล่นทั้งหมด
        // DistributeRolesToPlayers();

        // ตัวอย่าง 2: ดูบทบาททั้งหมด
        // ShowAllAvailableRoles();

        // ตัวอย่าง 3: ดึงบทบาทสุ่ม
        // GetRandomRole();
    }

    /// <summary>
    /// แจกบทบาทให้ผู้เล่นทั้งหมด
    /// </summary>
    public void DistributeRolesToPlayers()
    {
        Debug.Log($"กำลังแจกบทบาทให้ผู้เล่น {playerCount} คน...");

        List<Role> randomRoles = roleManager.GetRandomRolesForPlayers(playerCount);

        for (int i = 0; i < randomRoles.Count; i++)
        {
            roleManager.AssignRoleToPlayer(i, randomRoles[i]);
            playerRoles[i] = randomRoles[i];

            Debug.Log($"ผู้เล่น {i + 1}: {randomRoles[i].roleName}");
        }
    }

    /// <summary>
    /// แสดงบทบาททั้งหมดในเกม
    /// </summary>
    public void ShowAllAvailableRoles()
    {
        List<Role> allRoles = roleManager.GetAllRoles();

        Debug.Log("=== บทบาททั้งหมด ===");
        foreach (Role role in allRoles)
        {
            Debug.Log($"ชื่อ: {role.roleName}");
            Debug.Log($"ประเภท: {role.roleType}");
            Debug.Log($"คำอธิบาย: {role.description}");
            Debug.Log($"---");
        }
    }

    /// <summary>
    /// ดึงบทบาทสุ่มเดียว
    /// </summary>
    public void GetRandomRole()
    {
        Role randomRole = roleManager.GetRandomRole();
        Debug.Log($"บทบาทสุ่ม: {randomRole.roleName}");
    }

    /// <summary>
    /// ได้รับบทบาทของผู้เล่นที่ระบุ
    /// </summary>
    public Role GetPlayerRole(int playerId)
    {
        Role playerRole = roleManager.GetPlayerRole(playerId);
        if (playerRole != null)
        {
            Debug.Log($"บทบาทของผู้เล่น {playerId}: {playerRole.roleName}");
            return playerRole;
        }

        Debug.LogWarning($"ผู้เล่น {playerId} ยังไม่มีบทบาท");
        return null;
    }

    /// <summary>
    /// รีเซ็ตเกมและล้างบทบาท
    /// </summary>
    public void ResetGame()
    {
        roleManager.ClearPlayerRoles();
        playerRoles.Clear();

        if (roleSelector != null)
        {
            roleSelector.ClearSelection();
        }

        Debug.Log("รีเซ็ตเกมแล้ว");
    }

    /// <summary>
    /// ตรวจสอบว่ามีแวววูพในเกมหรือไม่
    /// </summary>
    public bool HasWerewolf()
    {
        foreach (var role in playerRoles.Values)
        {
            if (role.roleType == Role.RoleType.Werewolf)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// นับจำนวนผู้เล่นแต่ละบทบาท
    /// </summary>
    public void CountRoles()
    {
        int villagerCount = 0;
        int werewolfCount = 0;
        int seerCount = 0;

        foreach (var role in playerRoles.Values)
        {
            switch (role.roleType)
            {
                case Role.RoleType.Villager:
                    villagerCount++;
                    break;
                case Role.RoleType.Werewolf:
                    werewolfCount++;
                    break;
                case Role.RoleType.Seer:
                    seerCount++;
                    break;
            }
        }

        Debug.Log($"พลเมือง: {villagerCount}");
        Debug.Log($"แวววูพ: {werewolfCount}");
        Debug.Log($"ผู้ตัดสิน: {seerCount}");
    }
}
