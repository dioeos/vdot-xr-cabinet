using UnityEngine;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Management;
using System.Collections.Generic;

public class CustomAVRHandsVisualizer : MonoBehaviour {

  float logInterval = 0.5f;
  float nextLogTime = 0f; /// used to iterate through subsystems and set m_HandSubsystem
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
	foundSubsystem = true;
        break;
      }
    }
  }

  void Update() {
    if (m_HandSubsystem == null || !m_HandSubsystem.running) {
      Debug.Log("No handsubsystem");
      return;
    }
    if (Time.time < nextLogTime) {return;}
    nextLogTime = Time.time + logInterval;
    

    XRHand left = m_HandSubsystem.leftHand;
    XRHand right = m_HandSubsystem.rightHand;

    Debug.Log($"[XRHandsDebugTest] running={m_HandSubsystem.running}, " +
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
