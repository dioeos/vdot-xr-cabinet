using System.Collections;
using UnityEngine;
using UnityEngine.XR.Hands;

internal enum DoorHandleState { MinPos, Rotating, MaxPos }

public class DoorHandleController : MonoBehaviour
{

  private DoorHandleState handleState;
  private Rigidbody rb;

  [Header("Rotation Object References")]
  [Tooltip("Collider region that joints must be in to rotate door handle")]
  [SerializeField]
  private BoxCollider doorHandleRegion;

  [Tooltip("Visuals to rotate")]
  [SerializeField]
  private Transform handleVisual;

  [Tooltip("Hinge joint that constrains the handle rotation")]
  [SerializeField]
  private HingeJoint hinge;

  [Header("Hand components")]
  private IHandTrackingService d_handService;
  private Handedness activeHand;

  [Header("Rotation Confiugration Variables ")]
  [Tooltip("Min angle at rest, in *local* hinge space (usually 0).")]
  [SerializeField]
  private float localMinAngle = 0f; // e.g. handle horizontal

  [Tooltip("Max angle relative to rest (e.g. 90 = fully turned).")]
  [SerializeField]
  private float localMaxAngle = 90f; // e.g. 90 deg down

  [Tooltip("How many degrees of wrist tilt map to full handle rotation.")]
  [SerializeField]
  private float maxTwistDegrees = 20f; // wrist tilt from 0 → localMaxAngle

  [Header("Torque Controller")]
  private float kp = 30f;
  private float kd = 4f;
  private float maxTorque = 50f; // clamp torque

  // angle of hinge at Start; this is our "zero" reference in *hinge space*
  private float hingeZeroAngle;

  // absolute min/max in hinge.angle space (what the joint actually uses)
  private float absMinAngle;
  private float absMaxAngle;

  // target angle in absolute hinge space
  private float targetAbsAngleDeg;

  // middle inter joint angle at grab start (in handle-local space)
  private float startMiddleInterAngleDeg;

  // hinge axis in handle-local space
  private Vector3 hingeAxisLocal;

  public void Initialize(IHandTrackingService handService)
  {
    d_handService = handService;
  }

  void Start()
  {
    rb = GetComponent<Rigidbody>();
    if (rb == null)
    {
      Debug.LogError("DoorHandleController: No Rigidbody on handle!");
      return;
    }

    if (hinge == null)
    {
      Debug.LogError("DoorHandleController: HingeJoint not assigned.");
      return;
    }

    rb.isKinematic = false;
    rb.useGravity = false;

    if (handleVisual == null)
    {
      handleVisual = transform;
    }

    // Hinge axis in local space (as set on the HingeJoint)
    hingeAxisLocal = hinge.axis.normalized;

    // 1) Capture the hinge's current angle as our zero-reference.
    hingeZeroAngle = hinge.angle;

    // 2) Convert local min/max into absolute hinge-space angles.
    absMinAngle = hingeZeroAngle + localMinAngle;
    absMaxAngle = hingeZeroAngle + localMaxAngle;

    // 3) Configure the hinge limits in that same absolute space.
    JointLimits limits = hinge.limits;
    limits.min = absMinAngle;
    limits.max = absMaxAngle;
    hinge.limits = limits;
    hinge.useLimits = true;

    hinge.useMotor = false;
    hinge.useSpring = false;

    // 4) Initialize target to the current hinge angle (clamped).
    float clampedStart = Mathf.Clamp(hinge.angle, absMinAngle, absMaxAngle);
    targetAbsAngleDeg = clampedStart;
  }

  void FixedUpdate()
  {
    if (rb == null || hinge == null)
      return;

    if (handleState != DoorHandleState.Rotating)
      return;

    float current = hinge.angle;

    // error in degrees, shortest path
    float error = Mathf.DeltaAngle(current, targetAbsAngleDeg);

    float angularVel = hinge.velocity; // deg/sec

    float torqueScalar = kp * error - kd * angularVel;

    torqueScalar = Mathf.Clamp(torqueScalar, -maxTorque, maxTorque);

    Vector3 worldAxis = hinge.transform.TransformDirection(hinge.axis);
    rb.AddTorque(worldAxis * torqueScalar, ForceMode.Acceleration);
  }

  void Update()
  {
    if (d_handService == null || doorHandleRegion == null)
      return;

    bool rightInHandle = InColliderRegion(doorHandleRegion, Handedness.Right,
                                          out var openRightMiddleInter);

    bool leftInHandle = InColliderRegion(doorHandleRegion, Handedness.Left,
                                         out var openLeftMiddleInter);

    switch (handleState)
    {
      case DoorHandleState.MaxPos:
        if (rightInHandle && d_handService.IsFist(Handedness.Right))
        {
          Debug.LogWarning("Right in handle!");
          handleState = DoorHandleState.Rotating;
          activeHand = Handedness.Right;
          BeginRotateInteraction(openRightMiddleInter);
        }
        else if (leftInHandle && d_handService.IsFist(Handedness.Left))
        {
          handleState = DoorHandleState.Rotating;
          activeHand = Handedness.Left;
          BeginRotateInteraction(openLeftMiddleInter);
        }
        break;

      case DoorHandleState.Rotating:
        if (!InColliderRegion(doorHandleRegion, activeHand, out var rotatePose))
        {
          EndInteraction();
          return;
        }
        UpdateTargetAngleFromInter(rotatePose);
        break;
    }
  }

  // ---------------- HAND → TARGET ANGLE ----------------

  // Normalize an angle from 0..360 to -180..180
  private float NormalizeAngle(float angleDeg)
  {
    if (angleDeg > 180f)
      angleDeg -= 360f;
    return angleDeg;
  }

  private bool InColliderRegion(BoxCollider colliderRegion, Handedness hand,
                                out Pose middlePose)
  {
    middlePose = default;

    if (colliderRegion == null)
      return false;

    if (!d_handService.TryGetIndexIntermediateJoint(hand, out var ii_pose) ||
        !d_handService.TryGetMiddleIntermediateJoint(hand, out var mi_pose) ||
        !d_handService.TryGetRingIntermediateJoint(hand, out var ri_pose))
    {
      return false;
    }

    bool anyIn = colliderRegion.bounds.Contains(ii_pose.position) ||
                 colliderRegion.bounds.Contains(mi_pose.position) ||
                 colliderRegion.bounds.Contains(ri_pose.position);

    if (!anyIn)
      return false;

    middlePose = mi_pose;
    return true;
  }

  private void BeginRotateInteraction(Pose middleInterPose)
  {
    // Convert palm rotation into handle-local space
    // palmLocalRot = inverse(handleRot) * palmWorldRot
    Quaternion middleInterLocalRot =
        Quaternion.Inverse(transform.rotation) * middleInterPose.rotation;

    Vector3 miEuler = middleInterLocalRot.eulerAngles;

    // Use X as "wrist tilt" (pitch)
    // try Y or Z instead.
    float miAngle = NormalizeAngle(miEuler.x);

    startMiddleInterAngleDeg = miAngle;

    // sync target with current hinge angle (so it doesn't jump)
    float clampedCurrent = Mathf.Clamp(hinge.angle, absMinAngle, absMaxAngle);
    targetAbsAngleDeg = clampedCurrent;
  }

  private void UpdateTargetAngleFromInter(Pose middleInterPose)
  {
    Quaternion middleInterLocalRot =
        Quaternion.Inverse(transform.rotation) * middleInterPose.rotation;

    Vector3 miEuler = middleInterLocalRot.eulerAngles;

    // Same component as in BeginRotateInteraction
    float miAngle = NormalizeAngle(miEuler.x);

    float miDelta = miAngle - startMiddleInterAngleDeg;

    float t = Mathf.InverseLerp(0f, maxTwistDegrees, miDelta);
    float desiredLocalAngle = Mathf.Lerp(localMinAngle, localMaxAngle, t);

    // Convert local desired to absolute hinge-space angle
    float desiredAbsAngle = hingeZeroAngle + desiredLocalAngle;

    // Clamp to absolute limits
    targetAbsAngleDeg = Mathf.Clamp(desiredAbsAngle, absMinAngle, absMaxAngle);
  }

  private void EndInteraction()
  {
    handleState = DoorHandleState.MaxPos;

    float clampedCurrent = Mathf.Clamp(hinge.angle, absMinAngle, absMaxAngle);
    targetAbsAngleDeg = clampedCurrent;

    targetAbsAngleDeg = hingeZeroAngle + localMinAngle;
  }
}
