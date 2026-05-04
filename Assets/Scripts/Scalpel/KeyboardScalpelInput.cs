using UnityEngine;

namespace Volyager.Scalpel
{
    /// <summary>
    /// Keyboard implementation of <see cref="IScalpelInput"/> using Unity's new Input System.
    /// Reuses PlaneInputActions for now — same bindings (WASDQE move, IJKL+UO rotate, R reset).
    /// Lock and Resize actions are ignored for the scalpel.
    /// </summary>
    public class KeyboardScalpelInput : MonoBehaviour, IScalpelInput
    {
        private PlaneInputActions _actions;
        private bool _resetPressed;

        public Vector3 MoveInput => _actions.Plane.Move.ReadValue<Vector3>();
        public Vector3 RotateInput => _actions.Plane.Rotate.ReadValue<Vector3>();
        public bool ResetPressed => _resetPressed;

        private void Awake()
        {
            _actions = new PlaneInputActions();
        }

        private void OnEnable()
        {
            _actions.Plane.Enable();
            _actions.Plane.Reset.performed += OnResetPerformed;
        }

        private void OnDisable()
        {
            _actions.Plane.Reset.performed -= OnResetPerformed;
            _actions.Plane.Disable();
        }

        private void OnDestroy()
        {
            _actions?.Dispose();
        }

        private void LateUpdate()
        {
            _resetPressed = false;
        }

        private void OnResetPerformed(UnityEngine.InputSystem.InputAction.CallbackContext _)
        {
            _resetPressed = true;
        }
    }
}