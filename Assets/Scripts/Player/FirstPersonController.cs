using FokusTour.Input;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FokusTour.Player
{
    /// <summary>
    /// First-person movement for mobile (virtual joystick + look area).
    /// Keyboard/mouse fallback is available for testing in the Editor.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class FirstPersonController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CharacterController characterController;
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private VirtualJoystick moveJoystick;
        [SerializeField] private TouchLookArea lookArea;

        [Header("Movement")]
        [SerializeField] private float moveSpeed = 4f;
        [SerializeField] private float gravity = -15f;

        [Header("Look")]
        [SerializeField] private float lookSensitivity = 0.12f;
        [SerializeField] private float mouseSensitivity = 0.08f;
        [SerializeField] private float minPitch = -80f;
        [SerializeField] private float maxPitch = 80f;

        [Header("Editor Fallback")]
        [SerializeField] private bool enableKeyboardMouseFallback = true;

        private float _pitch;
        private float _yaw;
        private float _verticalVelocity;

        private void Awake()
        {
            if (characterController == null)
                characterController = GetComponent<CharacterController>();

            _yaw = transform.eulerAngles.y;
            if (cameraTransform != null)
                _pitch = cameraTransform.localEulerAngles.x;
        }

        private void Update()
        {
            Vector2 moveInput = ReadMoveInput();
            Vector2 lookInput = ReadLookInput();

            ApplyLook(lookInput);
            ApplyMovement(moveInput);
        }

        private Vector2 ReadMoveInput()
        {
            if (moveJoystick != null && moveJoystick.Value.sqrMagnitude > 0.0001f)
                return moveJoystick.Value;

            if (!enableKeyboardMouseFallback)
                return Vector2.zero;

            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
                return Vector2.zero;

            Vector2 input = Vector2.zero;
            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) input.y += 1f;
            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) input.y -= 1f;
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) input.x -= 1f;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) input.x += 1f;
            return Vector2.ClampMagnitude(input, 1f);
        }

        private Vector2 ReadLookInput()
        {
            if (lookArea != null && lookArea.IsDragging)
                return lookArea.ConsumeDelta() * lookSensitivity;

            if (!enableKeyboardMouseFallback)
                return Vector2.zero;

            Mouse mouse = Mouse.current;
            if (mouse == null || !mouse.rightButton.isPressed)
                return Vector2.zero;

            return mouse.delta.ReadValue() * mouseSensitivity;
        }

        private void ApplyLook(Vector2 lookInput)
        {
            if (lookInput.sqrMagnitude < 0.0001f)
                return;

            _yaw += lookInput.x;
            _pitch -= lookInput.y;
            _pitch = Mathf.Clamp(_pitch, minPitch, maxPitch);

            transform.rotation = Quaternion.Euler(0f, _yaw, 0f);
            if (cameraTransform != null)
                cameraTransform.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
        }

        private void ApplyMovement(Vector2 moveInput)
        {
            Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
            move *= moveSpeed;

            if (characterController.isGrounded && _verticalVelocity < 0f)
                _verticalVelocity = -2f;

            _verticalVelocity += gravity * Time.deltaTime;
            move.y = _verticalVelocity;

            characterController.Move(move * Time.deltaTime);
        }
    }
}
