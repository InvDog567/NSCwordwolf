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

    void Start()
    {
        playerRole = GetComponent<PlayerRole>();
    }

    void Update()
    {
        if (playerRole.currentRole ==
            PlayerRole.Role.Villager)
        {
            infoText.text = "";
            return;
        }

        Ray ray = playerCamera.ViewportPointToRay(
            new Vector3(0.5f, 0.5f));

        if (Physics.Raycast(ray,
            out RaycastHit hit,
            interactDistance))
        {
            PlayerRole target =
                hit.collider.GetComponent<PlayerRole>();

            if (target != null &&
                !target.isPlayer &&
                !target.isDead)
            {
                // =====================
                // SEER
                // =====================

                if (playerRole.currentRole ==
                    PlayerRole.Role.Seer)
                {
                    if (Input.GetKey(KeyCode.E))
                    {
                        holdTimer += Time.deltaTime;

                        infoText.text =
                            "Revealing...";

                        if (holdTimer >= holdTime)
                        {
                            infoText.text =
                                target.currentRole.ToString();
                        }
                    }
                    else
                    {
                        holdTimer = 0;

                        infoText.text =
                            "Hold E to reveal";
                    }
                }

                // =====================
                // WEREWOLF
                // =====================

                else if (playerRole.currentRole ==
                         PlayerRole.Role.Werewolf)
                {
                    if (Input.GetKey(KeyCode.E))
                    {
                        holdTimer += Time.deltaTime;

                        infoText.text =
                            "Killing...";

                        if (holdTimer >= holdTime)
                        {
                            target.isDead = true;

                            // Save death
                            if (GameManager.Instance != null)
                            {
                                if (target.npcIndex >= 0 &&
                                    target.npcIndex <
                                    GameManager.Instance.npcAlive.Count)
                                {
                                    GameManager.Instance
                                    .npcAlive[target.npcIndex]
                                    = false;
                                }
                            }

                            infoText.text = "Killed";

                            Destroy(target.gameObject);

                            holdTimer = 0;
                        }
                    }
                    else
                    {
                        holdTimer = 0;

                        infoText.text =
                            "Hold E to kill";
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
}