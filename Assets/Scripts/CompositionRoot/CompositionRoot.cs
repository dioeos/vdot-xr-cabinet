using UnityEngine;

public class CompositionRoot : MonoBehaviour
{
  [Header("Services")]
  [SerializeField]
  HandTrackingManager handTrackingManager;

  [Header("Consumers")]
  [SerializeField]
  DrawerController[] drawers;

  // [SerializeField]
  // DoorHandleController[] handles;

  void Awake()
  {
    foreach (var drawer in drawers)
    {
      drawer.Initialize(handTrackingManager);
    }
    // foreach (var handle in handles)
    // {
    //   handle.Initialize(handTrackingManager);
    // }
  }
}
