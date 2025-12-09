using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class GrabHighlight : MonoBehaviour {
  [SerializeField]
  private GameObject highlightMesh;

  private UnityEngine.XR.Interaction.Toolkit.Interactables
      .XRGrabInteractable grab;

  private void Awake() {
    // grab = GetComponent<
    //     UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
    grab = Util.GetXRGrab(this);

    if (highlightMesh != null) {
      highlightMesh.SetActive(false);
    }
  }

  private void OnEnable() {
    grab.selectEntered.AddListener(OnSelectEntered);
    grab.selectExited.AddListener(OnSelectExited);
  }

  private void OnDisable() {
    // grab.selectEntered.RemoveListener(OnSelectEntered);
    // grab.selectExited.RemoveListener(OnSelectExited);
  }

  private void OnSelectEntered(SelectEnterEventArgs args) {
    // On grab → show highlight
    if (highlightMesh != null) {
      highlightMesh.SetActive(true);
    }
  }

  private void OnSelectExited(SelectExitEventArgs args) {
    highlightMesh.SetActive(true);
  }

  /// <summary>
  /// Called by SmoothReturnToOrigin once the object is back at its origin.
  /// </summary>
  public void TurnOffHighlight() {
    if (highlightMesh != null) {
      highlightMesh.SetActive(false);
    }
  }
}
