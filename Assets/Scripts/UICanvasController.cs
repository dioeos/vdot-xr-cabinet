using UnityEngine;
using TMPro;

public class PopupUIController : MonoBehaviour {

  [SerializeField]
  private GameObject plane;
  public TextMeshPro displayTextField;

  // Start is called once before the first execution of Update after the
  // MonoBehaviour is created
  void Start() { Hide(); }

  public void Show(string tmpMessage) {
    Debug.Log("Showing popup with message: " + tmpMessage);
    displayTextField.text = tmpMessage;
    plane.SetActive(true);
    // canvasGroup.alpha = 1f;            // Make the popup visible
    // canvasGroup.interactable = true;   // Allow interaction
    // canvasGroup.blocksRaycasts = true; // Block raycasts to underlying UI
  }

  public void Hide() {
    Debug.Log("Hiding popup.");
    plane.SetActive(false);
    // canvasGroup.alpha = 0f;
    // canvasGroup.interactable = false;
    // canvasGroup.blocksRaycasts = false;
  }

  void Update() {}
}
