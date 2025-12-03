using UnityEngine;
using TMPro;

public class SpatialUIController : MonoBehaviour {
  [SerializeField]
  private GameObject SpatialUI;

  void Start() { Hide(); }

  public void Show() { SpatialUI.SetActive(true); }

  public void Hide() { SpatialUI.SetActive(false); }
}
