using UnityEngine;

namespace Volyager.ClippingPlane
{
    /// <summary>
    /// Contract for any input source that drives the clipping plane controller.
    /// Implementations: KeyboardPlaneInput (today), SimulatorPlaneInput (later),
    /// QuestControllerPlaneInput (eventually).
    ///
    /// The controller polls these properties each Update — implementations are
    /// expected to refresh internal state once per frame (typically in Update)
    /// and serve consistent values across reads within the same frame.
    /// </summary>
    public interface IPlaneInput
    {
        /// <summary>
        /// Translation intent. Components are normalised in [-1, 1] per axis.
        /// The controller multiplies by speed and Time.deltaTime.
        /// Coordinate convention is determined by the controller, not this interface
        /// (typically: x = strafe, y = up/down, z = forward/back).
        /// </summary>
        Vector3 MoveInput { get; }

        /// <summary>
        /// Rotation intent. Components in [-1, 1] per axis.
        /// Convention (controller-defined): x = pitch, y = yaw, z = roll.
        /// </summary>
        Vector3 RotateInput { get; }

        /// <summary>
        /// Resize intent for a finite rectangular clipping region.
        /// .x = width change, .y = height change, both in [-1, 1].
        /// </summary>
        Vector2 ResizeInput { get; }

        /// <summary>
        /// True only on the single frame the lock toggle was activated.
        /// </summary>
        bool LockTogglePressed { get; }

        /// <summary>
        /// True only on the single frame the reset action was activated.
        /// </summary>
        bool ResetPressed { get; }
    }
}