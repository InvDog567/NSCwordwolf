using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// ========================================
// WEREWOLF GAME - RANDOM ROLE SELECTOR SYSTEM
// Unity C# 3D Ver.6.0
// ========================================

#region ==================== ROLE DATA CLASS ====================

/// <summary>
/// Data class สำหรับเก็บข้อมูลบทบาท
/// </summary>
public class Role
{
    public enum RoleType
    {
        Villager,      // พลเมือง
        Werewolf,      // แวววูพ
        Seer           // ผู้ตัดสิน
    }

    public string roleName;
    public RoleType roleType;
    public Color roleColor;
    public string description;
    public Sprite roleIcon;

    public Role(string name, RoleType type, Color color, string desc, Sprite icon = null)
    {
        roleName = name;
        roleType = type;
        roleColor = color;
        description = desc;
        roleIcon = icon;
    }
}

#endregion

#region ==================== ROLE MANAGER ====================

/// <summary>
/// จัดการบทบาท สุ่มบทบาท และเก็บข้อมูลบทบาทของผู้เล่น
/// </summary>
public class RoleManager : MonoBehaviour
{
    public static RoleManager Instance { get; private set; }

    [SerializeField] private List<Role> availableRoles = new List<Role>();
    private Dictionary<int, Role> playerRoles = new Dictionary<int, Role>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        InitializeRoles();
    }

    /// <summary>
    /// สร้างข้อมูลบทบาทเริ่มต้น
    /// </summary>
    private void InitializeRoles()
    {
        availableRoles.Clear();

        // พลเมือง
        availableRoles.Add(new Role(
            "พลเมือง",
            Role.RoleType.Villager,
            new Color(0.8f, 0.8f, 0.8f),
            "บทบาทปกติ ไม่มีความสามารถพิเศษ"
        ));

        // แวววูพ
        availableRoles.Add(new Role(
            "แวววูพ",
            Role.RoleType.Werewolf,
            new Color(1f, 0.2f, 0.2f),
            "สามารถกินผู้เล่นคนอื่นในตอนกลางคืนได้"
        ));

        // ผู้ตัดสิน
        availableRoles.Add(new Role(
            "ผู้ตัดสิน",
            Role.RoleType.Seer,
            new Color(0.2f, 0.5f, 1f),
            "สามารถดูบทบาทของผู้เล่นคนอื่นได้"
        ));
    }

    /// <summary>
    /// สุ่มบทบาทให้ผู้เล่น
    /// </summary>
    public Role GetRandomRole()
    {
        if (availableRoles.Count == 0)
        {
            Debug.LogError("ไม่มีบทบาทในรายการ!");
            return null;
        }

        int randomIndex = Random.Range(0, availableRoles.Count);
        return availableRoles[randomIndex];
    }

    /// <summary>
    /// สุ่มบทบาทให้ผู้เล่นหลายคนแบบไม่ซ้ำกัน
    /// </summary>
    public List<Role> GetRandomRolesForPlayers(int playerCount)
    {
        List<Role> randomRoles = new List<Role>();

        if (playerCount > availableRoles.Count)
        {
            Debug.LogWarning($"จำนวนผู้เล่น ({playerCount}) มากกว่าจำนวนบทบาท ({availableRoles.Count})");
            playerCount = availableRoles.Count;
        }

        // Shuffle บทบาท
        List<Role> shuffledRoles = availableRoles.OrderBy(x => Random.value).ToList();

        for (int i = 0; i < playerCount; i++)
        {
            randomRoles.Add(shuffledRoles[i]);
        }

        return randomRoles;
    }

    /// <summary>
    /// เก็บบทบาทของผู้เล่น
    /// </summary>
    public void AssignRoleToPlayer(int playerId, Role role)
    {
        playerRoles[playerId] = role;
        Debug.Log($"ผู้เล่น {playerId} ได้บทบาท: {role.roleName}");
    }

    /// <summary>
    /// ดึงบทบาทของผู้เล่น
    /// </summary>
    public Role GetPlayerRole(int playerId)
    {
        if (playerRoles.TryGetValue(playerId, out Role role))
        {
            return role;
        }
        return null;
    }

    /// <summary>
    /// ล้างข้อมูลบทบาททั้งหมด
    /// </summary>
    public void ClearPlayerRoles()
    {
        playerRoles.Clear();
        Debug.Log("ล้างข้อมูลบทบาททั้งหมดแล้ว");
    }

    /// <summary>
    /// ได้รับรายการบทบาททั้งหมด
    /// </summary>
    public List<Role> GetAllRoles()
    {
        return new List<Role>(availableRoles);
    }
}

#endregion

#region ==================== ROLE ANIMATOR ====================

/// <summary>
/// จัดการ Animation สำหรับการสุ่มบทบาท
/// </summary>
public class RoleAnimator : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float spinDuration = 2f;      // ระยะเวลาการหมุน
    [SerializeField] private float spinSpeed = 360f;       // ความเร็วการหมุน (องศา/วินาที)
    [SerializeField] private float scaleMultiplier = 1.2f; // ขนาดเมื่อเลือก
    [SerializeField] private float bounceHeight = 0.5f;    // ความสูงของการกระเด้ง

    private RectTransform rectTransform;
    private List<Role> rolesToAnimate;
    private Role selectedRole;
    private bool isAnimating = false;

    private Vector3 originalScale;
    private Vector3 originalPosition;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            rectTransform = gameObject.AddComponent<RectTransform>();
        }

        originalScale = transform.localScale;
        originalPosition = transform.localPosition;
    }

    /// <summary>
    /// เริ่มต้น Animation สำหรับสุ่มบทบาท
    /// </summary>
    public void PlayRoleSelectionAnimation(List<Role> roles, System.Action<Role> onSelectionComplete = null)
    {
        if (isAnimating)
        {
            Debug.LogWarning("Animation กำลังเล่นอยู่แล้ว!");
            return;
        }

        rolesToAnimate = roles;
        StartCoroutine(RoleSpinAnimation(onSelectionComplete));
    }

    /// <summary>
    /// Animation สุ่มบทบาทแบบหมุน
    /// </summary>
    private IEnumerator RoleSpinAnimation(System.Action<Role> onComplete)
    {
        isAnimating = true;
        float elapsedTime = 0f;
        int currentIndex = 0;

        // Phase 1: หมุนแบบสุ่ม
        while (elapsedTime < spinDuration)
        {
            elapsedTime += Time.deltaTime;
            
            // เปลี่ยนบทบาทอย่างรวดเร็ว
            if ((int)(elapsedTime * 10) % 2 == 0)
            {
                currentIndex = Random.Range(0, rolesToAnimate.Count);
                selectedRole = rolesToAnimate[currentIndex];
            }

            // Rotation animation
            transform.Rotate(0, 0, spinSpeed * Time.deltaTime);

            yield return null;
        }

        // Phase 2: ปรับรูปร่างและตำแหน่ง
        yield return StartCoroutine(SelectionBounceAnimation(selectedRole));

        isAnimating = false;
        onComplete?.Invoke(selectedRole);
    }

    /// <summary>
    /// Animation การกระเด้งเมื่อเลือก
    /// </summary>
    private IEnumerator SelectionBounceAnimation(Role role)
    {
        float elapsedTime = 0f;
        float bounceDuration = 0.6f;

        // ขยายขนาด
        while (elapsedTime < bounceDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / bounceDuration;

            // Ease out animation
            float easeProgress = 1f - Mathf.Pow(1f - progress, 3f);
            transform.localScale = Vector3.Lerp(originalScale, originalScale * scaleMultiplier, easeProgress);

            yield return null;
        }

        transform.localScale = originalScale * scaleMultiplier;

        // การกระเด้ง
        elapsedTime = 0f;
        float bounceDurationSmall = 0.4f;

        while (elapsedTime < bounceDurationSmall)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / bounceDurationSmall;

            // Bounce curve
            float bounceValue = Mathf.Sin(progress * Mathf.PI) * bounceHeight;
            transform.localPosition = originalPosition + new Vector3(0, bounceValue, 0);

            yield return null;
        }

        transform.localPosition = originalPosition;
    }

    /// <summary>
    /// Animation แบบโรตเตอร์ 3D (ถ้ามี 3D object)
    /// </summary>
    public void Play3DRoleSelectionAnimation(List<Role> roles, System.Action<Role> onSelectionComplete = null)
    {
        if (isAnimating)
        {
            Debug.LogWarning("Animation กำลังเล่นอยู่แล้ว!");
            return;
        }

        rolesToAnimate = roles;
        StartCoroutine(Role3DSpinAnimation(onSelectionComplete));
    }

    /// <summary>
    /// Animation 3D สุ่มบทบาท
    /// </summary>
    private IEnumerator Role3DSpinAnimation(System.Action<Role> onComplete)
    {
        isAnimating = true;
        float elapsedTime = 0f;
        int currentIndex = 0;

        // Spin animation
        while (elapsedTime < spinDuration)
        {
            elapsedTime += Time.deltaTime;

            // เปลี่ยนบทบาท
            if ((int)(elapsedTime * 8) % 2 == 0)
            {
                currentIndex = Random.Range(0, rolesToAnimate.Count);
                selectedRole = rolesToAnimate[currentIndex];

                // Rotate 3D object
                transform.rotation *= Quaternion.Euler(0, 45f, 0);
            }

            // Scale animation
            float scaleProgress = Mathf.Sin(elapsedTime * 5f) * 0.1f + 1f;
            transform.localScale = originalScale * scaleProgress;

            yield return null;
        }

        // Final animation
        for (int i = 0; i < 3; i++)
        {
            transform.rotation *= Quaternion.Euler(0, 90f, 0);
            yield return new WaitForSeconds(0.2f);
        }

        transform.localScale = originalScale;
        isAnimating = false;
        onComplete?.Invoke(selectedRole);
    }

    /// <summary>
    /// รีเซ็ต Animation
    /// </summary>
    public void ResetAnimation()
    {
        StopAllCoroutines();
        transform.localScale = originalScale;
        transform.localPosition = originalPosition;
        transform.rotation = Quaternion.identity;
        isAnimating = false;
    }

    /// <summary>
    /// ตรวจสอบว่า Animation กำลังเล่นอยู่หรือไม่
    /// </summary>
    public bool IsAnimating => isAnimating;

    /// <summary>
    /// ได้รับบทบาทที่เลือก
    /// </summary>
    public Role GetSelectedRole() => selectedRole;
}

#endregion

#region ==================== ROLE SELECTOR ====================

/// <summary>
/// ควบคุม UI สำหรับการเลือกบทบาท
/// </summary>
public class RoleSelector : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button selectRoleButton;
    [SerializeField] private Image roleDisplayImage;
    [SerializeField] private TextMeshProUGUI roleNameText;
    [SerializeField] private TextMeshProUGUI roleDescriptionText;
    [SerializeField] private Image roleColorIndicator;

    [Header("Animation References")]
    [SerializeField] private RoleAnimator roleAnimator;

    [Header("Settings")]
    [SerializeField] private bool autoSelectOnStart = false;

    private RoleManager roleManager;
    private List<Role> availableRoles;
    private Role currentSelectedRole;

    private void Awake()
    {
        roleManager = RoleManager.Instance;

        if (roleManager == null)
        {
            Debug.LogError("RoleManager ไม่พบ! กรุณาสร้าง RoleManager ก่อน");
            return;
        }

        availableRoles = roleManager.GetAllRoles();

        // ถ้าไม่มี RoleAnimator ลองหาจาก child objects
        if (roleAnimator == null)
        {
            roleAnimator = GetComponentInChildren<RoleAnimator>();
        }
    }

    private void Start()
    {
        // Setup button listener
        if (selectRoleButton != null)
        {
            selectRoleButton.onClick.AddListener(OnSelectRoleButtonClicked);
        }

        // Auto select เมื่อเริ่มต้น
        if (autoSelectOnStart)
        {
            OnSelectRoleButtonClicked();
        }
    }

    /// <summary>
    /// เมื่อกดปุ่มเลือกบทบาท
    /// </summary>
    private void OnSelectRoleButtonClicked()
    {
        if (roleAnimator == null)
        {
            Debug.LogError("RoleAnimator ไม่พบ!");
            return;
        }

        // ปิดการใช้งานปุ่มระหว่างสุ่ม
        if (selectRoleButton != null)
        {
            selectRoleButton.interactable = false;
        }

        // เริ่มต้น Animation
        roleAnimator.PlayRoleSelectionAnimation(availableRoles, OnRoleSelected);
    }

    /// <summary>
    /// เมื่อเลือกบทบาทเสร็จสิ้น
    /// </summary>
    private void OnRoleSelected(Role selectedRole)
    {
        currentSelectedRole = selectedRole;
        UpdateRoleDisplay(selectedRole);

        // เปิดการใช้งานปุ่มอีกครั้ง
        if (selectRoleButton != null)
        {
            selectRoleButton.interactable = true;
        }

        // เรียก Event หรือ Callback ที่นี่
        Debug.Log($"เลือกบทบาท: {selectedRole.roleName}");
    }

    /// <summary>
    /// อัปเดต UI เพื่อแสดงบทบาทที่เลือก
    /// </summary>
    private void UpdateRoleDisplay(Role role)
    {
        if (roleNameText != null)
        {
            roleNameText.text = role.roleName;
        }

        if (roleDescriptionText != null)
        {
            roleDescriptionText.text = role.description;
        }

        if (roleColorIndicator != null)
        {
            roleColorIndicator.color = role.roleColor;
        }

        if (roleDisplayImage != null && role.roleIcon != null)
        {
            roleDisplayImage.sprite = role.roleIcon;
        }
    }

    /// <summary>
    /// ได้รับบทบาทที่เลือก
    /// </summary>
    public Role GetCurrentSelectedRole()
    {
        return currentSelectedRole;
    }

    /// <summary>
    /// ดูบทบาททั้งหมด
    /// </summary>
    public void DisplayAllRoles()
    {
        Debug.Log("=== บทบาททั้งหมด ===");
        foreach (Role role in availableRoles)
        {
            Debug.Log($"- {role.roleName}: {role.description}");
        }
    }

    /// <summary>
    /// ล้างการเลือก
    /// </summary>
    public void ClearSelection()
    {
        currentSelectedRole = null;
        if (roleNameText != null)
            roleNameText.text = "ยังไม่มีการเลือก";
        if (roleDescriptionText != null)
            roleDescriptionText.text = "";

        if (roleAnimator != null)
        {
            roleAnimator.ResetAnimation();
        }
    }
}

#endregion

#region ==================== ADVANCED FEATURES ====================

// ========== 1. ParticleEffect Manager ==========
public class RoleParticleEffect : MonoBehaviour
{
    [SerializeField] private ParticleSystem particleSystem;
    [SerializeField] private List<ParticleSystem.ColorOverLifetimeModule> colorModules;

    private void Start()
    {
        if (particleSystem == null)
        {
            particleSystem = GetComponent<ParticleSystem>();
        }
    }

    /// <summary>
    /// เล่น Particle Effect ตามสีบทบาท
    /// </summary>
    public void PlayRoleEffect(Role role)
    {
        if (particleSystem == null)
        {
            Debug.LogWarning("ParticleSystem ไม่พบ!");
            return;
        }

        // ตั้งค่าสี particle ตามบทบาท
        var colorModule = particleSystem.colorOverLifetime;
        colorModule.color = new ParticleSystem.MinMaxGradient(role.roleColor);

        particleSystem.Play();
    }

    /// <summary>
    /// หยุด Particle Effect
    /// </summary>
    public void StopEffect()
    {
        if (particleSystem != null)
        {
            particleSystem.Stop();
        }
    }
}

// ========== 2. Sound Manager ==========
public class RoleSoundManager : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip spinStartSound;
    [SerializeField] private AudioClip spinEndSound;
    [SerializeField] private Dictionary<Role.RoleType, AudioClip> roleSelectSounds;

    private void Awake()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        // เริ่มต้น dictionary
        roleSelectSounds = new Dictionary<Role.RoleType, AudioClip>();
    }

    /// <summary>
    /// เล่นเสียง Start Spin
    /// </summary>
    public void PlaySpinStartSound()
    {
        if (spinStartSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(spinStartSound);
        }
    }

    /// <summary>
    /// เล่นเสียง End Spin
    /// </summary>
    public void PlaySpinEndSound()
    {
        if (spinEndSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(spinEndSound);
        }
    }

    /// <summary>
    /// เล่นเสียงตามบทบาท
    /// </summary>
    public void PlayRoleSound(Role role)
    {
        if (roleSelectSounds.TryGetValue(role.roleType, out AudioClip clip))
        {
            if (clip != null && audioSource != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }
    }

    /// <summary>
    /// ตั้งค่าเสียงสำหรับบทบาท
    /// </summary>
    public void SetRoleSound(Role.RoleType roleType, AudioClip clip)
    {
        roleSelectSounds[roleType] = clip;
    }
}

// ========== 3. Canvas Animation (Screen Shake Effect) ==========
public class ScreenShakeEffect : MonoBehaviour
{
    [SerializeField] private RectTransform canvasRectTransform;
    [SerializeField] private float shakeDuration = 0.3f;
    [SerializeField] private float shakeAmount = 10f;

    private Vector2 originalPosition;

    private void Start()
    {
        if (canvasRectTransform == null)
        {
            canvasRectTransform = GetComponent<RectTransform>();
        }

        originalPosition = canvasRectTransform.anchoredPosition;
    }

    /// <summary>
    /// เล่น Screen Shake Effect
    /// </summary>
    public void PlayShake()
    {
        StartCoroutine(ShakeCoroutine());
    }

    private IEnumerator ShakeCoroutine()
    {
        float elapsedTime = 0f;

        while (elapsedTime < shakeDuration)
        {
            elapsedTime += Time.deltaTime;

            float randomX = Random.Range(-shakeAmount, shakeAmount);
            float randomY = Random.Range(-shakeAmount, shakeAmount);

            canvasRectTransform.anchoredPosition = originalPosition + new Vector2(randomX, randomY);

            yield return null;
        }

        canvasRectTransform.anchoredPosition = originalPosition;
    }
}

// ========== 4. Timer System ==========
public class RoleSelectionTimer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private float timeLimit = 10f;

    private float remainingTime;
    private bool isRunning = false;

    /// <summary>
    /// เริ่มจับเวลา
    /// </summary>
    public void StartTimer()
    {
        remainingTime = timeLimit;
        isRunning = true;
    }

    /// <summary>
    /// หยุดจับเวลา
    /// </summary>
    public void StopTimer()
    {
        isRunning = false;
    }

    private void Update()
    {
        if (isRunning)
        {
            remainingTime -= Time.deltaTime;

            if (remainingTime <= 0)
            {
                remainingTime = 0;
                isRunning = false;
                OnTimeUp();
            }

            UpdateTimerDisplay();
        }
    }

    private void UpdateTimerDisplay()
    {
        if (timerText != null)
        {
            timerText.text = $"เวลา: {remainingTime:F1}s";
        }
    }

    private void OnTimeUp()
    {
        Debug.Log("หมดเวลา!");
    }
}

// ========== 5. Role Distribution System ==========
public class RoleDistributionSystem : MonoBehaviour
{
    /// <summary>
    /// แจกบทบาทแบบสุ่มให้ผู้เล่นแต่ละคน (ด้วยการแสดงผล)
    /// </summary>
    public void DistributeRolesWithAnimation(int playerCount, RoleAnimator animator, System.Action onComplete)
    {
        StartCoroutine(DistributeRolesCoroutine(playerCount, animator, onComplete));
    }

    private IEnumerator DistributeRolesCoroutine(int playerCount, RoleAnimator animator, System.Action onComplete)
    {
        RoleManager roleManager = RoleManager.Instance;
        List<Role> roles = roleManager.GetRandomRolesForPlayers(playerCount);

        for (int i = 0; i < roles.Count; i++)
        {
            roleManager.AssignRoleToPlayer(i, roles[i]);
            
            // แสดง Animation สำหรับแต่ละผู้เล่น
            if (animator != null)
            {
                animator.PlayRoleSelectionAnimation(roleManager.GetAllRoles());
                yield return new WaitForSeconds(3f); // รอให้ Animation เสร็จ
            }

            Debug.Log($"ผู้เล่น {i + 1} ได้บทบาท: {roles[i].roleName}");
            yield return new WaitForSeconds(0.5f);
        }

        onComplete?.Invoke();
    }
}

// ========== 6. Leaderboard/Stats System ==========
public class RoleStatistics : MonoBehaviour
{
    private Dictionary<Role.RoleType, int> roleCount = new Dictionary<Role.RoleType, int>();
    private Dictionary<Role.RoleType, int> roleWinCount = new Dictionary<Role.RoleType, int>();

    private void Start()
    {
        InitializeStats();
    }

    private void InitializeStats()
    {
        roleCount[Role.RoleType.Villager] = 0;
        roleCount[Role.RoleType.Werewolf] = 0;
        roleCount[Role.RoleType.Seer] = 0;

        roleWinCount[Role.RoleType.Villager] = 0;
        roleWinCount[Role.RoleType.Werewolf] = 0;
        roleWinCount[Role.RoleType.Seer] = 0;
    }

    /// <summary>
    /// บันทึกการเล่นแต่ละครั้ง
    /// </summary>
    public void RecordRolePlay(Role.RoleType roleType)
    {
        if (roleCount.ContainsKey(roleType))
        {
            roleCount[roleType]++;
        }
    }

    /// <summary>
    /// บันทึกการชนะ
    /// </summary>
    public void RecordWin(Role.RoleType roleType)
    {
        if (roleWinCount.ContainsKey(roleType))
        {
            roleWinCount[roleType]++;
        }
    }

    /// <summary>
    /// ได้รับ Win Rate
    /// </summary>
    public float GetWinRate(Role.RoleType roleType)
    {
        if (roleCount[roleType] == 0)
            return 0;

        return (float)roleWinCount[roleType] / roleCount[roleType] * 100f;
    }

    /// <summary>
    /// แสดงสถิติ
    /// </summary>
    public void PrintStatistics()
    {
        Debug.Log("=== สถิติบทบาท ===");
        foreach (var kvp in roleCount)
        {
            float winRate = GetWinRate(kvp.Key);
            Debug.Log($"{kvp.Key}: เล่น {kvp.Value} ครั้ง, ชนะ {roleWinCount[kvp.Key]} ครั้ง ({winRate:F1}%)");
        }
    }
}

#endregion

// ========================================
// END OF WEREWOLF ROLE SYSTEM
// ========================================