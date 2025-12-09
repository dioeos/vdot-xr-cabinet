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

  // USE CLAMP TO LIMIT RANGE

  [Tooltip("The transform of the drawer visuals")]
  [SerializeField]
  private Transform drawerVisual;

  private DrawerMode mode = DrawerMode.None;
  private Handedness activeHand;

  private float palmStartOpenX;
  private float drawerStartOpenX;

  private float palmStartCloseX;
  private float drawerStartCloseX;

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
    palmStartOpenX = palmPose.position.x;            // world x at grab
    drawerStartOpenX = drawerVisual.localPosition.x; // local x at grab
  }

  private void OpenDrawerOnPalmPose(Pose palmPose)
  {
    // hands are tracked in world space, drawers slide in local
    float palmX = palmPose.position.x;

    // how much hand has moved along world x
    float palmDeltaX = palmX - palmStartOpenX;
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
    palmStartCloseX = palmPose.position.x;
    drawerStartCloseX = drawerVisual.localPosition.x;
  }

  private void CloseDrawerOnPalmPose(Pose palmPose)
  {
    float palmX = palmPose.position.x;
    float palmDeltaX = palmX - palmStartCloseX;

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
}
