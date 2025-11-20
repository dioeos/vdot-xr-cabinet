using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;

public class SmoothReturnToOrigin : MonoBehaviour
{
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;
    private Coroutine returnCoroutine;
    public float returnSpeed = 1.5f; // Adjust the speed of return

    void Start()
    {
        // Save the initial position and rotation when the scene starts
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();

        // Subscribe to the grab events
        grabInteractable.selectEntered.AddListener(OnGrabbed);
        grabInteractable.selectExited.AddListener(OnReleased);
    }

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        // Stop the return coroutine if it's running when the object is grabbed again
        if (returnCoroutine != null)
        {
            StopCoroutine(returnCoroutine);
        }
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        // Start the coroutine to smoothly return the object to its origin
        if (returnCoroutine != null)
        {
            StopCoroutine(returnCoroutine);
        }
        returnCoroutine = StartCoroutine(ReturnToOriginRoutine());
    }

    private IEnumerator ReturnToOriginRoutine()
    {
        // Cache the current position and rotation for smooth interpolation
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;
        float t = 0f;

        // Smoothly move and rotate the object back to its initial transform
        while (t < 1f)
        {
            t += Time.deltaTime * returnSpeed;
            transform.position = Vector3.Lerp(startPos, initialPosition, t);
            transform.rotation = Quaternion.Lerp(startRot, initialRotation, t);
            yield return null; // Wait until the next frame
        }

        // Ensure the object snaps precisely to the final position and rotation
        transform.position = initialPosition;
        transform.rotation = initialRotation;
        returnCoroutine = null;
    }
}
