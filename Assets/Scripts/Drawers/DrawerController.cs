using UnityEngine;

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
  private float openMovementScale = 1.8f;

  [Tooltip("The movement scale when closing the drawer")]
  [SerializeField]
  private float closeMovementScale = 1.8f;

  // [Tooltip("The max z range to clamp opening the drawer")]
  // [SerializeField]
  // private float zOpenRange = 0f;
  //
  // [Tooltip("The max z range to clamp closing the drawer")]
  // [SerializeField]
  // private float zCloseRange = 0f;

  // USE CLAMP TO LIMIT RANGE

  [Tooltip("The transform of the drawer visuals")]
  [SerializeField]
  private Transform drawerVisual;

  private DrawerMode mode = DrawerMode.None;
  private Handedness activeHand;
  private float palmStartOpenZ;
  private float drawerStartOpenZ;
  private float palmStartCloseZ;
  private float drawerStartCloseZ;

  private IHandTrackingService d_handService;

  // injection point
  public void Initialize(IHandTrackingService handService)
  {
    d_handService = handService;
  }

  void Update()
  {
    if (!d_handService.TryGetPalmPose(Handedness.Right, out var palmPose))
    {
      return;
    }

    bool hasRight =
        d_handService.TryGetPalmPose(Handedness.Right, out var rightPalmPose);
    bool hasLeft =
        d_handService.TryGetPalmPose(Handedness.Left, out var leftPalmPose);

    bool rightInOpen =
        hasRight && drawerOpenRegion.bounds.Contains(rightPalmPose.position);
    bool rightInClose =
        hasRight && drawerCloseRegion.bounds.Contains(rightPalmPose.position);
    bool leftInOpen =
        hasLeft && drawerOpenRegion.bounds.Contains(leftPalmPose.position);
    bool leftInClose =
        hasLeft && drawerCloseRegion.bounds.Contains(leftPalmPose.position);

    switch (mode)
    {
      case DrawerMode.None:
        // decide drawer action - right hand takes priority, then left
        if (rightInOpen)
        {
          mode = DrawerMode.Opening;
          activeHand = Handedness.Right;
          BeginOpenInteraction(rightPalmPose);
        }
        else if (leftInOpen)
        {
          mode = DrawerMode.Opening;
          activeHand = Handedness.Left;
          BeginOpenInteraction(leftPalmPose);
        }
        else if (rightInClose)
        {
          mode = DrawerMode.Closing;
          activeHand = Handedness.Right;
          BeginCloseInteraction(rightPalmPose);
        }
        else if (leftInClose)
        {
          mode = DrawerMode.Closing;
          activeHand = Handedness.Left;
          BeginCloseInteraction(leftPalmPose);
        }
        break;

      case DrawerMode.Opening:
        if (!d_handService.TryGetPalmPose(activeHand, out var openPose))
        {
          EndInteraction();
          return;
        }

        bool handInsideOpenRegion =
            drawerOpenRegion.bounds.Contains(openPose.position);
        if (handInsideOpenRegion)
        {
          OpenDrawerOnPalmPose(openPose);
        }
        else
        {
          EndInteraction();
        }
        break;

      case DrawerMode.Closing:
        if (!d_handService.TryGetPalmPose(activeHand, out var closePose))
        {
          EndInteraction();
          return;
        }

        bool handInsideCloseRegion =
            drawerCloseRegion.bounds.Contains(closePose.position);
        if (handInsideCloseRegion)
        {
          CloseDrawerOnPalmPose(closePose);
        }
        else
        {
          EndInteraction();
        }
        break;
    }
  }

  private void BeginOpenInteraction(Pose palmPose)
  {
    palmStartOpenZ = palmPose.position.z;            // world z at grab
    drawerStartOpenZ = drawerVisual.localPosition.z; // local z at grab
  }

  private void OpenDrawerOnPalmPose(Pose palmPose)
  {
    // hands are tracked in world space, drawers slide in local
    float palmZ = palmPose.position.z;

    // how much hand has moved along world z
    float palmDeltaZ = palmZ - palmStartOpenZ;
    Debug.Log($"==PALM DELTA== : {palmDeltaZ}");

    if (palmDeltaZ >= 0f)
    {
      Debug.LogWarning($"Cannot open in that direction");
      return;
    }

    // map delta into drawer local Z motion
    float targetZ = drawerStartOpenZ + palmDeltaZ * openMovementScale;

    // apply to visuals
    Vector3 local = drawerVisual.localPosition;
    local.z = targetZ;
    drawerVisual.localPosition = local;
  }

  private void BeginCloseInteraction(Pose palmPose)
  {
    palmStartCloseZ = palmPose.position.z;
    drawerStartCloseZ = drawerVisual.localPosition.z;
  }

  private void CloseDrawerOnPalmPose(Pose palmPose)
  {
    float palmZ = palmPose.position.z;
    float palmDeltaZ = palmZ - palmStartCloseZ;

    if (palmDeltaZ <= 0f)
    {
      Debug.LogWarning($"Cannot open in that direction");
      return;
    }

    float targetZ = drawerStartCloseZ + palmDeltaZ * closeMovementScale;

    Vector3 local = drawerVisual.localPosition;
    local.z = targetZ;
    drawerVisual.localPosition = local;
  }

  private void EndInteraction() { mode = DrawerMode.None; }
}
