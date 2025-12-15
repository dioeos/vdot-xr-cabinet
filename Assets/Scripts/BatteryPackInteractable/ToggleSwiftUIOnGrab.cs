using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using VDOTModule.XR.UI;

public class ToggleUIOnGrab : MonoBehaviour
{

  [Header("SwiftUI Panel Data")]
  [SerializeField]
  private string moduleId; // ex. "cabinet_objects"

  [SerializeField]
  [TextArea]
  private string displayText;

  private XRGrabInteractable xrGrab;
  private CustomSwiftUIDriver swiftUIDriver;

  private void Awake()
  {
    xrGrab = Util.GetXRGrab(this);
    if (swiftUIDriver == null)
      swiftUIDriver = FindFirstObjectByType<CustomSwiftUIDriver>();
  }

  private void OnEnable() { xrGrab.selectEntered.AddListener(OnGrab); }

  private void OnDisable() { xrGrab.selectEntered.RemoveListener(OnGrab); }

  private void OnGrab(SelectEnterEventArgs args)
  {
    Debug.Log($"Object grabbed → opening SwiftUI panel: {moduleId}");

    SendToSwiftUI();
  }

  private void SendToSwiftUI()
  {
    // Format is important — keep it simple and parseable
    // string message = $"ui:open:{moduleId}:{Escape(displayText)}";
    //
    // UnityToSwift.Send(message);
    swiftUIDriver.ShowInteractableInfo(moduleId, displayText);
  }

  // private string Escape(string s) { return s.Replace(":", "\\:"); }
}
