//using UnityEngine;

//public class ToggleCanvas : MonoBehaviour
//{
//    [TextArea]
//    public string infoText;
//    public CanvasPopupUIController popup;

//    public void ShowPopup()
//    {
//        if (popup != null)
//        {
//            popup.Show(infoText);
//        }
//    }

//    public void HidePopup()
//    {
//        if (popup != null)
//        {
//            popup.Hide();
//        }
//    }
//}

using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable))]
public class TogglePopupOnGrab : MonoBehaviour
{
    [SerializeField]
    private ToggleCanvas toggleCanvas;

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grab;

    private void Awake()
    {
        grab = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();

        // Fallback: if not wired in Inspector, try to grab it from same object
        if (toggleCanvas == null)
        {
            toggleCanvas = GetComponent<ToggleCanvas>();
        }
    }

    private void OnEnable()
    {
        grab.selectEntered.AddListener(OnGrab);
        grab.selectExited.AddListener(OnRelease);
    }

    private void OnDisable()
    {
        grab.selectEntered.RemoveListener(OnGrab);
        grab.selectExited.RemoveListener(OnRelease);
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        if (toggleCanvas != null)
        {
            toggleCanvas.ShowPopup();
        }
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        if (toggleCanvas != null)
        {
            toggleCanvas.HidePopup();
        }
    }
}
