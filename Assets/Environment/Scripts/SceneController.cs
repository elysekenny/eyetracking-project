using UnityEngine;
using UnityEngine.InputSystem;
using Eyeware.BeamEyeTracker.Unity;
using Eyeware.BeamEyeTracker;
using Unity.VisualScripting;

public class SceneController : BeamEyeTrackerMonoBehaviour
{

    public GameObject MaskReference;

    public NewColour BlueData;
    public NewColour RedData;
    public NewColour YellowData;

    private MaskController _MASKCONTROLLER;


    // READS THE PLAYER INPUTS
    InputAction BlueMask;
    InputAction RedMask;
    InputAction YellowMask;
    InputAction Interact;


    public void Start()
    {
        // USER INPUTS
        BlueMask     =  InputSystem.actions.FindAction("EquipBlue");
        RedMask      =  InputSystem.actions.FindAction("EquipRed");
        YellowMask   =  InputSystem.actions.FindAction("EquipYellow");
        Interact     =  InputSystem.actions.FindAction("Interact");

        // REFERENCES
        _MASKCONTROLLER = MaskReference.GetComponent<MaskController>();
    }

    public void Update()
    {
        // MASK CONTROLS
        if(BlueMask.WasPressedThisFrame())      { _MASKCONTROLLER.SetActiveMaskData(BlueData);}
        if(RedMask.WasPressedThisFrame())       { _MASKCONTROLLER.SetActiveMaskData(RedData);}
        if(YellowMask.WasPressedThisFrame())    { _MASKCONTROLLER.SetActiveMaskData(YellowData);}
        if(Interact.WasPressedThisFrame())      { _MASKCONTROLLER.TryInteractWith();}

        // CAMERA CONTROLS (mapped to eyetracking)
        MapGazeDirection();

    }

    private void MapHeadMovement()
    {
        if(betInputDevice == null){return;}
        //TODO: HeadPose integration would be nice
    }

    private void MapGazeDirection()
    {
        if(betInputDevice == null){return;}

        Vector2 gazeValue = betInputDevice.viewportGazePosition.ReadValue();
        gazeValue.x = Mathf.Clamp01(gazeValue.x);
        gazeValue.y = Mathf.Clamp01(gazeValue.y);

        Vector2 FullScreenPosition      = new Vector2(gazeValue.x *  Screen.width, gazeValue.y * Screen.height);
        Vector2 CameraSpace             = Camera.main.WorldToViewportPoint(FullScreenPosition);

        float xMove = 0;
        float yMove = 0;

        if(CameraSpace.x <= 0){xMove = -1;}
        if(CameraSpace.x >= 1){xMove = 1;}
        if(CameraSpace.y <= 0){yMove = -1;}
        if(CameraSpace.y >= 1){yMove = 1;}

        // Debug.Log("// X: " + CameraSpace.x + "// Y: " + CameraSpace.y);
        // TODO: Smmoth camera movement as oppossed to setting the position (lerp)
        Camera.main.transform.position = new Vector3(Camera.main.transform.position.x + xMove, Camera.main.transform.position.y + yMove, Camera.main.transform.position.z);
    }

}
