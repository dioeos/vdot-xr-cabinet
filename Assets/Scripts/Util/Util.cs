using UnityEngine;

public static class Util {
  public static UnityEngine.XR.Interaction.Toolkit.Interactables
      .XRGrabInteractable GetXRGrab(Component component) {
    return component.GetComponent<
        UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
  }
}
