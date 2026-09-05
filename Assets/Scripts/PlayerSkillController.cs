using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerSkillController : MonoBehaviour
{
    [Header("Character Config")]
    [SerializeField] private bool overrideCharacter = false;
    [SerializeField] private CharacterType debugCharacter = CharacterType.CharacterA_Roll;

    [Header("Character A: Roll")]
    [SerializeField] private int maxRollCharges = 2;
    [SerializeField] private float rollRechargeTime = 4.0f;
    [SerializeField] private float rollSpeed = 16.0f;
    [SerializeField] private float rollDuration = 0.35f;

    [Header("Character C: Teleport")]
    [SerializeField] private float teleportDistance = 8.0f;
    [SerializeField] private float teleportCooldown = 5.0f;

    private CharacterType activeCharacter;
    private CharacterController controller;
    private PlayerMovement playerMovement;

    // Roll state
    private int currentRollCharges;
    private float rollRechargeTimer;
    private bool isRolling;
    private Vector3 rollDirection;

    // Double Jump state
    private bool hasDoubleJumped;

    // Teleport state
    private float teleportCooldownTimer;

    public CharacterType ActiveCharacter => activeCharacter;
    public bool IsRolling => isRolling;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        playerMovement = GetComponent<PlayerMovement>();

        if (overrideCharacter)
        {
            activeCharacter = debugCharacter;
        }
        else
        {
            activeCharacter = CharacterSelection.SelectedCharacter;
        }

        currentRollCharges = maxRollCharges;
        rollRechargeTimer = 0f;
        teleportCooldownTimer = 0f;
    }

    private void Update()
    {
        if (Time.timeScale == 0f) return;

        UpdateRechargeTimers();
        HandleSkillInput();
        UpdateUIStatus();
    }

    private void UpdateRechargeTimers()
    {
        // Character A Roll Recharge
        if (activeCharacter == CharacterType.CharacterA_Roll)
        {
            if (currentRollCharges < maxRollCharges)
            {
                rollRechargeTimer += Time.deltaTime;
                if (rollRechargeTimer >= rollRechargeTime)
                {
                    currentRollCharges++;
                    rollRechargeTimer = 0f;
                }
            }
            else
            {
                rollRechargeTimer = 0f;
            }
        }

        // Character C Teleport Cooldown
        if (activeCharacter == CharacterType.CharacterC_Teleport)
        {
            if (teleportCooldownTimer > 0f)
            {
                teleportCooldownTimer -= Time.deltaTime;
                if (teleportCooldownTimer < 0f) teleportCooldownTimer = 0f;
            }
        }

        // Reset double jump when grounded
        if (activeCharacter == CharacterType.CharacterB_DoubleJump && controller.isGrounded)
        {
            hasDoubleJumped = false;
        }
    }

    private void HandleSkillInput()
    {
        switch (activeCharacter)
        {
            case CharacterType.CharacterA_Roll:
                if ((Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.LeftControl)) && currentRollCharges > 0 && !isRolling)
                {
                    ExecuteRoll();
                }
                break;

            case CharacterType.CharacterC_Teleport:
                if ((Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.E)) && teleportCooldownTimer <= 0f)
                {
                    ExecuteTeleport();
                }
                break;
        }
    }

    private void ExecuteRoll()
    {
        currentRollCharges--;
        if (rollRechargeTimer <= 0f)
        {
            rollRechargeTimer = 0.001f;
        }

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 inputDir = (transform.right * h + transform.forward * v).normalized;

        if (inputDir.sqrMagnitude > 0.01f)
        {
            rollDirection = inputDir;
        }
        else
        {
            rollDirection = transform.forward;
        }

        StartCoroutine(RollRoutine());
    }

    private IEnumerator RollRoutine()
    {
        isRolling = true;
        float elapsed = 0f;

        while (elapsed < rollDuration)
        {
            elapsed += Time.deltaTime;
            controller.Move(rollDirection * rollSpeed * Time.deltaTime);
            yield return null;
        }

        isRolling = false;
    }

    public bool TryDoubleJump()
    {
        if (activeCharacter == CharacterType.CharacterB_DoubleJump && !controller.isGrounded && !hasDoubleJumped)
        {
            hasDoubleJumped = true;
            return true;
        }
        return false;
    }

    private void ExecuteTeleport()
    {
        teleportCooldownTimer = teleportCooldown;

        Camera cam = Camera.main;
        Vector3 lookDir = cam != null ? cam.transform.forward : transform.forward;
        lookDir.y = 0f;
        lookDir.Normalize();

        Vector3 startPos = transform.position + Vector3.up * 0.5f;
        Vector3 targetPos = transform.position + lookDir * teleportDistance;

        // Check for obstructions
        RaycastHit hit;
        if (Physics.SphereCast(startPos, 0.4f, lookDir, out hit, teleportDistance, ~LayerMask.GetMask("Ignore Raycast")))
        {
            targetPos = hit.point - lookDir * 0.5f;
            targetPos.y = transform.position.y;
        }

        controller.enabled = false;
        transform.position = targetPos;
        controller.enabled = true;
    }

    private void UpdateUIStatus()
    {
        UIManager ui = UIManager.Instance;
        if (ui == null) return;

        string skillName = CharacterSelection.GetSkillName(activeCharacter);
        string status = "";

        switch (activeCharacter)
        {
            case CharacterType.CharacterA_Roll:
                if (currentRollCharges < maxRollCharges)
                {
                    float remaining = Mathf.Max(0f, rollRechargeTime - rollRechargeTimer);
                    status = $"Charges: {currentRollCharges}/{maxRollCharges} (Recharge: {remaining:0.0}s) [Q/Ctrl]";
                }
                else
                {
                    status = $"Charges: {currentRollCharges}/{maxRollCharges} [Q/Ctrl]";
                }
                break;

            case CharacterType.CharacterB_DoubleJump:
                status = hasDoubleJumped ? "Double Jump: Used [Space in air]" : "Double Jump: READY [Space in air]";
                break;

            case CharacterType.CharacterC_Teleport:
                if (teleportCooldownTimer > 0f)
                {
                    status = $"Cooldown: {teleportCooldownTimer:0.0}s";
                }
                else
                {
                    status = "READY [Q/E]";
                }
                break;
        }

        ui.UpdateSkillHUD(skillName, status);
    }
}
