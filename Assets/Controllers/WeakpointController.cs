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
    public GameObject _SceneManager;
    private TurnController _CONTROLLER;

    //PRIVATE ATTRIBUTES
    void Start()
    {
        _CONTROLLER = _SceneManager.GetComponent<TurnController>();
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



    private void OnTriggerEnter2D(Collider2D collision)
    {     
       // check if the weakpoint is hovered, if it is then enable the special combat action
       _CONTROLLER.SetWeakpointHighlight(true);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
       // disable the weakpoint option
        _CONTROLLER.SetWeakpointHighlight(false);
    }
}