using UnityEngine;
using TMPro;

public class NightAbility : MonoBehaviour
{
    public Camera playerCamera;

    [Header("UI")]
    public TMP_Text infoText;

    [Header("Settings")]
    public float interactDistance = 5f;
    public float holdTime = 2f;

    private float holdTimer;
    private PlayerRole playerRole;
    private bool usedAbility = false;
    private bool showingRole = false;
    private GameObject pendingDisable = null;
    private bool justExecuted = false;

    void Start()
    {
        playerRole = GetComponent<PlayerRole>();

        if (GameManager.Instance != null)
            GameManager.Instance.ResetNightState();
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

        if (playerRole.currentRole == PlayerRole.Role.Villager ||
            playerRole.currentRole == PlayerRole.Role.Gunner)
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

            if (target != null && !target.isPlayer && !target.isDead)
            {
                if (playerRole.currentRole == PlayerRole.Role.Seer)
                {
                    if (usedAbility)
                    {
                        infoText.text = "Ability already used";
                        return;
                    }

                    if (Input.GetKey(KeyCode.E))
                    {
                        holdTimer += Time.deltaTime;
                        infoText.text = "Revealing...";

                        if (holdTimer >= holdTime)
                        {
                            showingRole = true;
                            usedAbility = true;
                            holdTimer = 0;
                            infoText.text = "Role: " +
                                target.currentRole.ToString();
                        }
                    }
                    else
                    {
                        holdTimer = 0;
                        infoText.text = "Hold E to reveal";
                    }
                }
                else if (playerRole.currentRole == PlayerRole.Role.Werewolf)
                {
                    if (usedAbility)
                    {
                        infoText.text = "Ability already used";
                        return;
                    }

                    if (target.currentRole == PlayerRole.Role.Werewolf)
                    {
                        infoText.text = "This is your ally";
                        holdTimer = 0;
                        return;
                    }

                    bool targetIsJailed = false;
                    if (GameManager.Instance != null)
                        targetIsJailed = target.npcIndex ==
                            GameManager.Instance.jailedNPCIndex;

                    if (targetIsJailed)
                    {
                        infoText.text = "This player is in jail";
                        holdTimer = 0;
                        return;
                    }

                    if (Input.GetKey(KeyCode.E))
                    {
                        holdTimer += Time.deltaTime;
                        infoText.text = "Killing...";

                        if (holdTimer >= holdTime)
                        {
                            bool isProtected = false;
                            if (GameManager.Instance != null)
                                isProtected = target.npcIndex ==
                                    GameManager.Instance.doctorProtectedIndex;

                            if (isProtected)
                            {
                                infoText.text = "Target was protected!";
                                usedAbility = true;
                                holdTimer = 0;
                                return;
                            }

                            if (GameManager.Instance != null)
                            {
                                GameManager.Instance
                                    .npcAlive[target.npcIndex] = false;
                            }

                            usedAbility = true;
                            holdTimer = 0;
                            infoText.text = "Killed";
                            target.isDead = true;
                            pendingDisable = target.gameObject;
                        }
                    }
                    else
                    {
                        holdTimer = 0;
                        infoText.text = "Hold E to kill";
                    }
                }
                else if (playerRole.currentRole == PlayerRole.Role.Doctor)
                {
                    if (usedAbility)
                    {
                        infoText.text = "Already protecting someone";
                        return;
                    }

                    if (Input.GetKey(KeyCode.E))
                    {
                        holdTimer += Time.deltaTime;
                        infoText.text = "Protecting...";

                        if (holdTimer >= holdTime)
                        {
                            GameManager.Instance
                                .doctorProtectedIndex = target.npcIndex;
                            usedAbility = true;
                            holdTimer = 0;
                            infoText.text = "Protected!";
                        }
                    }
                    else
                    {
                        holdTimer = 0;
                        infoText.text = "Hold E to protect";
                    }
                }
            }
            else
            {
                if (playerRole.currentRole == PlayerRole.Role.Doctor
                    && !usedAbility)
                {
                    infoText.text = "Hold E to protect yourself";

                    if (Input.GetKey(KeyCode.E))
                    {
                        holdTimer += Time.deltaTime;

                        if (holdTimer >= holdTime)
                        {
                            GameManager.Instance
                                .doctorProtectedPlayer = true;
                            usedAbility = true;
                            holdTimer = 0;
                            infoText.text = "You are protected!";
                        }
                    }
                    else
                    {
                        holdTimer = 0;
                    }
                }
                else
                {
                    infoText.text = "";
                    holdTimer = 0;
                }
            }
        }
        else
        {
            if (playerRole.currentRole == PlayerRole.Role.Doctor
                && !usedAbility)
            {
                infoText.text = "Hold E to protect yourself";

                if (Input.GetKey(KeyCode.E))
                {
                    holdTimer += Time.deltaTime;

                    if (holdTimer >= holdTime)
                    {
                        GameManager.Instance
                            .doctorProtectedPlayer = true;
                        usedAbility = true;
                        holdTimer = 0;
                        infoText.text = "You are protected!";
                    }
                }
                else
                {
                    holdTimer = 0;
                }
            }
            else
            {
                infoText.text = "";
                holdTimer = 0;
            }
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
            holdTimer += Time.deltaTime;

            if (holdTimer >= holdTime)
            {
                GameManager.Instance.npcAlive[jailed] = false;
                holdTimer = 0;
                infoText.text = "Executed";
                justExecuted = true;
                Debug.Log("Jailer executed NPC " + jailed);
                GameManager.Instance.jailerUsedBullet = true;
            }
        }
        else
        {
            holdTimer = 0;
        }
    }
}