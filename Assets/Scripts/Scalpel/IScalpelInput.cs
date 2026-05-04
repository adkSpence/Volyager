using UnityEngine;

namespace Volyager.Scalpel
{
    /// <summary>
    /// Contract for any input source that drives the scalpel.
    /// Implementations: KeyboardScalpelInput (today), QuestControllerScalpelInput (later).
    ///
    /// Polled each Update by ScalpelController. Implementations refresh once per
    /// frame and serve consistent values across reads within the same frame.
    /// </summary>
    public interface IScalpelInput
    {
        /// <summary>
        /// Translation intent in [-1, 1] per axis. Controller multiplies by speed.
        /// </summary>
        Vector3 MoveInput { get; }

        /// <summary>
        /// Rotation intent in [-1, 1] per axis. Controller convention: x = pitch, y = yaw, z = roll.
        /// </summary>
        Vector3 RotateInput { get; }

        /// <summary>
        /// True only on the single frame the reset action was activated.
        /// </summary>
        bool ResetPressed { get; }
    }
}