using UnityEngine;
using TMPro;

public class DayAbility : MonoBehaviour
{
    public Camera playerCamera;

    [Header("UI")]
    public TMP_Text infoText;
    public TMP_Text gunnerBulletsText;

    [Header("Vigilante UI")]
    public GameObject vigilanteChoicePanel;
    public TMP_Text vigilanteChoiceInfoText;
    public KeyCode vigilanteShootKey = KeyCode.Q;
    public KeyCode vigilanteRevealKey = KeyCode.E;

    [Header("Settings")]
    public float interactDistance = 5f;
    public float holdTime = 2f;

    [Header("Result Display Time")]
    public float resultDisplayTime = 3f;

    private float holdTimerE;
    private float holdTimerQ;
    private PlayerRole playerRole;
    private bool justJailed = false;

    private bool vigilanteActedThisDay = false;
    private bool waitingForVigilanteChoice = false;
    private bool vigilanteChoiceReady = false;
    private int vigilanteChoiceReadyFrames = 0;
    private PlayerRole vigilanteTarget = null;

    private bool showingResult = false;
    private float resultTimer = 0f;
    private string resultMessage = "";

    void Start()
    {
        playerRole = GetComponent<PlayerRole>();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetDayState();

            if (NPCRoleLogic.Instance != null)
                NPCRoleLogic.Instance.RunDayActions();
        }

        if (vigilanteChoicePanel != null)
            vigilanteChoicePanel.SetActive(false);
    }

    void Update()
    {
        if (playerRole == null) return;

        if (showingResult)
        {
            infoText.text = resultMessage;
            resultTimer -= Time.deltaTime;

            if (resultTimer <= 0)
            {
                showingResult = false;
                resultMessage = "";
                infoText.text = "";
            }
            return;
        }

        if (justJailed)
        {
            justJailed = false;
            return;
        }

        if (waitingForVigilanteChoice)
        {
            HandleVigilanteChoice();
            return;
        }

        if (playerRole.currentRole != PlayerRole.Role.Gunner &&
            playerRole.currentRole != PlayerRole.Role.Jailer &&
            playerRole.currentRole != PlayerRole.Role.Vigilante)
        {
            if (infoText != null) infoText.text = "";
            if (gunnerBulletsText != null) gunnerBulletsText.text = "";
            return;
        }

        if (gunnerBulletsText != null &&
            playerRole.currentRole == PlayerRole.Role.Gunner)
        {
            gunnerBulletsText.text = "Bullets: " +
                GameManager.Instance.gunnerBulletsLeft;
        }

        Ray ray = playerCamera.ViewportPointToRay(
            new Vector3(0.5f, 0.5f));

        bool hitSomething = Physics.Raycast(
            ray, out RaycastHit hit, interactDistance);

        if (hitSomething)
        {
            PlayerRole target =
                hit.collider.GetComponent<PlayerRole>() ??
                hit.collider.GetComponentInParent<PlayerRole>();

            if (target != null && !target.isPlayer && !target.isDead)
            {
                if (playerRole.currentRole == PlayerRole.Role.Gunner)
                    HandleGunner(target);
                else if (playerRole.currentRole == PlayerRole.Role.Jailer)
                    HandleJailer(target);
                else if (playerRole.currentRole == PlayerRole.Role.Vigilante)
                    HandleVigilanteTarget(target);
            }
            else
            {
                ClearUI();
            }
        }
        else
        {
            ClearUI();
        }
    }

    void ClearUI()
    {
        infoText.text = "";
        holdTimerE = 0;
        holdTimerQ = 0;
    }

    void ShowResult(string message)
    {
        showingResult = true;
        resultMessage = message;
        resultTimer = resultDisplayTime;
        infoText.text = message;
    }

    void HandleGunner(PlayerRole target)
    {
        if (GameManager.Instance.gunnerBulletsLeft <= 0)
        {
            infoText.text = "No bullets left";
            return;
        }

        if (GameManager.Instance.gunnerShotThisDay)
        {
            infoText.text = "Already shot today";
            return;
        }

        if (Input.GetKey(KeyCode.E))
        {
            holdTimerE += Time.deltaTime;
            infoText.text = "Shooting...";

            if (holdTimerE >= holdTime)
            {
                holdTimerE = 0;
                ShootGunner(target);
                GameManager.Instance.gunnerShotThisDay = true;
            }
        }
        else
        {
            holdTimerE = 0;
            infoText.text = "Hold E to shoot (" +
                GameManager.Instance.gunnerBulletsLeft + " bullets)";
        }
    }

    void HandleJailer(PlayerRole target)
    {
        if (GameManager.Instance.jailedNPCIndex != -1)
        {
            infoText.text = "Jail target already selected";
            return;
        }

        bool onCooldown =
            !GameManager.Instance.CanBeJailed(target.npcIndex);

        if (onCooldown)
        {
            infoText.text = "Cannot jail same person twice in a row";
            holdTimerE = 0;
            return;
        }

        if (Input.GetKey(KeyCode.E))
        {
            holdTimerE += Time.deltaTime;
            infoText.text = "Selecting for jail...";

            if (holdTimerE >= holdTime)
            {
                GameManager.Instance.SetJailTarget(target.npcIndex);
                holdTimerE = 0;
                infoText.text = "Will be jailed tonight";
                justJailed = true;
            }
        }
        else
        {
            holdTimerE = 0;
            infoText.text = "Hold E to jail tonight";
        }
    }

    void HandleVigilanteTarget(PlayerRole target)
    {
        if (vigilanteActedThisDay)
            return;

        bool shootUsed = GameManager.Instance.vigilanteUsedShoot;
        bool revealUsed = GameManager.Instance.vigilanteUsedReveal;

        if (shootUsed && revealUsed)
        {
            infoText.text = "Both abilities used";
            return;
        }

        if (Input.GetKey(KeyCode.E))
        {
            holdTimerE += Time.deltaTime;
            infoText.text = "Selecting target...";

            if (holdTimerE >= holdTime)
            {
                vigilanteTarget = target;
                waitingForVigilanteChoice = true;
                vigilanteChoiceReady = false;
                vigilanteChoiceReadyFrames = 0;

                holdTimerE = 0;
                holdTimerQ = 0;
                infoText.text = "";

                if (vigilanteChoicePanel != null)
                    vigilanteChoicePanel.SetActive(true);

                if (vigilanteChoiceInfoText != null)
                {
                    string shootText = shootUsed ? "[Shoot used]" : "[Q] Shoot";
                    string revealText = revealUsed ? "[Reveal used]" : "[E] Reveal Role";

                    vigilanteChoiceInfoText.text =
                        shootText + "  |  " + revealText;
                }
            }
        }
        else
        {
            holdTimerE = 0;
            infoText.text = "Hold E to select";
        }
    }

    void HandleVigilanteChoice()
    {
        if (!vigilanteChoiceReady)
        {
            vigilanteChoiceReadyFrames++;

            if (vigilanteChoiceReadyFrames < 3)
                return;

            vigilanteChoiceReady = true;
        }

        bool shootUsed = GameManager.Instance.vigilanteUsedShoot;
        bool revealUsed = GameManager.Instance.vigilanteUsedReveal;

        if (Input.GetKeyUp(vigilanteShootKey) && !shootUsed)
        {
            int idx = vigilanteTarget.npcIndex;

            GameManager.Instance.npcAlive[idx] = false;
            GameManager.Instance.vigilanteUsedShoot = true;
            vigilanteActedThisDay = true;

            vigilanteTarget.isDead = true;
            vigilanteTarget.gameObject.SetActive(false);

            CloseVigilantePanel("");
            ShowResult("Shot!");
        }
        else if (Input.GetKeyUp(vigilanteRevealKey) && !revealUsed)
        {
            string role = vigilanteTarget.currentRole.ToString();

            GameManager.Instance.vigilanteUsedReveal = true;
            vigilanteActedThisDay = true;

            CloseVigilantePanel("");
            ShowResult("Role: " + role);
        }
    }

    void CloseVigilantePanel(string message)
    {
        waitingForVigilanteChoice = false;
        vigilanteChoiceReady = false;
        vigilanteChoiceReadyFrames = 0;

        holdTimerE = 0;
        holdTimerQ = 0;

        if (vigilanteChoicePanel != null)
            vigilanteChoicePanel.SetActive(false);

        if (!string.IsNullOrEmpty(message))
            infoText.text = message;
    }

    void ShootGunner(PlayerRole target)
    {
        GameManager.Instance.gunnerBulletsLeft--;
        GameManager.Instance.npcAlive[target.npcIndex] = false;

        target.isDead = true;
        target.gameObject.SetActive(false);

        ShowResult("Shot!");

        GameManager.Instance.CheckWinCondition();
    }
}