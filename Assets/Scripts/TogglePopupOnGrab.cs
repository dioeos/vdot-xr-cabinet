using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
public class TogglePopupOnGrab : MonoBehaviour
{
    [SerializeField]
    private ToggleCanvas toggleCanvas;

    private XRGrabInteractable grab;

    private void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();

        // Auto-find ToggleCanvas if not assigned
        if (toggleCanvas == null)
        {
            toggleCanvas = GetComponent<ToggleCanvas>();
        }
    }

    private void OnEnable()
    {
        grab.selectEntered.AddListener(OnGrab);
        grab.selectExited.AddListener(OnRelease);
    }

    private void OnDisable()
    {
        grab.selectEntered.RemoveListener(OnGrab);
        grab.selectExited.RemoveListener(OnRelease);
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        Debug.Log("Object grabbed, showing popup.");
        if (toggleCanvas != null)
            toggleCanvas.ShowPopup();
        else
            Debug.LogWarning("ToggleCanvas reference is missing.");
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        if (toggleCanvas != null)
            toggleCanvas.HidePopup();
    }
}
