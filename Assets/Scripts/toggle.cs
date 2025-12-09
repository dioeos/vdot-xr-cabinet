using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ToggleVisibility : MonoBehaviour
{
    bool isVisible = true;
    Renderer[] renderers;

    void Start()
    {
        renderers = GetComponentsInChildren<Renderer>();
        SetVisible(isVisible);
    }

    public void OnSelectEntered(SelectEnterEventArgs args)
    {
        isVisible = !isVisible;
        SetVisible(isVisible);
    }

    void SetVisible(bool value)
    {
        foreach (var r in renderers)
            r.enabled = value;
    }
}
