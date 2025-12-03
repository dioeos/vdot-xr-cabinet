using UnityEngine;

public class ToggleCanvas : MonoBehaviour {
  [TextArea]
  public string infoText;
  public SpatialUIController spatialUI;

  public void ShowPopup() {
    Debug.Log("ShowPopup called on: " + gameObject.name);
    if (spatialUI != null)
      spatialUI.Show();
    else
      Debug.LogWarning("PopupUIController reference is missing on: " +
                       gameObject.name);
  }

  public void HidePopup() {
    if (spatialUI != null)
      spatialUI.Hide();
  }
}
