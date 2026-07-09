// Assets/Scripts/VillageSpawnManager.cs
using UnityEngine;
using System.Collections.Generic;

public class VillageSpawnManager : MonoBehaviour
{
    [Header("=== Player ===")]
    public GameObject player;

    [Header("=== Spawn Points ===")]
    public Transform spawnPoint_Bartender;
    public Transform spawnPoint_Blacksmith;
    public Transform spawnPoint_Woodcutter;
    public Transform spawnPoint_Carpenter;
    public Transform spawnPoint_Doctor;
    public Transform spawnPoint_Herbalist;
    public Transform spawnPoint_Default;

    private Dictionary<string, Transform> spawnMap;

    void Awake()
    {
        spawnMap = new Dictionary<string, Transform>
        {
            { "SpawnPoint_Bartender",  spawnPoint_Bartender  },
            { "SpawnPoint_Blacksmith", spawnPoint_Blacksmith },
            { "SpawnPoint_Woodcutter", spawnPoint_Woodcutter },
            { "SpawnPoint_Carpenter",  spawnPoint_Carpenter  },
            { "SpawnPoint_Doctor",     spawnPoint_Doctor     },
            { "SpawnPoint_Herbalist",  spawnPoint_Herbalist  },
        };
    }

    void Start()
    {
        SpawnPlayer();
    }

    void SpawnPlayer()
    {
        if (player == null)
        {
            Debug.LogError("Player ไม่ได้ผูกใน Inspector!");
            return;
        }

        // ดึงข้อมูลจาก MiniGameManager (Singleton ที่ DontDestroyOnLoad)
        if (MiniGameManager.Instance == null)
        {
            Debug.LogWarning("ไม่พบ MiniGameManager ใช้ Default SpawnPoint แทน");
            SpawnAt(spawnPoint_Default);
            return;
        }

        string targetSpawn = MiniGameManager.Instance.SelectedMiniGame?.spawnPointName;

        Debug.Log($"Village loaded | Spawn target: {targetSpawn}");

        if (!string.IsNullOrEmpty(targetSpawn) && spawnMap.ContainsKey(targetSpawn))
        {
            SpawnAt(spawnMap[targetSpawn]);
        }
        else
        {
            Debug.LogWarning($"SpawnPoint '{targetSpawn}' ไม่เจอ ใช้ Default แทน");
            SpawnAt(spawnPoint_Default);
        }
    }

    void SpawnAt(Transform spawnPoint)
    {
        if (spawnPoint == null)
        {
            Debug.LogError("SpawnPoint เป็น null!");
            return;
        }

        player.transform.position = spawnPoint.position;
        player.transform.rotation = spawnPoint.rotation;

        Debug.Log($"Player spawned at: {spawnPoint.name} | Pos: {spawnPoint.position}");
    }

    // เรียกได้ภายนอกถ้าต้อง Respawn
    public void RespawnPlayer() => SpawnPlayer();
}