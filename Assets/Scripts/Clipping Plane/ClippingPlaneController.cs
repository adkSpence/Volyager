using UnityEngine;

namespace Volyager.ClippingPlane
{
    /// <summary>
    /// Drives a clipping plane Transform from any <see cref="IPlaneInput"/> source.
    /// Supports translation, rotation, resize-intent, lock, and reset.
    ///
    /// Movement is applied in the plane's LOCAL frame — pressing "forward" moves the
    /// plane along its own normal, not world Z. This is what feels right for a tool
    /// that's been oriented at an arbitrary angle.
    ///
    /// Resize is captured into <see cref="Size"/> as a Vector2 (width, height) but
    /// does not yet drive shader-level finite-rectangular clipping. That comes when
    /// we modify the volume rendering shader; for now Size is the integration point.
    /// </summary>
    public class ClippingPlaneController : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("The Transform that gets moved/rotated. Drag your clipping plane GameObject here.")]
        [SerializeField] private Transform _plane;

        [Tooltip("Input source. If left null, the controller will GetComponent on this GameObject at Start.")]
        [SerializeField] private MonoBehaviour _inputSourceBehaviour;

        [Header("Speeds")]
        [SerializeField] private float _moveSpeed = 0.5f;
        [SerializeField] private float _rotateSpeed = 60f;
        [SerializeField] private float _resizeSpeed = 0.3f;

        [Header("Resize bounds")]
        [SerializeField] private Vector2 _minSize = new Vector2(0.05f, 0.05f);
        [SerializeField] private Vector2 _maxSize = new Vector2(2.0f, 2.0f);
        [SerializeField] private Vector2 _initialSize = new Vector2(1.0f, 1.0f);

        /* Runtime state */
        private IPlaneInput _input;
        private bool _isLocked;
        private Vector3 _initialPosition;
        private Quaternion _initialRotation;
        private Vector2 _size;

        /// <summary>
        /// Current rectangular cut-window size (width, height) in world units.
        /// Read by future shader work; not yet applied to rendering.
        /// </summary>
        public Vector2 Size => _size;

        /// <summary>
        /// True when the plane is locked and ignores movement input.
        /// </summary>
        public bool IsLocked => _isLocked;

        private void Start()
        {
            /* Resolve input source. Either the Inspector slot or fall back to a
             * GetComponent on this GameObject. */
            if (_inputSourceBehaviour != null)
            {
                _input = _inputSourceBehaviour as IPlaneInput;
                if (_input == null)
                {
                    Debug.LogError(
                        $"[{nameof(ClippingPlaneController)}] Assigned input source " +
                        $"'{_inputSourceBehaviour.name}' does not implement IPlaneInput.",
                        this);
                    enabled = false;
                    return;
                }
            }
            else
            {
                _input = GetComponent<IPlaneInput>();
                if (_input == null)
                {
                    Debug.LogError(
                        $"[{nameof(ClippingPlaneController)}] No IPlaneInput found. " +
                        "Assign one in the Inspector or add one to this GameObject.",
                        this);
                    enabled = false;
                    return;
                }
            }

            if (_plane == null)
            {
                Debug.LogError(
                    $"[{nameof(ClippingPlaneController)}] Plane Transform is not assigned.",
                    this);
                enabled = false;
                return;
            }

            /* Capture starting transform for reset. */
            _initialPosition = _plane.position;
            _initialRotation = _plane.rotation;
            _size = _initialSize;
        }

        private void Update()
        {
            /* Lock toggle and reset are honoured even while locked — otherwise you
             * could lock yourself out of the only way to recover. */
            if (_input.LockTogglePressed)
            {
                _isLocked = !_isLocked;
            }

            if (_input.ResetPressed)
            {
                _plane.position = _initialPosition;
                _plane.rotation = _initialRotation;
                _size = _initialSize;
            }

            if (_isLocked)
            {
                return;
            }

            ApplyMove();
            ApplyRotate();
            ApplyResize();
        }

        // -- Per-action helpers ---------------------------------------------------

        private void ApplyMove()
        {
            Vector3 m = _input.MoveInput;
            if (m == Vector3.zero) return;

            /* Local-space translation: pressing W moves along the plane's own forward,
             * not world forward. Feels correct when the plane has been rotated. */
            _plane.Translate(m * _moveSpeed * Time.deltaTime, Space.Self);
        }

        private void ApplyRotate()
        {
            Vector3 r = _input.RotateInput;
            if (r == Vector3.zero) return;

            /* Convention from IPlaneInput: x = pitch, y = yaw, z = roll.
             * Self-space so rotations compound in the plane's local frame. */
            _plane.Rotate(r * _rotateSpeed * Time.deltaTime, Space.Self);
        }

        private void ApplyResize()
        {
            Vector2 s = _input.ResizeInput;
            if (s == Vector2.zero) return;

            _size += s * _resizeSpeed * Time.deltaTime;
            _size.x = Mathf.Clamp(_size.x, _minSize.x, _maxSize.x);
            _size.y = Mathf.Clamp(_size.y, _minSize.y, _maxSize.y);
        }

        // -- State indicator ------------------------------------------------------

        private void OnGUI()
        {
            const int padding = 10;
            const int width = 220;
            const int height = 60;

            GUI.Box(new Rect(padding, padding, width, height), GUIContent.none);
            GUI.Label(
                new Rect(padding + 8, padding + 4, width - 16, 20),
                _isLocked ? "LOCKED  (Space to unlock)" : "FREE  (Space to lock)");
            GUI.Label(
                new Rect(padding + 8, padding + 24, width - 16, 20),
                $"Size: {_size.x:F2} × {_size.y:F2}");
            GUI.Label(
                new Rect(padding + 8, padding + 40, width - 16, 20),
                "R to reset");
        }
    }
}