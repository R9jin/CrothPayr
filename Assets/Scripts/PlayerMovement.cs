using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float runSpeed = 10f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float jumpHeight = 1.2f;

    [Header("Look")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float lookUpLimit = 80f;

    [Header("Menu")]
    [Tooltip("Must exactly match the scene name in File > Build Settings")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Animation")]
    [SerializeField] private Animator playerAnimator;

    [Header("Footstep Audio")]
    [SerializeField] private AudioClip footstepClip;
    [SerializeField] private AudioSource footstepAudioSource;
    [SerializeField] [Range(0f, 1f)] private float footstepVolume = 0.7f;
    [SerializeField] private float walkPitch = 1f;    // pitch while walking
    [SerializeField] private float runPitch  = 1.35f; // pitch while sprinting

    private CharacterController controller;
    private PlayerSkillController skillController;
    private Vector3 velocity;
    private float verticalLookRotation;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        skillController = GetComponent<PlayerSkillController>();

        // Auto-find the Animator on a child model if not manually assigned
        if (playerAnimator == null)
            playerAnimator = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // --- Diegetic footstep audio setup ---
        // Use a looping AudioSource so the long walking track plays seamlessly.
        // No timer/PlayOneShot — just Play() when moving, Stop() when idle/airborne.
        if (footstepAudioSource == null)
            footstepAudioSource = gameObject.AddComponent<AudioSource>();

        footstepAudioSource.clip         = footstepClip;
        footstepAudioSource.playOnAwake  = false;
        footstepAudioSource.loop         = true;          // ← key: loop the track
        footstepAudioSource.spatialBlend = 1f;            // full 3D / diegetic
        footstepAudioSource.rolloffMode  = AudioRolloffMode.Logarithmic;
        footstepAudioSource.minDistance  = 1f;
        footstepAudioSource.maxDistance  = 20f;
        footstepAudioSource.volume       = footstepVolume;
        footstepAudioSource.pitch        = walkPitch;
    }

    private void Update()
    {
        // Don't move/look while the game is paused
        if (Time.timeScale == 0f) return;

        HandleLook();
        HandleMove();
        HandleFootsteps();
        HandleReturnToMenu();
    }

    private void HandleReturnToMenu()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            ReturnToMainMenu();
        }
    }

    private void ReturnToMainMenu()
    {
        Time.timeScale = 1f; // reset in case something paused it before quitting
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void HandleLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Rotate the whole player left/right
        transform.Rotate(Vector3.up * mouseX);

        // Rotate only the camera up/down, clamped so you can't flip over
        verticalLookRotation -= mouseY;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -lookUpLimit, lookUpLimit);
        if (cameraTransform != null)
            cameraTransform.localRotation = Quaternion.Euler(verticalLookRotation, 0f, 0f);
    }

    private void HandleMove()
    {
        bool isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
            velocity.y = -2f; // keeps the controller snapped to the ground

        float horizontal = Input.GetAxis("Horizontal"); // A/D
        float vertical   = Input.GetAxis("Vertical");   // W/S
        Vector3 move = transform.right * horizontal + transform.forward * vertical;

        // Hold Left Shift to run
        bool isSprinting = Input.GetKey(KeyCode.LeftShift);
        float currentSpeed = isSprinting ? runSpeed : moveSpeed;
        controller.Move(move * currentSpeed * Time.deltaTime);

        // Drive animation Speed (0 = idle, ~0.5 = walk, ~1 = run)
        float inputMagnitude = Mathf.Clamp01(move.magnitude);
        float animSpeed      = isSprinting ? inputMagnitude : inputMagnitude * 0.5f;
        if (playerAnimator != null)
            playerAnimator.SetFloat("Speed", animSpeed, 0.1f, Time.deltaTime);

        if (Input.GetButtonDown("Jump"))
        {
            if (isGrounded)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
            else if (skillController != null && skillController.TryDoubleJump())
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // Drive jump animation
        if (playerAnimator != null)
            playerAnimator.SetBool("IsJumping", !isGrounded);
    }

    private void HandleFootsteps()
    {
        if (footstepAudioSource == null || footstepClip == null) return;

        bool isGrounded  = controller.isGrounded;
        float horizontal = Input.GetAxis("Horizontal");
        float vertical   = Input.GetAxis("Vertical");
        float inputMag   = Mathf.Abs(horizontal) + Mathf.Abs(vertical);
        bool isSprinting = Input.GetKey(KeyCode.LeftShift);
        bool shouldPlay  = isGrounded && inputMag > 0.1f;

        if (shouldPlay)
        {
            // Shift pitch to match walk vs sprint cadence — no overlapping copies
            footstepAudioSource.pitch = isSprinting ? runPitch : walkPitch;

            if (!footstepAudioSource.isPlaying)
                footstepAudioSource.Play();
        }
        else
        {
            if (footstepAudioSource.isPlaying)
                footstepAudioSource.Stop();
        }
    }
}