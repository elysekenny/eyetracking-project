using UnityEngine;
using UnityEngine.InputSystem;
using Eyeware.BeamEyeTracker.Unity;
using Eyeware.BeamEyeTracker;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;
using TMPro;
using System;

// TODO- game start popup. just a window that says how to play and the premise of how to win/ lose

public class SceneController : BeamEyeTrackerMonoBehaviour
{

    public GameObject MaskReference;
    public GameObject DataPanel;

    public NewColour BlueData;
    public NewColour RedData;
    public NewColour YellowData;

    public GameObject HelpPrompt;
    public GameObject OnScreenUI;
    public GameObject ThreatLevel;
    public GameObject[] AllEnemies;

    private MaskController _MASKCONTROLLER;


    // READS THE PLAYER INPUTS
    InputAction BlueMask;
    InputAction RedMask;
    InputAction YellowMask;
    InputAction Interact;


    public void Start()
    {
        PlayerPrefs.SetString("Gamestate", "Start"); 

        // USER INPUTS
        BlueMask     =  InputSystem.actions.FindAction("EquipBlue");
        RedMask      =  InputSystem.actions.FindAction("EquipRed");
        YellowMask   =  InputSystem.actions.FindAction("EquipYellow");
        Interact     =  InputSystem.actions.FindAction("Interact");

        // REFERENCES
        _MASKCONTROLLER = MaskReference.GetComponent<MaskController>();

        // set the area report to the saved data
        TextMeshProUGUI BluebellText         = DataPanel.gameObject.transform.Find("Bluebells Remaining")           .gameObject.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI BluebellTextShadow   = DataPanel.gameObject.transform.Find("Bluebells Remaining Shadow")    .gameObject.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI RoseText             = DataPanel.gameObject.transform.Find("Roses Remaining")               .gameObject.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI RoseTextShadow       = DataPanel.gameObject.transform.Find("Roses Remaining Shadow")        .gameObject.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI SunflowerText        = DataPanel.gameObject.transform.Find("Sunflower Remaining")           .gameObject.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI SunflowerTextShadow  = DataPanel.gameObject.transform.Find("Sunflower Remaining Shadow")    .gameObject.GetComponent<TextMeshProUGUI>();

        BluebellText.text         = PlayerPrefs.GetString("BLUEBELL_REMAINING", "1");
        BluebellTextShadow.text   = PlayerPrefs.GetString("BLUEBELL_REMAINING", "1");

        RoseText.text             = PlayerPrefs.GetString("ROSE_REMAINING", "2");
        RoseTextShadow.text       = PlayerPrefs.GetString("ROSE_REMAINING", "2");

        SunflowerText.text        = PlayerPrefs.GetString("SUNFLOWER_REMAINING", "2");
        SunflowerTextShadow.text  = PlayerPrefs.GetString("SUNFLOWER_REMAINING", "2");

        if(PlayerPrefs.GetInt("TotalEnemies") <= 1)
        {
            // threat level = low
            ThreatLevel.GetComponent<TextMeshProUGUI>().text = "LOW";
            ThreatLevel.GetComponent<TextMeshProUGUI>().color = new Color(0.85f, 1, 0, 1);

        }
        else if (PlayerPrefs.GetInt("TotalEnemies") <= 3)
        {
            // threat level = medium
            ThreatLevel.GetComponent<TextMeshProUGUI>().text = "MEDIUM";
            ThreatLevel.GetComponent<TextMeshProUGUI>().color = new Color(1, 0.5f, 0, 1);

        }
        else
        {
            // threat level = high
            ThreatLevel.GetComponent<TextMeshProUGUI>().text = "HIGH";
            ThreatLevel.GetComponent<TextMeshProUGUI>().color = new Color(1, 0, 0, 1);
        }

        // check the load condition (if there is one)
        if(PlayerPrefs.GetString("LoadCase") == "START")
        {
            HelpPrompt.SetActive(true);
            OnScreenUI.SetActive(false);
        }
        else if(PlayerPrefs.GetString("LoadCase") == "REPLAY")
        {
            // reset all the enemies in the scene
            foreach(GameObject _enemy in AllEnemies)
            {
                _enemy.SetActive(true);
            }
        }

        // remove the defeated enemy from the world
        string EnemyToRemove = PlayerPrefs.GetString("CurrentEnemy", "None");
        for(int i = 0; i < AllEnemies.Length; i++)
        {
            if(AllEnemies[i].name == EnemyToRemove)
            {
                AllEnemies[i].SetActive(false);
            }
        }

        // check for win condition
        if(PlayerPrefs.GetInt("TotalEnemies") <= 0)
        {
            Debug.Log("Win condition!");
            PlayerPrefs.SetString("Gamestate", "Win");
            SceneManager.LoadScene("UI Screens");
        }
    }

    public void Update()
    {
        // MASK CONTROLS
        if(!HelpPrompt.activeSelf)
        {
            if(BlueMask.WasPressedThisFrame())      { _MASKCONTROLLER.SetActiveMaskData(BlueData);}
            if(RedMask.WasPressedThisFrame())       { _MASKCONTROLLER.SetActiveMaskData(RedData);}
            if(YellowMask.WasPressedThisFrame())    { _MASKCONTROLLER.SetActiveMaskData(YellowData);}
        }


        if(Interact.WasPressedThisFrame())     
         { 
            if(HelpPrompt.activeSelf)
            {
                HelpPrompt.SetActive(false);
                OnScreenUI.SetActive(true);
            }
            else
            {
                _MASKCONTROLLER.TryInteractWith();
            }
        }

        // CAMERA CONTROLS (mapped to eyetracking)
        if(!HelpPrompt.activeSelf){MapGazeDirection();}
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
        Vector3 NewCameraPos =  new Vector3(Camera.main.transform.position.x + xMove, Camera.main.transform.position.y + yMove, Camera.main.transform.position.z);
        Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, NewCameraPos, 0.5f);
    }

}
