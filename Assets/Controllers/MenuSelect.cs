using UnityEngine;
using Eyeware.BeamEyeTracker.Unity;
using UnityEngine.InputSystem;
using Unity.VisualScripting;

public class MenuSelect : BeamEyeTrackerMonoBehaviour
{
    public bool IN_DEBUG;
    void Update()
    {
        if(IN_DEBUG){MapMaskPosition_Mouse();}    
        else{MapMaskPosition_Eye();}
    }

    private void MapMaskPosition_Eye()
    {
        // ignore if there is no eye tracking device connected
        if(betInputDevice == null){return;}

        Vector2 gazeValue = betInputDevice.viewportGazePosition.ReadValue();
        gazeValue.x = Mathf.Clamp01(gazeValue.x);
        gazeValue.y = Mathf.Clamp01(gazeValue.y);

        // TRANSLATE THE SCREEN POSITION TO WITHIN THE BOUNDS OF THE CAMERA VIEWPORT
        Vector2 FullScreenPosition   = new Vector2(gazeValue.x *  Screen.width, gazeValue.y * Screen.height);
        Vector2 WorldPos             = Camera.main.ScreenToWorldPoint(FullScreenPosition);

        Vector3 NewPos = new Vector3(WorldPos.x, WorldPos.y, 0);
        this.transform.position = Vector3.Lerp(this.transform.position, NewPos, 0.5f);
    }

    //DEBUG FOR MY SANITY
    private void MapMaskPosition_Mouse()
    {
        float MouseX = Mouse.current.position.ReadValue().x;
        float MouseY = Mouse.current.position.ReadValue().y;

        Vector2 ClampToCameraView = Camera.main.ScreenToWorldPoint(new Vector2(MouseX, MouseY));
        Vector3 NewPos = new Vector3(ClampToCameraView.x, ClampToCameraView.y, 0);
        this.transform.position = Vector3.Lerp(this.transform.position, NewPos, 0.5f);
    }
}
