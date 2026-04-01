using UnityEngine;
using Eyeware.BeamEyeTracker.Unity;


public class MenuSelect : BeamEyeTrackerMonoBehaviour
{
    void Update()
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
}
