// using UnityEngine;
// using UnityEngine.XR.Hands;
// using UnityEngine.XR.Management;
// using System.Collections.Generic;
//
// public class CustomAVRHandsVisualizer : MonoBehaviour {
//
//   float logInterval = 0.5f;
//   float nextLogTime = 0f; /// used to iterate through subsystems and set
//   m_HandSubsystem static readonly List<XRHandSubsystem> s_SubsystemsReuse =
//       new List<XRHandSubsystem>();
//
//   XRHandSubsystem m_HandSubsystem;
//   // HandGameObjects m_LeftHandGameObjects;
//   // HandGameObjects m_RightHandGameObjects;
//
//   void Start() {
//     Debug.Log("Starting AVR Hands script");
//     /// iterate & set m_HandSubsystem
//
//     SubsystemManager.GetSubsystems(s_SubsystemsReuse);
//     var foundSubsystem = false;
//
//     for (var i = 0; i < s_SubsystemsReuse.Count; ++i) {
//       var currHandSubsystem = s_SubsystemsReuse[i];
//
//       if (currHandSubsystem.running) {
//         m_HandSubsystem = currHandSubsystem;
// 	foundSubsystem = true;
//         break;
//       }
//     }
//   }
//
//   void Update() {
//     if (m_HandSubsystem == null || !m_HandSubsystem.running) {
//       Debug.Log("No handsubsystem");
//       return;
//     }
//     if (Time.time < nextLogTime) {return;}
//     nextLogTime = Time.time + logInterval;
//
//
//     XRHand left = m_HandSubsystem.leftHand;
//     XRHand right = m_HandSubsystem.rightHand;
//
//     Debug.Log($"[XRHandsDebugTest] running={m_HandSubsystem.running}, " +
//               $"leftTracked={left.isTracked},
//               rightTracked={right.isTracked}");
//
//     // Try reading a joint pose — palm or index tip
//     TryLogJoint(right, XRHandJointID.Palm, "Right Palm");
//     TryLogJoint(right, XRHandJointID.IndexTip, "Right IndexTip");
//
//     TryLogJoint(left, XRHandJointID.Palm, "Left Palm");
//     TryLogJoint(left, XRHandJointID.IndexTip, "Left IndexTip");
//   }
//
//   void TryLogJoint(XRHand hand, XRHandJointID id, string label) {
//     if (!hand.isTracked)
//       return;
//
//     XRHandJoint joint = hand.GetJoint(id);
//     if (joint.TryGetPose(out Pose pose)) {
//       Debug.Log($"[XRHandsDebugTest] {label}: {pose.position}");
//     } else {
//       Debug.Log($"[XRHandsDebugTest] {label}: NO POSE");
//     }
//   }
// }

using UnityEngine;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Management;

public class HandTrackingManager : MonoBehaviour, IHandTrackingService
{

  XRHandSubsystem m_HandSubsystem;

  void Awake()
  {
    var loader = XRGeneralSettings.Instance.Manager.activeLoader;
    if (loader == null)
    {
      Debug.LogError("No active XR loader yet – hand subsystem not available.");
      return;
    }
    m_HandSubsystem = loader.GetLoadedSubsystem<XRHandSubsystem>();
  }

  void Update()
  {
    if (m_HandSubsystem == null || !m_HandSubsystem.running)
    {
      Debug.Log("Did not find subsystem");
      return;
    }

    XRHand leftHand = m_HandSubsystem.leftHand;
    XRHand rightHand = m_HandSubsystem.rightHand;

    // LogJointData(leftHand, "[LEFT HAND]");
    // LogJointData(rightHand, "[RIGHT HAND]");
  }

  void LogJointData(XRHand hand, string handLabel)
  {
    for (var i = XRHandJointID.BeginMarker.ToIndex();
         i < XRHandJointID.EndMarker.ToIndex(); i++)
    {
      var jointData = hand.GetJoint(XRHandJointIDUtility.FromIndex(i));
      if (jointData.TryGetPose(out Pose pose))
      {
        Debug.Log($"{handLabel} {jointData}: {pose}");
      }
    }
  }

  public bool TryGetIndexIntermediateJoint(Handedness hand, out Pose ii_pose)
  {
    ii_pose = default;

    if (m_HandSubsystem == null || !m_HandSubsystem.running)
    {
      return false;
    }

    XRHand xrHand = hand == Handedness.Left ? m_HandSubsystem.leftHand
                                            : m_HandSubsystem.rightHand;

    // retrieve index inter. joint
    var indexInter = xrHand.GetJoint(XRHandJointID.IndexIntermediate);
    return indexInter.TryGetPose(out ii_pose);
  }

  public bool TryGetMiddleIntermediateJoint(Handedness hand, out Pose mi_pose)
  {
    mi_pose = default;

    if (m_HandSubsystem == null || !m_HandSubsystem.running)
    {
      return false;
    }

    XRHand xrHand = hand == Handedness.Left ? m_HandSubsystem.leftHand
                                            : m_HandSubsystem.rightHand;

    // retrieve index inter. joint
    var middleInter = xrHand.GetJoint(XRHandJointID.MiddleIntermediate);
    return middleInter.TryGetPose(out mi_pose);
  }

  public bool TryGetRingIntermediateJoint(Handedness hand, out Pose ri_pose)
  {
    ri_pose = default;

    if (m_HandSubsystem == null || !m_HandSubsystem.running)
    {
      return false;
    }

    XRHand xrHand = hand == Handedness.Left ? m_HandSubsystem.leftHand
                                            : m_HandSubsystem.rightHand;

    // retrieve index inter. joint
    var ringInter = xrHand.GetJoint(XRHandJointID.RingIntermediate);
    return ringInter.TryGetPose(out ri_pose);
  }

  // public bool TryGetPalmPose(Handedness hand, out Pose pose)
  // {
  //   pose = default;
  //   /// returns bool and out var that is the pose of palm
  //   if (m_HandSubsystem == null || !m_HandSubsystem.running)
  //   {
  //     return false;
  //   }
  //
  //   XRHand xrHand = hand == Handedness.Left ? m_HandSubsystem.leftHand
  //                                           : m_HandSubsystem.rightHand;
  //
  //   if (!xrHand.isTracked)
  //   {
  //     // Debug.Log("Hand is not tracked!!!!");
  //   }
  //   // retrieve palmJoint via GetJoint(id) and determine pose
  //   var palmJoint = xrHand.GetJoint(XRHandJointID.Palm);
  //   var att = palmJoint.TryGetPose(out pose);
  //   Debug.Log($"Here is POSE: {pose}");
  //   return att;
  // }

  public bool TryGetHand(Handedness hand, out XRHand xrHand)
  {
    xrHand = default;

    if (m_HandSubsystem == null || !m_HandSubsystem.running)
      return false;

    xrHand = hand == Handedness.Left ? m_HandSubsystem.leftHand
                                     : m_HandSubsystem.rightHand;

    // FIX: returns false always
    return xrHand.isTracked;
  }

  private float ComputeFingerCurl(XRHandJoint proximal, XRHandJoint tip,
                                  XRHandJoint wrist)
  {

    if (!proximal.TryGetPose(out Pose p_Pose) ||
        !tip.TryGetPose(out Pose t_Pose) || !wrist.TryGetPose(out Pose w_Pose))
      return 0f;

    Vector3 proximalPos = (p_Pose.position - w_Pose.position).normalized;
    Vector3 tipPos = (t_Pose.position - p_Pose.position).normalized;

    return Vector3.Angle(proximalPos, tipPos);
  }

  public bool IsFist(Handedness hand)
  {
    if (!TryGetHand(hand, out XRHand xrHand))
      return false;

    XRHandJoint wrist = xrHand.GetJoint(XRHandJointID.Wrist);

    float curlIndex =
        ComputeFingerCurl(xrHand.GetJoint(XRHandJointID.IndexProximal),
                          xrHand.GetJoint(XRHandJointID.IndexTip), wrist);

    float curlMiddle =
        ComputeFingerCurl(xrHand.GetJoint(XRHandJointID.MiddleProximal),
                          xrHand.GetJoint(XRHandJointID.MiddleTip), wrist);

    float curlRing =
        ComputeFingerCurl(xrHand.GetJoint(XRHandJointID.RingProximal),
                          xrHand.GetJoint(XRHandJointID.RingTip), wrist);

    float curlPinky =
        ComputeFingerCurl(xrHand.GetJoint(XRHandJointID.LittleProximal),
                          xrHand.GetJoint(XRHandJointID.LittleTip), wrist);

    return curlIndex > 50f && curlMiddle > 50f && curlRing > 50f &&
           curlPinky > 50f;
  }
}
