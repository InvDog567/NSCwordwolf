using UnityEngine;
using TMPro;

public class NightAbility : MonoBehaviour
{
    public Camera playerCamera;

    [Header("UI")]
    public TMP_Text infoText;

    [Header("Witch UI")]
    public GameObject witchChoicePanel;
    public TMP_Text witchChoiceInfoText;
    public KeyCode witchKillKey = KeyCode.Q;
    public KeyCode witchProtectKey = KeyCode.E;

    [Header("Settings")]
    public float interactDistance = 5f;
    public float holdTime = 2f;

    private float holdTimerE;
    private float holdTimerQ;
    private PlayerRole playerRole;
    private bool usedAbility = false;
    private bool showingRole = false;
    private GameObject pendingDisable = null;
    private bool justExecuted = false;

    private bool witchActedThisNight = false;
    private bool waitingForWitchChoice = false;
    private PlayerRole witchTarget = null;

    private bool arsonistActedThisNight = false;

    void Start()
    {
        playerRole = GetComponent<PlayerRole>();

        if (GameManager.Instance != null)
            GameManager.Instance.ResetNightState();

        if (witchChoicePanel != null)
            witchChoicePanel.SetActive(false);
    }

    void Update()
    {
        if (pendingDisable != null)
        {
            pendingDisable.SetActive(false);
            pendingDisable = null;
        }

        if (justExecuted)
        {
            justExecuted = false;
            return;
        }

        if (playerRole == null) return;

        if (waitingForWitchChoice)
        {
            HandleWitchChoice();
            return;
        }

        if (playerRole.currentRole == PlayerRole.Role.Villager ||
            playerRole.currentRole == PlayerRole.Role.Gunner ||
            playerRole.currentRole == PlayerRole.Role.Vigilante)
        {
            infoText.text = "";
            return;
        }

        if (playerRole.currentRole == PlayerRole.Role.Jailer)
        {
            HandleJailerNight();
            return;
        }

        if (showingRole) return;

        Ray ray = playerCamera.ViewportPointToRay(
            new Vector3(0.5f, 0.5f));

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance))
        {
            PlayerRole target =
                hit.collider.GetComponent<PlayerRole>();

            if (target == null)
                target = hit.collider
                    .GetComponentInParent<PlayerRole>();

            if (target != null && !target.isPlayer && !target.isDead)
            {
                if (playerRole.currentRole == PlayerRole.Role.Seer)
                    HandleSeer(target);
                else if (playerRole.currentRole == PlayerRole.Role.Werewolf)
                    HandleWerewolf(target);
                else if (playerRole.currentRole == PlayerRole.Role.Doctor)
                    HandleDoctor(target);
                else if (playerRole.currentRole == PlayerRole.Role.Arsonist)
                    HandleArsonist(target);
                else if (playerRole.currentRole == PlayerRole.Role.Witch)
                    HandleWitchTarget(target);
            }
            else
            {
                HandleNoTarget();
            }
        }
        else
        {
            HandleNoTarget();
        }
    }

    void HandleSeer(PlayerRole target)
    {
        if (usedAbility)
        {
            infoText.text = "Ability already used";
            return;
        }

        if (Input.GetKey(KeyCode.E))
        {
            holdTimerE += Time.deltaTime;
            infoText.text = "Revealing...";

            if (holdTimerE >= holdTime)
            {
                showingRole = true;
                usedAbility = true;
                holdTimerE = 0;
                infoText.text = "Role: " + target.currentRole.ToString();
            }
        }
        else
        {
            holdTimerE = 0;
            infoText.text = "Hold E to reveal";
        }
    }

    void HandleWerewolf(PlayerRole target)
    {
        if (usedAbility)
        {
            infoText.text = "Ability already used";
            return;
        }

        if (target.currentRole == PlayerRole.Role.Werewolf)
        {
            infoText.text = "This is your ally";
            holdTimerE = 0;
            return;
        }

        bool targetIsJailed = false;
        if (GameManager.Instance != null)
            targetIsJailed = target.npcIndex ==
                GameManager.Instance.jailedNPCIndex;

        if (targetIsJailed)
        {
            infoText.text = "This player is in jail";
            holdTimerE = 0;
            return;
        }

        if (Input.GetKey(KeyCode.E))
        {
            holdTimerE += Time.deltaTime;
            infoText.text = "Killing...";

            if (holdTimerE >= holdTime)
            {
                bool isProtected = false;
                if (GameManager.Instance != null)
                    isProtected = target.npcIndex ==
                        GameManager.Instance.doctorProtectedIndex;

                if (isProtected)
                {
                    infoText.text = "Target was protected!";
                    usedAbility = true;
                    holdTimerE = 0;
                    return;
                }

                if (GameManager.Instance != null)
                    GameManager.Instance.npcAlive[target.npcIndex] = false;

                usedAbility = true;
                holdTimerE = 0;
                infoText.text = "Killed";
                target.isDead = true;
                pendingDisable = target.gameObject;

                // --- WITNESS CHECK (Player kill) ---
                if (GameManager.Instance != null)
                {
                    PlayerRole[] allRoles = FindObjectsOfType<PlayerRole>();
                    foreach (var pr in allRoles)
                    {
                        if (pr.npcIndex == -1 || pr.npcIndex == target.npcIndex || pr.isDead) continue;
                        if (!pr.gameObject.activeInHierarchy) continue;

                        var nightBehavior = pr.GetComponent<NpcNightBehavior>();
                        float detectRadius = (nightBehavior != null) ? nightBehavior.detectionRadius : 15f;

                        float dist = Vector3.Distance(pr.transform.position, target.transform.position);
                        if (dist <= detectRadius)
                        {
                            bool canSee = true;
                            if (nightBehavior != null && nightBehavior.requireLineOfSight)
                            {
                                Vector3 dir = (target.transform.position - pr.transform.position).normalized;
                                if (Physics.Raycast(pr.transform.position + Vector3.up, dir, dist, nightBehavior.sightObstacles))
                                {
                                    canSee = false;
                                }
                            }

                            if (canSee)
                            {
                                if (!GameManager.Instance.witnessedMurderers.Contains(-1))
                                {
                                    GameManager.Instance.witnessedMurderers.Add(-1);
                                }
                                Debug.Log($"[WITNESS] NPC {pr.npcIndex} witnessed PLAYER murdering NPC {target.npcIndex}!");
                            }
                        }
                    }
                }
                // ---------------------
            }
        }
        else
        {
            holdTimerE = 0;
            infoText.text = "Hold E to kill";
        }
    }

    void HandleDoctor(PlayerRole target)
    {
        if (usedAbility)
        {
            infoText.text = "Already protecting someone";
            return;
        }

        if (Input.GetKey(KeyCode.E))
        {
            holdTimerE += Time.deltaTime;
            infoText.text = "Protecting...";

            if (holdTimerE >= holdTime)
            {
                GameManager.Instance.doctorProtectedIndex = target.npcIndex;
                usedAbility = true;
                holdTimerE = 0;
                infoText.text = "Protected!";
            }
        }
        else
        {
            holdTimerE = 0;
            infoText.text = "Hold E to protect";
        }
    }

    void HandleArsonist(PlayerRole target)
    {
        if (arsonistActedThisNight)
        {
            infoText.text = "Already acted this night";
            return;
        }

        bool anyDoused = false;
        for (int i = 0; i < GameManager.Instance.npcDoused.Count; i++)
        {
            if (GameManager.Instance.npcDoused[i])
            {
                anyDoused = true;
                break;
            }
        }

        if (anyDoused)
            infoText.text = "Hold E to douse\nHold Q to ignite";
        else
            infoText.text = "Hold E to douse";

        if (Input.GetKey(KeyCode.E) && !Input.GetKey(KeyCode.Q))
        {
            holdTimerE += Time.deltaTime;
            infoText.text = "Dousing...";

            if (holdTimerE >= holdTime)
            {
                GameManager.Instance.npcDoused[target.npcIndex] = true;
                arsonistActedThisNight = true;
                holdTimerE = 0;
                holdTimerQ = 0;
                infoText.text = "Doused!";
                Debug.Log("Arsonist doused NPC " + target.npcIndex);
            }
        }
        else if (Input.GetKey(KeyCode.Q) && !Input.GetKey(KeyCode.E) && anyDoused)
        {
            holdTimerQ += Time.deltaTime;
            infoText.text = "Igniting...";

            if (holdTimerQ >= holdTime)
            {
                GameManager.Instance.IgniteAllDoused();
                arsonistActedThisNight = true;
                holdTimerQ = 0;
                holdTimerE = 0;
                infoText.text = "Ignited!";
            }
        }
        else
        {
            if (!Input.GetKey(KeyCode.E)) holdTimerE = 0;
            if (!Input.GetKey(KeyCode.Q)) holdTimerQ = 0;
        }
    }

    void HandleWitchTarget(PlayerRole target)
    {
        if (GameManager.Instance.isFirstNight)
        {
            infoText.text = "Cannot use poison on first night";
            return;
        }

        if (witchActedThisNight)
        {
            infoText.text = "Already acted this night";
            return;
        }

        bool killUsed = GameManager.Instance.witchUsedKill;
        bool protectUsed = GameManager.Instance.witchUsedProtect;

        if (killUsed && protectUsed)
        {
            infoText.text = "Both potions used";
            return;
        }

        if (Input.GetKey(KeyCode.E))
        {
            holdTimerE += Time.deltaTime;
            infoText.text = "Selecting target...";

            if (holdTimerE >= holdTime)
            {
                witchTarget = target;
                waitingForWitchChoice = true;
                holdTimerE = 0;
                holdTimerQ = 0;
                infoText.text = "";

                if (witchChoicePanel != null)
                    witchChoicePanel.SetActive(true);

                if (witchChoiceInfoText != null)
                {
                    string killText = killUsed
                        ? "[Kill used]" : "[Q] Kill";
                    string protectText = protectUsed
                        ? "[Protect used]" : "[E] Protect";

                    witchChoiceInfoText.text =
                        killText + "  |  " + protectText;
                }
            }
        }
        else
        {
            holdTimerE = 0;
            infoText.text = "Hold E to select target";
        }
    }

    void HandleWitchChoice()
    {
        if (Input.GetKeyDown(witchKillKey) &&
            !GameManager.Instance.witchUsedKill)
        {
            GameManager.Instance.npcAlive[witchTarget.npcIndex] = false;
            GameManager.Instance.witchUsedKill = true;
            witchActedThisNight = true;
            witchTarget.isDead = true;
            pendingDisable = witchTarget.gameObject;
            CloseWitchPanel("Poisoned!");
        }
        else if (Input.GetKeyDown(witchProtectKey) &&
                 !GameManager.Instance.witchUsedProtect)
        {
            GameManager.Instance.doctorProtectedIndex = witchTarget.npcIndex;
            GameManager.Instance.witchUsedProtect = true;
            witchActedThisNight = true;
            CloseWitchPanel("Protected!");
        }
    }

    void CloseWitchPanel(string message)
    {
        waitingForWitchChoice = false;
        witchTarget = null;
        holdTimerE = 0;
        holdTimerQ = 0;

        if (witchChoicePanel != null)
            witchChoicePanel.SetActive(false);

        infoText.text = message;
    }

        void HandleNoTarget()
    {
        if (playerRole.currentRole == PlayerRole.Role.Doctor && !usedAbility)
        {
            if (Input.GetKey(KeyCode.E))
            {
                holdTimerE += Time.deltaTime;
                infoText.text = "Protecting yourself...";

                if (holdTimerE >= holdTime)
                {
                    GameManager.Instance.doctorProtectedPlayer = true;
                    usedAbility = true;
                    holdTimerE = 0;
                    infoText.text = "You are protected!";
                }
            }
            else
            {
                holdTimerE = 0;
                infoText.text = "Hold E to protect yourself";
            }
        }
        else
        {
            infoText.text = "";
            holdTimerE = 0;
            holdTimerQ = 0;
        }
    }

    void HandleJailerNight()
    {
        if (GameManager.Instance == null) return;

        int jailed = GameManager.Instance.jailedNPCIndex;

        if (jailed == -1)
        {
            infoText.text = "No one in jail tonight";
            return;
        }

        if (GameManager.Instance.jailerUsedBullet)
        {
            infoText.text = "NPC " + jailed +
                            " is jailed (no bullet left)";
            return;
        }

        infoText.text = "NPC " + jailed +
                        " is jailed. Hold E to execute.";

        if (Input.GetKey(KeyCode.E))
        {
            holdTimerE += Time.deltaTime;

            if (holdTimerE >= holdTime)
            {
                GameManager.Instance.npcAlive[jailed] = false;
                holdTimerE = 0;
                justExecuted = true;
                GameManager.Instance.jailerUsedBullet = true;

                StartCoroutine(
                    ShowThenClear("Executed", 1.5f));

                Debug.Log("Jailer executed NPC " + jailed);
            }
        }
        else
        {
            holdTimerE = 0;
        }
    }

    System.Collections.IEnumerator ShowThenClear(string message, float seconds)
    {
        infoText.text = message;
        yield return new WaitForSeconds(seconds);

        if (infoText.text == message)
            infoText.text = "";
    }
}