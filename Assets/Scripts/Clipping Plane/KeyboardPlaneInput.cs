using UnityEngine;

namespace Volyager.ClippingPlane
{
    /// <summary>
    /// Keyboard implementation of <see cref="IPlaneInput"/> using Unity's new Input System.
    /// Reads from the auto-generated <see cref="PlaneInputActions"/> wrapper class.
    ///
    /// Bindings (defined in PlaneInputActions.inputactions):
    ///   Move    — WASD + Q/E (Vector3)
    ///   Rotate  — IJKL + U/O (Vector3)
    ///   Resize  — Arrow keys (Vector2)
    ///   Lock    — Space (button)
    ///   Reset   — R (button)
    ///
    /// Attach to any GameObject in the scene. Only one instance should be active at a time.
    /// </summary>
    public class KeyboardPlaneInput : MonoBehaviour, IPlaneInput
    {
        /* The auto-generated wrapper. We instantiate it ourselves rather than using
         * PlayerInput because we don't need the GameObject-binding overhead here. */
        private PlaneInputActions _actions;

        /* One-frame edge flags for Lock and Reset. Set in the .performed callbacks,
         * cleared at end of frame in LateUpdate so any consumer reading them in their
         * Update() sees them exactly once. */
        private bool _lockTogglePressed;
        private bool _resetPressed;

        public Vector3 MoveInput => _actions.Plane.Move.ReadValue<Vector3>();
        public Vector3 RotateInput => _actions.Plane.Rotate.ReadValue<Vector3>();
        public Vector2 ResizeInput => _actions.Plane.Resize.ReadValue<Vector2>();
        public bool LockTogglePressed => _lockTogglePressed;
        public bool ResetPressed => _resetPressed;

        private void Awake()
        {
            _actions = new PlaneInputActions();
        }

        private void OnEnable()
        {
            _actions.Plane.Enable();
            _actions.Plane.Lock.performed += OnLockPerformed;
            _actions.Plane.Reset.performed += OnResetPerformed;
        }

        private void OnDisable()
        {
            _actions.Plane.Lock.performed -= OnLockPerformed;
            _actions.Plane.Reset.performed -= OnResetPerformed;
            _actions.Plane.Disable();
        }

        private void OnDestroy()
        {
            /* Dispose the action map fully — important when the scene unloads, otherwise
             * the input system can hold references to destroyed callbacks. */
            _actions?.Dispose();
        }

        private void LateUpdate()
        {
            /* Clear one-frame flags AFTER all consumers have had their Update().
             * Any controller polling LockTogglePressed in Update() will see true exactly
             * on the frame the key was pressed, then false thereafter. */
            _lockTogglePressed = false;
            _resetPressed = false;
        }

        // Edge-event callbacks fired by the Input System on button press.
        private void OnLockPerformed(UnityEngine.InputSystem.InputAction.CallbackContext _)
        {
            _lockTogglePressed = true;
        }

        private void OnResetPerformed(UnityEngine.InputSystem.InputAction.CallbackContext _)
        {
            _resetPressed = true;
        }
    }
}