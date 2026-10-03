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

    [Header("Crouch Settings")]
    [SerializeField] private KeyCode crouchKey = KeyCode.C;
    [SerializeField] private KeyCode crouchKeyAlt = KeyCode.LeftControl;
    [SerializeField] private float crouchHeight = 1.1f;
    [SerializeField] private float normalHeight = 2.0f;
    [SerializeField] private float crouchCenterY = -0.45f;
    [SerializeField] private float normalCenterY = 0f;
    [SerializeField] private float crouchTransitionSpeed = 12f;

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
    private PlayerHealth playerHealth;
    private Vector3 velocity;
    private float verticalLookRotation;

    private bool isCrouching = false;
    private float defaultCameraY = 0.8f;
    private float nextNoiseTime = 0f;

    public bool IsCrouching => isCrouching;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        skillController = GetComponent<PlayerSkillController>();
        playerHealth = GetComponent<PlayerHealth>();
        if (playerHealth == null) playerHealth = gameObject.AddComponent<PlayerHealth>();

        // Ensure tag is Player
        if (!gameObject.CompareTag("Player"))
        {
            try
            {
                gameObject.tag = "Player";
            }
            catch { }
        }

        // Auto-find the Animator on a child model if not manually assigned
        if (playerAnimator == null)
            playerAnimator = GetComponentInChildren<Animator>();

        if (cameraTransform == null)
        {
            Camera cam = GetComponentInChildren<Camera>();
            if (cam != null) cameraTransform = cam.transform;
        }

        if (cameraTransform != null)
        {
            defaultCameraY = cameraTransform.localPosition.y;
        }
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // --- Diegetic footstep audio setup ---
        if (footstepAudioSource == null)
            footstepAudioSource = gameObject.AddComponent<AudioSource>();

        footstepAudioSource.clip         = footstepClip;
        footstepAudioSource.playOnAwake  = false;
        footstepAudioSource.loop         = true;
        footstepAudioSource.spatialBlend = 1f;
        footstepAudioSource.rolloffMode  = AudioRolloffMode.Logarithmic;
        footstepAudioSource.minDistance  = 1f;
        footstepAudioSource.maxDistance  = 20f;
        footstepAudioSource.volume       = footstepVolume;
        footstepAudioSource.pitch        = walkPitch;
    }

    private void Update()
    {
        // Don't move/look while the game is paused or dead
        if (Time.timeScale == 0f) return;
        if (playerHealth != null && playerHealth.IsDead) return;

        HandleCrouchInput();
        HandleLook();
        HandleMove();
        HandleFootstepsAndNoise();
        HandleReturnToMenu();
    }

    private void HandleCrouchInput()
    {
        // Player can crouch with C or LeftControl
        bool crouchHeld = Input.GetKey(crouchKey) || Input.GetKey(crouchKeyAlt);
        isCrouching = crouchHeld;

        // Smooth transition of CharacterController height and center
        float targetHeight = isCrouching ? crouchHeight : normalHeight;
        float targetCenterY = isCrouching ? crouchCenterY : normalCenterY;

        controller.height = Mathf.Lerp(controller.height, targetHeight, Time.deltaTime * crouchTransitionSpeed);
        Vector3 curCenter = controller.center;
        curCenter.y = Mathf.Lerp(curCenter.y, targetCenterY, Time.deltaTime * crouchTransitionSpeed);
        controller.center = curCenter;

        // Smooth transition of Camera local Y
        if (cameraTransform != null)
        {
            Vector3 camLocalPos = cameraTransform.localPosition;
            float targetCamY = isCrouching ? defaultCameraY * 0.5f : defaultCameraY;
            camLocalPos.y = Mathf.Lerp(camLocalPos.y, targetCamY, Time.deltaTime * crouchTransitionSpeed);
            cameraTransform.localPosition = camLocalPos;
        }
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
        Time.timeScale = 1f;
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

        // Rotate only the camera up/down
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

        // Speed calculation:
        // "Player can now crouch but halves the move speed while removing the sound it makes while moving."
        float currentSpeed;
        if (isCrouching)
        {
            currentSpeed = moveSpeed * 0.5f; // Halved move speed
        }
        else if (Input.GetKey(KeyCode.LeftShift))
        {
            currentSpeed = runSpeed;
        }
        else
        {
            currentSpeed = moveSpeed;
        }

        controller.Move(move * currentSpeed * Time.deltaTime);

        // Drive animation Speed
        float inputMagnitude = Mathf.Clamp01(move.magnitude);
        float animSpeed = isCrouching ? inputMagnitude * 0.3f : (Input.GetKey(KeyCode.LeftShift) ? inputMagnitude : inputMagnitude * 0.5f);
        if (playerAnimator != null)
            playerAnimator.SetFloat("Speed", animSpeed, 0.1f, Time.deltaTime);

        if (Input.GetButtonDown("Jump") && !isCrouching)
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

    private void HandleFootstepsAndNoise()
    {
        bool isGrounded  = controller.isGrounded;
        float horizontal = Input.GetAxis("Horizontal");
        float vertical   = Input.GetAxis("Vertical");
        float inputMag   = Mathf.Abs(horizontal) + Mathf.Abs(vertical);
        bool isSprinting = Input.GetKey(KeyCode.LeftShift) && !isCrouching;

        // "Player can now crouch but halves the move speed while removing the sound it makes while moving."
        bool isMoving = isGrounded && inputMag > 0.1f;
        bool shouldPlayAudio = isMoving && !isCrouching;

        if (footstepAudioSource != null && footstepClip != null)
        {
            if (shouldPlayAudio)
            {
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

        // Noise emission:
        // Crouching: 0 noise (completely silent to Proximity AI!).
        // Walking: 8m audible noise.
        // Sprinting: 14m audible noise.
        if (isMoving && !isCrouching && Time.time >= nextNoiseTime)
        {
            float noiseRadius = isSprinting ? 14f : 8f;
            float interval = isSprinting ? 0.3f : 0.45f;
            nextNoiseTime = Time.time + interval;
            NoiseSystem.Emit(transform.position, noiseRadius, gameObject);
        }
    }
}