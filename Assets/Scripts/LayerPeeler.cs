using UnityEngine;
using UnityEngine.InputSystem;
using UVRTransferFunction = UnityVolumeRendering.TransferFunction;
using UnityVolumeRendering;

public class LayerPeeler : MonoBehaviour
{
    private VolumeRenderedObject volumeObject;
    private UVRTransferFunction tf;
    private bool ready = false;

    private bool outerVisible = true;
    private bool middleVisible = true;
    private bool innerVisible = true;

    // Opacity when visible
    private float outerOpacity = 0.04f;
    private float middleOpacity = 0.15f;
    private float innerOpacity = 0.9f;

    void Update()
    {
        if (!ready)
        {
            volumeObject = FindObjectOfType<VolumeRenderedObject>();
            if (volumeObject != null)
            {
                tf = volumeObject.transferFunction;
                if (tf != null)
                {
                    ready = true;
                    Debug.Log("LayerPeeler ready. Press 1 to peel outer, 2 to peel middle, 3 to peel inner. Press R to restore all.");
                }
            }
            return;
        }

        // Peel outer
        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            outerVisible = false;
            UpdateTransferFunction();
            Debug.Log("Outer shell peeled.");
        }

        // Peel middle
        if (Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            middleVisible = false;
            UpdateTransferFunction();
            Debug.Log("Middle layer peeled.");
        }

        // Peel inner
        if (Keyboard.current.digit3Key.wasPressedThisFrame)
        {
            innerVisible = false;
            UpdateTransferFunction();
            Debug.Log("Inner core peeled.");
        }

        // Restore all
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            outerVisible = true;
            middleVisible = true;
            innerVisible = true;
            UpdateTransferFunction();
            Debug.Log("All layers restored.");
        }
    }

    void UpdateTransferFunction()
    {
        tf.alphaControlPoints.Clear();

        tf.alphaControlPoints.Add(new TFAlphaControlPoint(0.0f, 0.0f));
        tf.alphaControlPoints.Add(new TFAlphaControlPoint(0.18f, 0.0f));

        // Outer shell
        float outerA = outerVisible ? outerOpacity : 0.0f;
        tf.alphaControlPoints.Add(new TFAlphaControlPoint(0.19f, outerA));
        tf.alphaControlPoints.Add(new TFAlphaControlPoint(0.32f, outerA));

        // Middle layer
        float middleA = middleVisible ? middleOpacity : 0.0f;
        tf.alphaControlPoints.Add(new TFAlphaControlPoint(0.33f, middleA));
        tf.alphaControlPoints.Add(new TFAlphaControlPoint(0.49f, middleA));

        // Inner core
        float innerA = innerVisible ? innerOpacity : 0.0f;
        tf.alphaControlPoints.Add(new TFAlphaControlPoint(0.50f, innerA));
        tf.alphaControlPoints.Add(new TFAlphaControlPoint(0.82f, innerA));
        tf.alphaControlPoints.Add(new TFAlphaControlPoint(0.83f, 0.0f));
        tf.alphaControlPoints.Add(new TFAlphaControlPoint(1.0f, 0.0f));

        tf.GenerateTexture();
        volumeObject.SetTransferFunction(tf);
    }
}