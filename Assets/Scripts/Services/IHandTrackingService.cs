using UnityEngine;
using UnityEngine.XR.Hands;

public enum Handedness { Left, Right }

public interface IHandTrackingService
{
  /// tries to get palm, gives Pose of palm if possible
  bool TryGetPalmPose(Handedness hand, out Pose pose);

  /// tries to get hand
  bool TryGetHand(Handedness hand, out XRHand xrHand);

  /// determines if hand is "closed"
  bool IsFist(Handedness hand);
}
