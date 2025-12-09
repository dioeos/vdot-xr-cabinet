using UnityEngine;
using UnityEngine.XR.Hands;

internal enum DrawerMode { None, Opening, Closing }

public class DrawerController : MonoBehaviour
{

  [Tooltip("The collider region that joints must be in to move the drawer")]
  [SerializeField]
  private BoxCollider drawerOpenRegion;

  [Tooltip("The collider region that joints must be in to close the drawer")]
  [SerializeField]
  private BoxCollider drawerCloseRegion;

  [Tooltip("The movement scale when opening the drawer")]
  [SerializeField]
  private float openMovementScale = 15f;

  [Tooltip("The movement scale when closing the drawer")]
  [SerializeField]
  private float closeMovementScale = 15f;

  // USE CLAMP TO LIMIT RANGE

  [Tooltip("The transform of the drawer visuals")]
  [SerializeField]
  private Transform drawerVisual;

  private DrawerMode mode = DrawerMode.None;
  private Handedness activeHand;

  private float drawerStartOpenX;
  private float middleInterStartOpenX;

  private float drawerStartCloseX;
  private float middleInterCloseX;

  private IHandTrackingService d_handService;

  // injection point
  public void Initialize(IHandTrackingService handService)
  {
    d_handService = handService;
  }

  void Update()
  {
    bool rightInOpen = InColliderRegion(drawerOpenRegion, Handedness.Right,
                                        out var openRightMiddleInter);
    bool leftInOpen = InColliderRegion(drawerOpenRegion, Handedness.Left,
                                       out var openLeftMiddleInter);

    bool rightInClose = InColliderRegion(drawerCloseRegion, Handedness.Right,
                                         out var closeRightMiddleInter);
    bool leftInClose = InColliderRegion(drawerCloseRegion, Handedness.Left,
                                        out var closeLeftMiddleInter);

    switch (mode)
    {
      case DrawerMode.None:
        // decide drawer action - right hand takes priority, then left
        if (rightInOpen)
        {
          Debug.Log("RIGHT IN OPEN");
          mode = DrawerMode.Opening;
          activeHand = Handedness.Right;
          BeginOpenInteraction(openRightMiddleInter);
        }
        else if (leftInOpen)
        {
          mode = DrawerMode.Opening;
          activeHand = Handedness.Left;
          BeginOpenInteraction(openLeftMiddleInter);
        }
        else if (rightInClose)
        {
          mode = DrawerMode.Closing;
          activeHand = Handedness.Right;
          BeginCloseInteraction(closeRightMiddleInter);
        }
        else if (leftInClose)
        {
          mode = DrawerMode.Closing;
          activeHand = Handedness.Left;
          BeginCloseInteraction(closeLeftMiddleInter);
        }
        break;

      case DrawerMode.Opening:
        if (!InColliderRegion(drawerOpenRegion, activeHand, out var openPose))
        {
          Debug.Log("Ending interaction");
          EndInteraction();
          return;
        }
        OpenDrawerOnPalmPose(openPose);
        break;

      case DrawerMode.Closing:
        if (!InColliderRegion(drawerCloseRegion, activeHand, out var closePose))
        {
          EndInteraction();
          return;
        }
        CloseDrawerOnPalmPose(closePose);
        break;
    }
  }

  private void BeginOpenInteraction(Pose mi_pose)
  {
    Debug.Log("Starting Interaction");
    middleInterStartOpenX = mi_pose.position.x;      // world x at grab
    drawerStartOpenX = drawerVisual.localPosition.x; // local x at grab
  }

  private void OpenDrawerOnPalmPose(Pose palmPose)
  {
    // hands are tracked in world space, drawers slide in local
    float palmX = palmPose.position.x;

    // how much hand has moved along world x
    float palmDeltaX = palmX - middleInterStartOpenX;
    Debug.Log($"==PALM DELTA== : {palmDeltaX}");

    // NOW: opening requires moving TOWARD you => X increasing => delta > 0
    if (palmDeltaX <= 0f)
    {
      Debug.LogWarning($"Cannot open in that direction");
      return;
    }

    // map delta into drawer local X motion
    float targetX = drawerStartOpenX + palmDeltaX * openMovementScale;

    // apply to visuals
    Vector3 local = drawerVisual.localPosition;
    local.x = targetX;
    drawerVisual.localPosition = local;
  }

  private void BeginCloseInteraction(Pose palmPose)
  {
    middleInterCloseX = palmPose.position.x;
    drawerStartCloseX = drawerVisual.localPosition.x;
  }

  private void CloseDrawerOnPalmPose(Pose palmPose)
  {
    float palmX = palmPose.position.x;
    float palmDeltaX = palmX - middleInterCloseX;

    // NOW: closing requires pushing AWAY from you => X decreasing => delta < 0
    if (palmDeltaX >= 0f)
    {
      Debug.LogWarning($"Cannot close in that direction");
      return;
    }

    float targetX = drawerStartCloseX + palmDeltaX * closeMovementScale;

    Vector3 local = drawerVisual.localPosition;
    local.x = targetX;
    drawerVisual.localPosition = local;
  }

  private void EndInteraction() { mode = DrawerMode.None; }

  /// calls
  private bool InColliderRegion(BoxCollider colliderRegion, Handedness hand,
                                out Pose mi_pose)
  {
    mi_pose = default;
    // check index, middle, ring intermediate joints if in collider
    // if so, return intermediate pose

    if (!d_handService.TryGetIndexIntermediateJoint(hand, out var ii_pose))
    {
      Debug.LogWarning("Could not get ii_joint");
      return false;
    }

    // if (!d_handService.TryGetMiddleIntermediateJoint(hand, out var mi_pose))
    //   return false;

    if (!d_handService.TryGetRingIntermediateJoint(hand, out var ri_pose))
    {
      Debug.LogWarning("Could not get ri_pose");
      return false;
    }

    if (!colliderRegion.bounds.Contains(ii_pose.position) ||
        !colliderRegion.bounds.Contains(ri_pose.position))
      return false;

    if (!d_handService.TryGetHand(hand, out var xrhand))
      return false;

    // abstract this
    var middleInterJoint = xrhand.GetJoint(XRHandJointID.MiddleIntermediate);
    return middleInterJoint.TryGetPose(out mi_pose);
  }
}
