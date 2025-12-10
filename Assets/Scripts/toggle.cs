using UnityEngine;

public class RotationTagActivator : MonoBehaviour
{
    [Header("Rotation Thresholds (X-axis)")]
    public float onThreshold = 0f;   // turn ON below this
    public float offThreshold = -7f;   // turn OFF above this
    public float changeTolerance = 10f; // only react if rotation changes this much

    [Header("Debug Info")]
    [SerializeField] private Vector3 currentRotation;
    private float previousXRotation = 0f;

    private GameObject[] taggedObjects;

    void Start()
    {
        string tagToUse = gameObject.name;

        try
        {
            taggedObjects = GameObject.FindGameObjectsWithTag(tagToUse);
        }
        catch
        {
            Debug.LogWarning($"Tag '{tagToUse}' does NOT exist. No objects will be toggled.");
            taggedObjects = new GameObject[0];
        }

        // initialize the previous rotation
        previousXRotation = transform.eulerAngles.x;
    }

    void Update()
    {
        currentRotation = transform.eulerAngles;

        // Because eulerAngles wrap from 360 to 0, we convert to signed angle (-180 to 180)
        float xRot = NormalizeAngle(currentRotation.x);

        Debug.Log($"Rotation -> X: {xRot}, Y: {currentRotation.y}, Z: {currentRotation.z}");

        // Only continue if the rotation changed by at least ±10 degrees
        if (Mathf.Abs(xRot - previousXRotation) >= changeTolerance)
        {
            bool shouldEnable = (xRot >= onThreshold);
            bool shouldDisable = (xRot < offThreshold);

            if (shouldEnable || shouldDisable)
            {
                foreach (GameObject obj in taggedObjects)
                {
                    if (obj != null && obj != this.gameObject)
                    {
                        
                        obj.SetActive(shouldEnable);  // enable when X < -7, disable when X > +7
                    }
                }
            }

            // update rotation memory
            previousXRotation = xRot;
        }
    }

    // convert unity's 0-360 into -180 to 180 range
    float NormalizeAngle(float angle)
    {
        if (angle > 180f) angle -= 360f;
        return angle;
    }
}
