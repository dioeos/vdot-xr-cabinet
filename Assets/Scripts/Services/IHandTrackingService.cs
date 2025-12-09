using UnityEngine;
using UnityEngine.XR.Hands;

public enum Handedness { Left, Right }

public interface IHandTrackingService
{
  /// tries to get palm, gives Pose of palm if possible
  /// WARNING: Palm is not tracked
  /// bool TryGetPalmPose(Handedness hand, out Pose pose);

  bool TryGetIndexIntermediateJoint(Handedness hand, out Pose ii_pose);

  bool TryGetMiddleIntermediateJoint(Handedness hand, out Pose mi_pose);

  bool TryGetRingIntermediateJoint(Handedness hand, out Pose ri_pose);

  /// tries to get hand
  bool TryGetHand(Handedness hand, out XRHand xrHand);

  /// determines if hand is "closed"
  bool IsFist(Handedness hand);
}
