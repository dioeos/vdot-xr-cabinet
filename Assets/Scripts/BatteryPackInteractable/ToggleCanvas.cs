using UnityEngine;

public class ToggleCanvas : MonoBehaviour {
  [TextArea]
  public string contentMessage;
  public SpatialUIController spatialUI;

  public void ShowPopup() {
    Debug.Log("ShowPopup called on: " + gameObject.name);
    if (spatialUI != null)
      spatialUI.Show(contentMessage);
    else
      Debug.LogWarning("PopupUIController reference is missing on: " +
                       gameObject.name);
  }

  public void HidePopup() {
    if (spatialUI != null)
      spatialUI.Hide();
  }
}
