using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 2.5f;
    public float runSpeed = 5f;
    public float crouchSpeed = 1.2f;
    public float gravity = 9.8f;
    public float jumpHeight = 1.0f;
    private float currentSpeed;
    private Vector3 velocity;

    [Header("Stamina")]
    public float maxStamina = 5f;
    public float staminaDrain = 1f;
    public float staminaRegen = 0.5f;
    private float currentStamina;
    private bool isRunning = false;

    [Header("Crouch")]
    public float crouchHeight = 1f;
    public float normalHeight = 2f;
    private bool isCrouching = false;

    [Header("Health & Sanity")]
    public float maxHealth = 100f;
    public float maxSanity = 100f;
    public float sanityDrainPerSecond = 0.5f;
    public float darknessSanityMultiplier = 2f;
    private float currentHealth;
    private float currentSanity;

    [Header("Flashlight")]
    public Light flashlight;
    public float flickerIntensityMin = 0.8f;
    public float flickerIntensityMax = 1.2f;
    public float flickerSpeed = 0.05f;
    private bool flashlightOn = true;

    [Header("Interaction")]
    public float interactDistance = 2f;
    public LayerMask interactLayer;

    [Header("Mobile Controls")]
    public Joystick moveJoystick;
    public Button runButton;
    public Button crouchButton;
    public Button interactButton;
    public Button flashlightButton;

    private CharacterController controller;
    private Camera playerCamera;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        playerCamera = GetComponentInChildren<Camera>();
        currentHealth = maxHealth;
        currentSanity = maxSanity;
        currentStamina = maxStamina;

        if (runButton) runButton.onClick.AddListener(ToggleRun);
        if (crouchButton) crouchButton.onClick.AddListener(ToggleCrouch);
        if (interactButton) interactButton.onClick.AddListener(Interact);
        if (flashlightButton) flashlightButton.onClick.AddListener(ToggleFlashlight);

        if (flashlight) StartCoroutine(FlashlightFlicker());
    }

    void Update()
    {
        HandleMovement();
        HandleSanity();
    }

    void HandleMovement()
    {
        Vector3 move = new Vector3(moveJoystick.Horizontal, 0, moveJoystick.Vertical);
        move = playerCamera.transform.TransformDirection(move);
        move.y = 0f;
        move.Normalize();

        currentSpeed = walkSpeed;
        if (isRunning && currentStamina > 0 && !isCrouching)
        {
            currentSpeed = runSpeed;
            currentStamina -= staminaDrain * Time.deltaTime;
            if (currentStamina <= 0) isRunning = false;
        }
        else if (isCrouching)
        {
            currentSpeed = crouchSpeed;
        }
        else
        {
            currentStamina = Mathf.Min(currentStamina + staminaRegen * Time.deltaTime, maxStamina);
        }

        controller.Move(move * currentSpeed * Time.deltaTime);

        // Gravity
        if (!controller.isGrounded)
            velocity.y -= gravity * Time.deltaTime;
        else
            velocity.y = 0;
        controller.Move(velocity * Time.deltaTime);
    }

    void ToggleRun()
    {
        if (currentStamina > 0 && !isCrouching)
            isRunning = !isRunning;
    }

    void ToggleCrouch()
    {
        isCrouching = !isCrouching;
        controller.height = isCrouching ? crouchHeight : normalHeight;
    }

    void HandleSanity()
    {
        float drain = sanityDrainPerSecond * Time.deltaTime;
        if (!IsInLight()) drain *= darknessSanityMultiplier;
        currentSanity = Mathf.Max(currentSanity - drain, 0);
    }

    bool IsInLight()
    {
        // Simple check: if flashlight is on, consider player in light
        return flashlightOn && flashlight.enabled;
    }

    void ToggleFlashlight()
    {
        flashlightOn = !flashlightOn;
        flashlight.enabled = flashlightOn;
    }

    IEnumerator FlashlightFlicker()
    {
        while (true)
        {
            if (flashlightOn && flashlight)
            {
                flashlight.intensity = Random.Range(flickerIntensityMin, flickerIntensityMax);
                flashlight.enabled = (Random.value > 0.05f); // 5% chance to flicker off
            }
            yield return new WaitForSeconds(flickerSpeed);
        }
    }

    void Interact()
    {
        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, interactDistance, interactLayer))
        {
            var interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable != null)
                interactable.OnInteract();
        }
    }

    // Call this to apply damage
    public void TakeDamage(float amount)
    {
        currentHealth = Mathf.Max(currentHealth - amount, 0);
        if (currentHealth == 0) Die();
    }

    void Die()
    {
        // Implement game over logic here
        Debug.Log("You died.");
    }

    // Optional: expose current stats for UI
    public float GetHealth() => currentHealth;
    public float GetSanity() => currentSanity;
    public float GetStamina() => currentStamina;
}