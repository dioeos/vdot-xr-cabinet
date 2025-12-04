using UnityEngine;
using UnityEngine.XR.Hands;

public class CustomAVRHandsVisualizer : MonoBehaviour {

  /// used to iterate through subsystems and set m_HandSubsystem
  static readonly List<XRHandSubsystem> s_SubsystemsReuse =
      new List<XRHandSubsystem>();

  XRHandSubsystem m_HandSubsystem;
  // HandGameObjects m_LeftHandGameObjects;
  // HandGameObjects m_RightHandGameObjects;

  void Start() {
    Debug.Log("Starting AVR Hands script");
    /// iterate & set m_HandSubsystem

    SubsystemManager.GetSubsystems(s_SubsystemsReuse);
    var foundSubsystem = false;

    for (var i = 0; i < s_SubsystemsReuse.Count; ++i) {
      var currHandSubsystem = s_SubsystemsReuse[i];

      if (currHandSubsystem.running) {
        m_HandSubsystem = currHandSubsystem;
        currHandSubsystem = true;
        break;
      }
    }
  }

  void Update() {
    if (handSubsystem == null || !handSubsystem.running)
      Debug.Log("No handsubsystem");
    return;

    XRHand left = handSubsystem.leftHand;
    XRHand right = handSubsystem.rightHand;

    Debug.Log($"[XRHandsDebugTest] running={handSubsystem.running}, " +
              $"leftTracked={left.isTracked}, rightTracked={right.isTracked}");

    // Try reading a joint pose — palm or index tip
    TryLogJoint(right, XRHandJointID.Palm, "Right Palm");
    TryLogJoint(right, XRHandJointID.IndexTip, "Right IndexTip");

    TryLogJoint(left, XRHandJointID.Palm, "Left Palm");
    TryLogJoint(left, XRHandJointID.IndexTip, "Left IndexTip");
  }

  void TryLogJoint(XRHand hand, XRHandJointID id, string label) {
    if (!hand.isTracked)
      return;

    XRHandJoint joint = hand.GetJoint(id);
    if (joint.TryGetPose(out Pose pose)) {
      Debug.Log($"[XRHandsDebugTest] {label}: {pose.position}");
    } else {
      Debug.Log($"[XRHandsDebugTest] {label}: NO POSE");
    }
  }
}
