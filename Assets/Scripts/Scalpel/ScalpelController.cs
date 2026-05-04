using UnityEngine;

namespace Volyager.Scalpel
{
    /// <summary>
    /// Drives a scalpel tool from any <see cref="IScalpelInput"/> source.
    ///
    /// The scalpel is represented by its own Transform (typically a small visible
    /// proxy mesh — a cone or pencil shape). The TIP is at the local +Z extent
    /// of that Transform; the DIRECTION is the Transform's forward.
    ///
    /// Each frame the controller pushes the tip's world position and direction
    /// to the volume's material as shader uniforms (_ScalpelTipPosition,
    /// _ScalpelDirection, _ScalpelEnabled).
    /// </summary>
    public class ScalpelController : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Transform that represents the scalpel handle. The TIP is at this Transform's local +Z extent.")]
        [SerializeField] private Transform _scalpelTransform;

        [Tooltip("Material whose shader receives the scalpel uniforms (the volume's material).")]
        [SerializeField] private Renderer _volumeRenderer;

        [Tooltip("Input source. Defaults to GetComponent on this GameObject if left null.")]
        [SerializeField] private MonoBehaviour _inputSourceBehaviour;

        [Header("Scalpel geometry")]
        [Tooltip("Distance from the scalpel transform's origin to the tip, along local +Z.")]
        [SerializeField] private float _tipOffset = 0.1f;

        [Header("Speeds")]
        [SerializeField] private float _moveSpeed = 0.3f;
        [SerializeField] private float _rotateSpeed = 60f;

        [Header("Debug visualisation")]
        [Tooltip("World-space radius of the magenta debug blob in the shader.")]
        [Range(0.001f, 0.5f)]
        [SerializeField] private float _debugRadius = 0.05f;

        /* Shader property IDs — cached for performance, no string lookups per frame */
        private static readonly int ScalpelTipPositionID = Shader.PropertyToID("_ScalpelTipPosition");
        private static readonly int ScalpelDirectionID = Shader.PropertyToID("_ScalpelDirection");
        private static readonly int ScalpelDebugRadiusID = Shader.PropertyToID("_ScalpelDebugRadius");
        private static readonly int ScalpelEnabledID = Shader.PropertyToID("_ScalpelEnabled");

        private IScalpelInput _input;
        private Vector3 _initialPosition;
        private Quaternion _initialRotation;

        private void Start()
        {
            /* Resolve input source — same pattern as ClippingPlaneController. */
            if (_inputSourceBehaviour != null)
            {
                _input = _inputSourceBehaviour as IScalpelInput;
                if (_input == null)
                {
                    Debug.LogError(
                        $"[{nameof(ScalpelController)}] Assigned input source does not implement IScalpelInput.",
                        this);
                    enabled = false;
                    return;
                }
            }
            else
            {
                _input = GetComponent<IScalpelInput>();
                if (_input == null)
                {
                    Debug.LogError(
                        $"[{nameof(ScalpelController)}] No IScalpelInput found.",
                        this);
                    enabled = false;
                    return;
                }
            }

            if (_scalpelTransform == null)
            {
                Debug.LogError($"[{nameof(ScalpelController)}] Scalpel Transform is not assigned.", this);
                enabled = false;
                return;
            }

            if (_volumeRenderer == null)
            {
                Debug.LogError($"[{nameof(ScalpelController)}] Volume Renderer is not assigned.", this);
                enabled = false;
                return;
            }

            _initialPosition = _scalpelTransform.position;
            _initialRotation = _scalpelTransform.rotation;

            /* Enable the scalpel branch in the shader. */
            _volumeRenderer.material.SetInt(ScalpelEnabledID, 1);
        }

        private void Update()
        {
            if (_input.ResetPressed)
            {
                _scalpelTransform.position = _initialPosition;
                _scalpelTransform.rotation = _initialRotation;
            }

            ApplyMove();
            ApplyRotate();
            PushUniforms();
        }

        private void OnDisable()
        {
            /* Turn off the scalpel branch when the controller is disabled, so the
             * volume renders normally. */
            if (_volumeRenderer != null && _volumeRenderer.material != null)
            {
                _volumeRenderer.material.SetInt(ScalpelEnabledID, 0);
            }
        }

        // -- Per-action helpers ------------------------------------------------

        private void ApplyMove()
        {
            Vector3 m = _input.MoveInput;
            if (m == Vector3.zero) return;
            /* Local-space — pressing forward moves the scalpel along its own forward axis. */
            _scalpelTransform.Translate(m * _moveSpeed * Time.deltaTime, Space.Self);
        }

        private void ApplyRotate()
        {
            Vector3 r = _input.RotateInput;
            if (r == Vector3.zero) return;
            _scalpelTransform.Rotate(r * _rotateSpeed * Time.deltaTime, Space.Self);
        }

        private void PushUniforms()
        {
            /* Tip = scalpel transform position + tipOffset along its forward axis. */
            Vector3 tipWorld = _scalpelTransform.position + _scalpelTransform.forward * _tipOffset;
            Vector3 dirWorld = _scalpelTransform.forward;

            Material mat = _volumeRenderer.material;
            mat.SetVector(ScalpelTipPositionID, tipWorld);
            mat.SetVector(ScalpelDirectionID, dirWorld);
            mat.SetFloat(ScalpelDebugRadiusID, _debugRadius);
        }
    }
}