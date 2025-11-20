using UnityEngine;

public class ToggleCanvas : MonoBehaviour
{
    [TextArea]
    public string infoText;

    public PopupUIController popup;

    public void ShowPopup()
    {
        Debug.Log("ShowPopup called on: " + gameObject.name);
        if (popup != null)
            popup.Show(infoText);
        else
            Debug.LogWarning("PopupUIController reference is missing on: " + gameObject.name);
    }

    public void HidePopup()
    {
        if (popup != null)
            popup.Hide();
    }
}
