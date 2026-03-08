using UnityEngine;
using Eyeware.BeamEyeTracker.Unity;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
public class WeakpointController : BeamEyeTrackerMonoBehaviour
{
    public GameObject MaskReference;
    public bool IN_DEBUG = false;

    //PRIVATE ATTRIBUTES
    private NewColour CurrentMaskData;

    void Start()
    {
        
    }

    // map the position of the mask to the position of the eye tracker
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
        MaskReference.transform.position = Vector3.Lerp(MaskReference.transform.position, NewPos, 0.5f);
    }

    //DEBUG FOR MY SANITY
    private void MapMaskPosition_Mouse()
    {
        float MouseX = Mouse.current.position.ReadValue().x;
        float MouseY = Mouse.current.position.ReadValue().y;

        Vector2 ClampToCameraView = Camera.main.ScreenToWorldPoint(new Vector2(MouseX, MouseY));
        Vector3 NewPos = new Vector3(ClampToCameraView.x, ClampToCameraView.y, 0);
        MaskReference.transform.position = Vector3.Lerp(MaskReference.transform.position, NewPos, 0.5f);
    }

    private void UpdateUINewMask()
    {
        // Set the colour of features to the colour of the mask
     
    }

    public void SetActiveMaskData(NewColour MaskData)
    {
        CurrentMaskData = MaskData;
        
        //SET THE COLOUR AND LOAD THE MASK DATA
        // Update any ui changes based on the colour mask changing
        UpdateUINewMask();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(!CurrentMaskData){return;}
        GameObject Object = collision.gameObject;
       
       // check if the weakpoint is hovered, if it is then enable the special combat action
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if(!CurrentMaskData){return;}

        GameObject Object = collision.gameObject;               
       
       // disable the weakpoint option
    }
}