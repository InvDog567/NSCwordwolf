using System.Collections.Generic;
using UnityEngine;

public class DiscussionNpcProfile
{
    public int npcIndex;
    public string npcName;
    public string developerPrompt;
}

public class DiscussionRoster : MonoBehaviour
{
    public static DiscussionRoster Instance { get; private set; }

    private static readonly string[] NpcNames =
    {
        "Thomas", "Emily", "Arthur", "Clara", "Samuel", "Lily",
        "George", "Anna", "Henry", "Daniel", "Jack", "Victor"
    };

    private readonly Dictionary<int, DiscussionNpcProfile> profiles =
        new Dictionary<int, DiscussionNpcProfile>();

    public static void Register(NPCChatController controller)
    {
        if (controller == null)
            return;

        PlayerRole role = controller.GetComponent<PlayerRole>();
        if (role == null || role.isPlayer || role.npcIndex < 0)
            return;

        DiscussionRoster roster = GetOrCreate();
        roster.profiles[role.npcIndex] = new DiscussionNpcProfile
        {
            npcIndex = role.npcIndex,
            npcName = GetFixedNpcName(role.npcIndex),
            developerPrompt = controller.BuildDiscussionDeveloperPrompt()
        };
    }

    public List<DiscussionNpcProfile> GetAliveProfiles()
    {
        var result = new List<DiscussionNpcProfile>();

        foreach (DiscussionNpcProfile profile in profiles.Values)
        {
            if (GameManager.Instance != null &&
                (profile.npcIndex >= GameManager.Instance.npcAlive.Count ||
                 !GameManager.Instance.npcAlive[profile.npcIndex]))
            {
                continue;
            }

            result.Add(profile);
        }

        return result;
    }

    public string GetNpcName(int npcIndex)
    {
        if (npcIndex == -1)
            return "the player";

        return profiles.TryGetValue(npcIndex, out DiscussionNpcProfile profile)
            ? profile.npcName
            : GetFixedNpcName(npcIndex);
    }

    public static string GetFixedNpcName(int npcIndex)
    {
        return npcIndex >= 0 && npcIndex < NpcNames.Length
            ? NpcNames[npcIndex]
            : "an unknown villager";
    }

    private static DiscussionRoster GetOrCreate()
    {
        if (Instance != null)
            return Instance;

        var rosterObject = new GameObject("DiscussionRoster");
        Instance = rosterObject.AddComponent<DiscussionRoster>();
        DontDestroyOnLoad(rosterObject);
        return Instance;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }
}
