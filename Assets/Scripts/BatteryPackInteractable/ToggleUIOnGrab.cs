using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
public class ToggleUIOnGrab : MonoBehaviour {

  [SerializeField]
  private ToggleCanvas toggleCanvas;
  private XRGrabInteractable xrGrab;

  private void Awake() {
    xrGrab = Util.GetXRGrab(this);

    // Auto-find ToggleCanvas if not assigned
    if (toggleCanvas == null) {
      toggleCanvas = GetComponent<ToggleCanvas>();
    }
  }

  private void OnEnable() {
    xrGrab.selectEntered.AddListener(OnGrab);
    xrGrab.selectExited.AddListener(OnRelease);
  }

  private void OnDisable() {
    /// listeners cleanly removed when object is disabled
    xrGrab.selectEntered.RemoveListener(OnGrab);
    xrGrab.selectExited.RemoveListener(OnRelease);
  }

  private void OnGrab(SelectEnterEventArgs args) {
    Debug.Log("Object grabbed, showing popup.");
    if (toggleCanvas != null)
      toggleCanvas.ShowPopup();
    else
      Debug.LogWarning("ToggleCanvas reference is missing.");
  }

  private void OnRelease(SelectExitEventArgs args) {
    if (toggleCanvas != null)
      toggleCanvas.HidePopup();
  }
}
