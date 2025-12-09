using UnityEngine;

public class HideOnPhysicalTouch : MonoBehaviour
{
    [Header("Object that should appear when this disappears")]
    public GameObject objectToShow;     // assign in Inspector

    private bool hasTriggered = false;  // prevents double-triggering

    void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;

        if (other.CompareTag("Hand"))
        {
            hasTriggered = true;

            // Hide THIS object's renderers and colliders
            foreach (var r in GetComponentsInChildren<Renderer>())
                r.enabled = false;

            foreach (var c in GetComponentsInChildren<Collider>())
                c.enabled = false;

            // Show the "hand_" object (tagged or referenced)
            if (objectToShow != null)
            {
                objectToShow.SetActive(true);
            }
            else
            {
                Debug.LogWarning("HideOnPhysicalTouch: No object assigned to 'objectToShow'");
            }
        }
    }
}
