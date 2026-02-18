using UnityEngine;
using UnityEngine.InputSystem;

public class SceneController : MonoBehaviour
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



    public void Start()
    {
        BlueMask     =  InputSystem.actions.FindAction("EquipBlue");
        RedMask      =  InputSystem.actions.FindAction("EquipRed");
        YellowMask   =  InputSystem.actions.FindAction("EquipYellow");

        _MASKCONTROLLER = MaskReference.GetComponent<MaskController>();
    }

    public void Update()
    {
        // MASK CONTROLS
        if(BlueMask.WasPressedThisFrame())      { _MASKCONTROLLER.SetActiveMaskData(BlueData);}
        if(RedMask.WasPressedThisFrame())       { _MASKCONTROLLER.SetActiveMaskData(RedData);}
        if(YellowMask.WasPressedThisFrame())    { _MASKCONTROLLER.SetActiveMaskData(YellowData);}

        // CAMERA CONTROLS (mapped to eyetracking)
    }

}
