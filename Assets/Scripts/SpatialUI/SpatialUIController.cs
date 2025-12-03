using UnityEngine;
using TMPro;

public class SpatialUIController : MonoBehaviour {
  [SerializeField]
  private GameObject SpatialUI;
  private TextMeshPro contentSection;

  void Start() { Hide(); }

  public void Show(string message) {
    SpatialUI.SetActive(true);
    contentSection.text = message;
  }

  public void Hide() { SpatialUI.SetActive(false); }
}
