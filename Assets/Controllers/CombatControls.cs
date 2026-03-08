using UnityEngine;
using UnityEngine.InputSystem;
using Eyeware.BeamEyeTracker.Unity;
using Eyeware.BeamEyeTracker;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;

public class CombatControls : BeamEyeTrackerMonoBehaviour
{

    public GameObject MaskReference;

    public NewColour BlueData;
    public NewColour RedData;
    public NewColour YellowData;

    private WeakpointController _WEAKPOINTCONTROLLER;
    private Vector3 CameraVelocity = new Vector3(0, 0, 2);


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
        _WEAKPOINTCONTROLLER = MaskReference.GetComponent<WeakpointController>();
    }

    public void Update()
    {
        // MASK CONTROLS
        // if(BlueMask.WasPressedThisFrame())      { _MASKCONTROLLER.SetActiveMaskData(BlueData);}
        // if(RedMask.WasPressedThisFrame())       { _MASKCONTROLLER.SetActiveMaskData(RedData);}
        // if(YellowMask.WasPressedThisFrame())    { _MASKCONTROLLER.SetActiveMaskData(YellowData);}
        // if(Interact.WasPressedThisFrame())      { _MASKCONTROLLER.TryInteractWith();}

    }
}
