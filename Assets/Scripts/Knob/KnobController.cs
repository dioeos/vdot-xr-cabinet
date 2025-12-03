/**
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragGroup1 : MonoBehaviour
{
    private float zDistance;
    private float InitialPos;

    private Vector3 GetMouseWorldPosition(){
        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = zDistance;
        return Camera.main.ScreenToWorldPoint(mouseScreenPos);
    }

private void OnMouseDown()
{

        Debug.Log("mouse down on : " + gameObject.name);
        zDistance = Camera.main.WorldToScreenPoint(transform.position).z;
        InitialPos = GetMouseWorldPosition().y;
}

private void OnMouseDrag()
{

    float yDiff = GetMouseWorldPosition().y ;

    float delta  = 0f;
    if(InitialPos >  yDiff )
    {
        delta = -1f;
    }

    if( InitialPos < yDiff )
    {
        delta = 1f;
    }

    Debug.Log("gameobject : " + gameObject.transform.position.y );
    Debug.Log("mouse position : " + GetMouseWorldPosition().y);
    transform.localEulerAngles = new Vector3(transform.localEulerAngles.x,
transform.localEulerAngles.y , transform.localEulerAngles.z+ delta); InitialPos
= yDiff;
}

}
**/

using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class DragGroup1_visionOS : MonoBehaviour {
  private float initialTouchY;
  private UnityEngine.XR.Interaction.Toolkit.Interactors
      .IXRSelectInteractor interactor;
  private bool isDragging = false;

  // Called when the user touches/selects the object
  public void OnSelectEntered(SelectEnterEventArgs args) {
    interactor = args.interactorObject as UnityEngine.XR.Interaction.Toolkit
                     .Interactors.IXRSelectInteractor;

    if (interactor == null)
      return;

    isDragging = true;

    Vector3 worldPos = interactor.transform.position;
    initialTouchY = worldPos.y;

    Debug.Log("Touch down on: " + gameObject.name);
  }

  // Called when touch ends
  public void OnSelectExited(SelectExitEventArgs args) {
    isDragging = false;
    interactor = null;

    Debug.Log("Touch released.");
  }

  private void Update() {
    if (!isDragging || interactor == null)
      return;

    Vector3 worldPos = interactor.transform.position;
    float yDiff = worldPos.y;

    float delta = 0f;

    if (initialTouchY > yDiff)
      delta = -1f;
    else if (initialTouchY < yDiff)
      delta = 1f;

    transform.localEulerAngles =
        new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y,
                    transform.localEulerAngles.z + delta);

    initialTouchY = yDiff;
  }
}
