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
  private float openMovementScale;

  [Tooltip("The movement scale when closing the drawer")]
  [SerializeField]
  private float closeMovementScale;

  [Header("Local X clamp (drawerVisual.localPosition.x)")]
  [Tooltip("Minimum local X (fully closed)")]
  [SerializeField]
  private float minLocalX;

  [Tooltip("Maximum local X (fully open)")]
  [SerializeField]
  private float maxLocalX;

  [Tooltip("The transform of the drawer visuals")]
  [SerializeField]
  private Transform drawerVisual;

  private DrawerMode mode = DrawerMode.None;
  private Handedness activeHand;

  // Start local X (for opening)
  private float drawerStartOpenX;
  // World-space position of the driving joint at grab (opening)
  private Vector3 openStartWorldPos;

  // Start local X (for closing)
  private float drawerStartCloseX;
  // World-space position of the driving joint at grab (closing)
  private Vector3 closeStartWorldPos;

  // Drawer slide direction in world space (local +X transformed to world)
  private Vector3 drawerDirWorld;

  private IHandTrackingService d_handService;

  // injection point
  public void Initialize(IHandTrackingService handService)
  {
    d_handService = handService;
  }

  void Start()
  {
    if (drawerVisual == null)
    {
      Debug.LogError("DrawerController: drawerVisual is not assigned.");
      return;
    }

    // local +X is our logical slide axis; convert to world direction
    drawerDirWorld = drawerVisual.TransformDirection(Vector3.right).normalized;
  }

  void Update()
  {
    if (d_handService == null || drawerVisual == null)
      return;

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
          Debug.Log("Drawer: RIGHT hand in OPEN region");
          mode = DrawerMode.Opening;
          activeHand = Handedness.Right;
          BeginOpenInteraction(openRightMiddleInter);
        }
        else if (leftInOpen)
        {
          Debug.Log("Drawer: LEFT hand in OPEN region");
          mode = DrawerMode.Opening;
          activeHand = Handedness.Left;
          BeginOpenInteraction(openLeftMiddleInter);
        }
        else if (rightInClose)
        {
          Debug.Log("Drawer: RIGHT hand in CLOSE region");
          mode = DrawerMode.Closing;
          activeHand = Handedness.Right;
          BeginCloseInteraction(closeRightMiddleInter);
        }
        else if (leftInClose)
        {
          Debug.Log("Drawer: LEFT hand in CLOSE region");
          mode = DrawerMode.Closing;
          activeHand = Handedness.Left;
          BeginCloseInteraction(closeLeftMiddleInter);
        }
        break;

      case DrawerMode.Opening:
        if (!InColliderRegion(drawerOpenRegion, activeHand, out var openPose))
        {
          Debug.Log("Drawer: Ending OPEN interaction (hand left region)");
          EndInteraction();
          return;
        }
        OpenDrawerOnMiddlePose(openPose);
        break;

      case DrawerMode.Closing:
        if (!InColliderRegion(drawerCloseRegion, activeHand, out var closePose))
        {
          Debug.Log("Drawer: Ending CLOSE interaction (hand left region)");
          EndInteraction();
          return;
        }
        CloseDrawerOnMiddlePose(closePose);
        break;
    }
  }

  // ----------------- OPENING -----------------

  private void BeginOpenInteraction(Pose middlePose)
  {
    Debug.Log("Drawer: Begin OPEN interaction");
    openStartWorldPos = middlePose.position;         // world at grab
    drawerStartOpenX = drawerVisual.localPosition.x; // local at grab
  }

  private void OpenDrawerOnMiddlePose(Pose middlePose)
  {
    Vector3 currentWorld = middlePose.position;
    Vector3 deltaWorld = currentWorld - openStartWorldPos;

    // movement along the drawer's slide axis
    float deltaAlongDrawer = Vector3.Dot(deltaWorld, drawerDirWorld);
    Debug.Log($"[OPEN] deltaAlongDrawer = {deltaAlongDrawer}");

    // Opening requires moving in the positive drawerDirWorld direction
    // which corresponds to increasing local X (per your reference)
    if (deltaAlongDrawer <= 0f)
    {
      // Hand moved opposite to opening direction, ignore
      return;
    }

    float targetX = drawerStartOpenX + deltaAlongDrawer * openMovementScale;

    // Clamp to allowed range
    targetX = Mathf.Clamp(targetX, minLocalX, maxLocalX);

    Vector3 local = drawerVisual.localPosition;
    local.x = targetX;
    drawerVisual.localPosition = local;
  }

  // ----------------- CLOSING -----------------

  private void BeginCloseInteraction(Pose middlePose)
  {
    Debug.Log("Drawer: Begin CLOSE interaction");
    closeStartWorldPos = middlePose.position;         // world at grab
    drawerStartCloseX = drawerVisual.localPosition.x; // local at grab
  }

  private void CloseDrawerOnMiddlePose(Pose middlePose)
  {
    Vector3 currentWorld = middlePose.position;
    Vector3 deltaWorld = currentWorld - closeStartWorldPos;

    float deltaAlongDrawer = Vector3.Dot(deltaWorld, drawerDirWorld);
    Debug.Log($"[CLOSE] deltaAlongDrawer = {deltaAlongDrawer}");

    // Closing requires pushing opposite to drawerDirWorld
    // which corresponds to decreasing local X
    if (deltaAlongDrawer >= 0f)
    {
      // Hand moved in opening direction, ignore
      return;
    }

    float targetX = drawerStartCloseX + deltaAlongDrawer * closeMovementScale;

    targetX = Mathf.Clamp(targetX, minLocalX, maxLocalX);

    Vector3 local = drawerVisual.localPosition;
    local.x = targetX;
    drawerVisual.localPosition = local;
  }

  // ----------------- COMMON -----------------

  private void EndInteraction() { mode = DrawerMode.None; }

  /// <summary>
  /// Checks if fingers for the given hand are inside colliderRegion.
  /// If so, returns the middle-intermediate joint pose as the driver pose.
  /// </summary>
  private bool InColliderRegion(BoxCollider colliderRegion, Handedness hand,
                                out Pose middlePose)
  {
    middlePose = default;

    if (colliderRegion == null)
      return false;

    // if (!d_handService.TryGetHand(hand, out var xrhand))
    //   return false;

    // var indexInter = xrhand.GetJoint(XRHandJointID.IndexIntermediate);
    // var middleInter = xrhand.GetJoint(XRHandJointID.MiddleIntermediate);
    // var ringInter = xrhand.GetJoint(XRHandJointID.RingIntermediate);

    if (!d_handService.TryGetIndexIntermediateJoint(hand, out var ii_pose) ||
        !d_handService.TryGetMiddleIntermediateJoint(hand, out var mi_pose) ||
        !d_handService.TryGetRingIntermediateJoint(hand, out var ri_pose))
    {
      return false;
    }

    // if (!indexInter.TryGetPose(out var iiPose) ||
    //     !middleInter.TryGetPose(out var miPose) ||
    //     !ringInter.TryGetPose(out var riPose))
    // {
    //   return false;
    // }

    // Looser condition: any of the three fingers inside region
    bool anyIn = colliderRegion.bounds.Contains(ii_pose.position) ||
                 colliderRegion.bounds.Contains(mi_pose.position) ||
                 colliderRegion.bounds.Contains(ri_pose.position);

    if (!anyIn)
      return false;

    // Use middle finger as the driver pose
    middlePose = mi_pose;
    return true;
  }
}
