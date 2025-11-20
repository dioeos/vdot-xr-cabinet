using UnityEngine;
using TMPro;

public class PopupUIController : MonoBehaviour
{

    public CanvasGroup canvasGroup;
    public TextMeshProUGUI textField;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Hide();
    }

    public void Show(string tmpMessage)
    {
        Debug.Log("Showing popup with message: " + tmpMessage);
        textField.text = tmpMessage;
        canvasGroup.alpha = 1f; // Make the popup visible
        canvasGroup.interactable = true; // Allow interaction
        canvasGroup.blocksRaycasts = true; // Block raycasts to underlying UI
    }

    public void Hide()
    {
        Debug.Log("Hiding popup.");
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    void Update()
    {

    }
}
