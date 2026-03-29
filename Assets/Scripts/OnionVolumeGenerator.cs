using UnityEngine;
using UVRTransferFunction = UnityVolumeRendering.TransferFunction;
using UnityVolumeRendering;

public class OnionVolumeGenerator : MonoBehaviour
{
    void Start()
    {
        GenerateOnion();
    }

    void GenerateOnion()
    {
        int size = 128;

        VolumeDataset dataset = ScriptableObject.CreateInstance<VolumeDataset>();
        dataset.datasetName = "Onion";
        dataset.dimX = size;
        dataset.dimY = size;
        dataset.dimZ = size;
        dataset.data = new float[size * size * size];

        Vector3 centre = new Vector3(size / 2f, size / 2f, size / 2f);
        float maxDist = size / 2f;

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                for (int z = 0; z < size; z++)
                {
                    float dist = Vector3.Distance(new Vector3(x, y, z), centre);
                    float normalised = dist / maxDist;

                    float value = 0f;
                    if (normalised >= 0.66f) value = 0.2f;
                    else if (normalised >= 0.33f) value = 0.5f;
                    else if (normalised >= 0.0f) value = 0.8f;

                    dataset.data[x + y * size + z * size * size] = value;
                }
            }
        }

        dataset.FixDimensions();

        VolumeRenderedObject obj = VolumeObjectFactory.CreateObject(dataset);
        obj.transform.position = Vector3.zero;

        UVRTransferFunction tf = ScriptableObject.CreateInstance<UVRTransferFunction>();
        tf.colourControlPoints.Clear();
        tf.alphaControlPoints.Clear();

        // Colours
        // Value 0.2 = outer shell — red
        tf.colourControlPoints.Add(new TFColourControlPoint(0.0f, Color.black));
        tf.colourControlPoints.Add(new TFColourControlPoint(0.18f, Color.black));
        tf.colourControlPoints.Add(new TFColourControlPoint(0.19f, Color.red));
        tf.colourControlPoints.Add(new TFColourControlPoint(0.32f, Color.red));

        // Value 0.5 = middle layer — yellow
        tf.colourControlPoints.Add(new TFColourControlPoint(0.33f, Color.yellow));
        tf.colourControlPoints.Add(new TFColourControlPoint(0.49f, Color.yellow));

        // Value 0.8 = inner core — green
        tf.colourControlPoints.Add(new TFColourControlPoint(0.50f, Color.green));
        tf.colourControlPoints.Add(new TFColourControlPoint(0.82f, Color.green));
        tf.colourControlPoints.Add(new TFColourControlPoint(0.83f, Color.black));
        tf.colourControlPoints.Add(new TFColourControlPoint(1.0f, Color.black));

        // Outer shell — thin and ghosted
        tf.alphaControlPoints.Add(new TFAlphaControlPoint(0.0f, 0.0f));
        tf.alphaControlPoints.Add(new TFAlphaControlPoint(0.18f, 0.0f));
        tf.alphaControlPoints.Add(new TFAlphaControlPoint(0.19f, 0.04f));
        tf.alphaControlPoints.Add(new TFAlphaControlPoint(0.32f, 0.04f));

        // Middle layer
        tf.alphaControlPoints.Add(new TFAlphaControlPoint(0.33f, 0.15f));
        tf.alphaControlPoints.Add(new TFAlphaControlPoint(0.49f, 0.15f));

        // Inner core — solid
        tf.alphaControlPoints.Add(new TFAlphaControlPoint(0.50f, 0.9f));
        tf.alphaControlPoints.Add(new TFAlphaControlPoint(0.82f, 0.9f));
        tf.alphaControlPoints.Add(new TFAlphaControlPoint(0.83f, 0.0f));
        tf.alphaControlPoints.Add(new TFAlphaControlPoint(1.0f, 0.0f));

        tf.GenerateTexture();
        obj.SetTransferFunction(tf);

        Debug.Log("Onion volume rendered with illustrative ghosting.");
    }
}