using UnityEngine;
using UnityEngine.InputSystem;

public class PrototypePlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float sprintSpeed = 9f;
    [SerializeField] private float gravity = -18f;
    [SerializeField] private float lookSensitivity = 0.16f;
    [SerializeField] private float fixedCameraHeight = 1.7f;

    [Header("Interaction")]
    [SerializeField] private float interactionDistance = 4f;
    [SerializeField] private float pushForce = 15f;
    [SerializeField] private float throwForce = 16f;
    [SerializeField] private float holdDistance = 2.4f;
    [SerializeField] private float holdLerpSpeed = 14f;

    private CharacterController controller;
    private Camera playerCamera;
    private Transform playerRoot;
    private Rigidbody heldBody;
    private float verticalVelocity;
    private float pitch;
    private float yaw;

    private void Awake()
    {
        playerCamera = GetComponent<Camera>();
        SetupPlayerRoot();

        if (playerCamera != null)
        {
            playerCamera.orthographic = false;
            playerCamera.fieldOfView = 75f;
            playerCamera.nearClipPlane = 0.03f;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void SetupPlayerRoot()
    {
        CharacterController existingController = GetComponent<CharacterController>();
        if (existingController != null)
        {
            Destroy(existingController);
        }

        playerRoot = new GameObject("PlayerRoot").transform;
        playerRoot.position = transform.position;
        playerRoot.rotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);

        transform.SetParent(playerRoot);
        transform.localPosition = new Vector3(0f, fixedCameraHeight, 0f);
        transform.localRotation = Quaternion.identity;

        controller = playerRoot.gameObject.AddComponent<CharacterController>();
        controller.height = 2.05f;
        controller.radius = 0.32f;
        controller.center = new Vector3(0f, 1.03f, 0f);
        controller.stepOffset = 0.25f;
        controller.slopeLimit = 50f;

        yaw = playerRoot.eulerAngles.y;
    }

    private void Update()
    {
        UpdateLook();
        UpdateMovement();
        UpdateInteraction();
        UpdateHeldObject();
        KeepCameraOffset();

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private void UpdateLook()
    {
        Mouse mouse = Mouse.current;
        if (Cursor.lockState != CursorLockMode.Locked)
        {
            if (mouse != null && mouse.leftButton.wasPressedThisFrame)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            return;
        }

        if (mouse == null)
        {
            return;
        }

        Vector2 mouseDelta = mouse.delta.ReadValue();
        float yaw = mouseDelta.x * lookSensitivity;
        float pitchDelta = mouseDelta.y * lookSensitivity;

        pitch = Mathf.Clamp(pitch - pitchDelta, -75f, 80f);
        this.yaw += yaw;

        if (playerRoot != null)
        {
            playerRoot.rotation = Quaternion.Euler(0f, this.yaw, 0f);
        }

        transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    private void UpdateMovement()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return;
        }

        float horizontal = 0f;
        if (keyboard.aKey.isPressed)
        {
            horizontal -= 1f;
        }

        if (keyboard.dKey.isPressed)
        {
            horizontal += 1f;
        }

        float vertical = 0f;
        if (keyboard.sKey.isPressed)
        {
            vertical -= 1f;
        }

        if (keyboard.wKey.isPressed)
        {
            vertical += 1f;
        }

        Vector3 forward = playerRoot != null ? playerRoot.forward : transform.forward;
        Vector3 right = playerRoot != null ? playerRoot.right : transform.right;
        Vector3 input = (right * horizontal + forward * vertical).normalized;
        float currentSpeed = keyboard.leftShiftKey.isPressed ? sprintSpeed : moveSpeed;
        Vector3 move = input * currentSpeed;

        if (controller.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }

        verticalVelocity += gravity * Time.deltaTime;
        move.y = verticalVelocity;

        controller.Move(move * Time.deltaTime);
    }

    private void KeepCameraOffset()
    {
        if (playerRoot == null)
        {
            return;
        }

        transform.localPosition = new Vector3(0f, fixedCameraHeight, 0f);
    }

    private void UpdateInteraction()
    {
        Mouse mouse = Mouse.current;
        Keyboard keyboard = Keyboard.current;
        if (mouse == null || keyboard == null)
        {
            return;
        }

        if (mouse.rightButton.wasPressedThisFrame && heldBody != null)
        {
            ThrowHeldObject();
            return;
        }

        if (!Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit hit, interactionDistance))
        {
            if (keyboard.eKey.wasPressedThisFrame)
            {
                ReleaseHeldObject();
            }

            return;
        }

        Rigidbody hitBody = hit.rigidbody;

        if (mouse.leftButton.wasPressedThisFrame && hitBody != null)
        {
            hitBody.AddForce(playerCamera.transform.forward * pushForce, ForceMode.Impulse);
            hitBody.AddTorque(Random.insideUnitSphere * pushForce * 0.45f, ForceMode.Impulse);
        }

        if (keyboard.eKey.wasPressedThisFrame)
        {
            if (heldBody == hitBody)
            {
                ReleaseHeldObject();
            }
            else if (hitBody != null)
            {
                GrabObject(hitBody);
            }
        }
    }

    private void GrabObject(Rigidbody body)
    {
        if (body == null)
        {
            return;
        }

        ReleaseHeldObject();
        heldBody = body;
        heldBody.useGravity = false;
        heldBody.linearDamping = 5f;
        heldBody.angularDamping = 5f;
    }

    private void UpdateHeldObject()
    {
        if (heldBody == null)
        {
            return;
        }

        Vector3 targetPosition = playerCamera.transform.position + playerCamera.transform.forward * holdDistance;
        Vector3 direction = targetPosition - heldBody.worldCenterOfMass;
        heldBody.linearVelocity = direction * holdLerpSpeed;
        heldBody.angularVelocity *= 0.82f;

        if (Vector3.Distance(playerCamera.transform.position, heldBody.worldCenterOfMass) > interactionDistance + 2f)
        {
            ReleaseHeldObject();
        }
    }

    private void ThrowHeldObject()
    {
        if (heldBody == null)
        {
            return;
        }

        Rigidbody body = heldBody;
        ReleaseHeldObject();
        body.AddForce(playerCamera.transform.forward * throwForce, ForceMode.Impulse);
        body.AddTorque(Random.insideUnitSphere * throwForce * 0.35f, ForceMode.Impulse);
    }

    private void ReleaseHeldObject()
    {
        if (heldBody == null)
        {
            return;
        }

        heldBody.useGravity = true;
        heldBody.linearDamping = 0.15f;
        heldBody.angularDamping = 0.05f;
        heldBody = null;
    }

    private void OnGUI()
    {
        GUI.color = Color.black;
        GUI.Box(new Rect(16f, 16f, 520f, 88f), string.Empty);
        GUI.color = Color.white;
        GUI.Label(new Rect(28f, 26f, 500f, 24f), "WASD move  Shift sprint  Mouse look");
        GUI.Label(new Rect(28f, 46f, 500f, 24f), "Left click push  E grab/release  Right click throw");
        GUI.Label(new Rect(28f, 66f, 500f, 24f), "Goal: leave the cafeteria, pass the sauna corridor, and find the men's restroom.");
    }
}
