using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public float walkSpeed = 3f;
    public float runSpeed = 6f;
    public float crouchSpeed = 1.5f;
    public float stamina = 5f, maxStamina = 5f, staminaRegen = 1f;
    public Transform cameraTransform;
    public float crouchHeight = 1f, standHeight = 2f;
    public float gravity = -9.81f;
    private CharacterController controller;
    private float verticalVelocity;
    private bool isRunning, isCrouched;
    private MobileInput input;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        input = FindObjectOfType<MobileInput>();
    }

    void Update()
    {
        Vector2 moveInput = input.GetMovement();
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        float speed = walkSpeed;

        isRunning = input.IsRunning() && stamina > 0 && !isCrouched;
        if (isRunning) speed = runSpeed;
        if (isRunning) stamina -= Time.deltaTime;
        else stamina = Mathf.Min(stamina + staminaRegen * Time.deltaTime, maxStamina);

        if (input.CrouchPressed())
        {
            isCrouched = !isCrouched;
            controller.height = isCrouched ? crouchHeight : standHeight;
            speed = isCrouched ? crouchSpeed : speed;
        }

        if (controller.isGrounded) verticalVelocity = -2f;
        else verticalVelocity += gravity * Time.deltaTime;

        Vector3 velocity = move * speed;
        velocity.y = verticalVelocity;
        controller.Move(velocity * Time.deltaTime);

        float lookX = input.GetLook().x;
        float lookY = input.GetLook().y;
        transform.Rotate(0, lookX, 0);
        cameraTransform.Rotate(-lookY, 0, 0);

        if (input.InteractPressed())
        {
            Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, 2f))
            {
                var interactable = hit.collider.GetComponent<Interactable>();
                if (interactable != null) interactable.Interact();
            }
        }
    }
}