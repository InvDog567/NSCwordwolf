using UnityEngine;
using TMPro;

public class DayAbility : MonoBehaviour
{
    public Camera playerCamera;

    [Header("UI")]
    public TMP_Text infoText;
    public TMP_Text gunnerBulletsText;

    [Header("Settings")]
    public float interactDistance = 5f;
    public float holdTime = 2f;

    private float holdTimer;
    private PlayerRole playerRole;
    private bool justJailed = false;

    void Start()
    {
        playerRole = GetComponent<PlayerRole>();

        if (GameManager.Instance != null)
            GameManager.Instance.ResetDayState();
    }

    void Update()
    {
        if (playerRole == null) return;

        if (justJailed)
        {
            justJailed = false;
            return;
        }

        if (playerRole.currentRole != PlayerRole.Role.Gunner &&
            playerRole.currentRole != PlayerRole.Role.Jailer)
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

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance))
        {
            PlayerRole target =
                hit.collider.GetComponent<PlayerRole>();

            if (target != null && !target.isPlayer && !target.isDead)
            {
                if (playerRole.currentRole == PlayerRole.Role.Gunner)
                {
                    if (GameManager.Instance.gunnerBulletsLeft <= 0)
                    {
                        infoText.text = "No bullets left";
                        return;
                    }

                    if (Input.GetKey(KeyCode.E))
                    {
                        holdTimer += Time.deltaTime;
                        infoText.text = "Shooting...";

                        if (holdTimer >= holdTime)
                        {
                            holdTimer = 0;
                            ShootGunner(target);
                        }
                    }
                    else
                    {
                        holdTimer = 0;
                        infoText.text = "Hold E to shoot (" +
                            GameManager.Instance.gunnerBulletsLeft +
                            " bullets)";
                    }
                }
                else if (playerRole.currentRole == PlayerRole.Role.Jailer)
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
                        holdTimer = 0;
                        return;
                    }

                    if (Input.GetKey(KeyCode.E))
                    {
                        holdTimer += Time.deltaTime;
                        infoText.text = "Selecting for jail...";

                        if (holdTimer >= holdTime)
                        {
                            GameManager.Instance.SetJailTarget(target.npcIndex);
                            holdTimer = 0;
                            infoText.text = "Will be jailed tonight";
                            justJailed = true;
                        }
                    }
                    else
                    {
                        holdTimer = 0;
                        infoText.text = "Hold E to jail tonight";
                    }
                }
            }
            else
            {
                infoText.text = "";
                holdTimer = 0;
            }
        }
        else
        {
            infoText.text = "";
            holdTimer = 0;
        }
    }

    void ShootGunner(PlayerRole target)
    {
        GameManager.Instance.gunnerBulletsLeft--;
        GameManager.Instance.npcAlive[target.npcIndex] = false;
        target.isDead = true;
        target.gameObject.SetActive(false);

        infoText.text = "Shot!";
        Debug.Log("Gunner shot NPC " + target.npcIndex +
                  " | Bullets left: " +
                  GameManager.Instance.gunnerBulletsLeft);

        int result = GameManager.Instance.CheckWinCondition();
        Debug.Log("Win check after shot: " + result);
    }
}