using UnityEngine;
using Eyeware.BeamEyeTracker.Unity;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
public class MaskController : BeamEyeTrackerMonoBehaviour
{
    public GameObject MaskReference;
    public GameObject ColourBackground;
    public GameObject LocationShadow;
    public GameObject InteractPrompt;
    public GameObject EnemyData;

    public bool IN_DEBUG = false;

    //PRIVATE ATTRIBUTES
    private NewColour CurrentMaskData;
    private List<GameObject> MaskedObjects = new List<GameObject>();
    private LoadEnemy EnemyDataLoader;

    void Start()
    {
        EnemyDataLoader =  EnemyData.GetComponent<LoadEnemy>();
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

          // TODO: Smmoth mask position as oppossed to setting the position (lerp)
        MaskReference.transform.position = new Vector3(WorldPos.x, WorldPos.y, 0);
    }

    //DEBUG FOR MY SANITY
    private void MapMaskPosition_Mouse()
    {
        float MouseX = Mouse.current.position.ReadValue().x;
        float MouseY = Mouse.current.position.ReadValue().y;

        Vector2 ClampToCameraView = Camera.main.ScreenToWorldPoint(new Vector2(MouseX, MouseY));
        MaskReference.transform.position = new Vector3(ClampToCameraView.x, ClampToCameraView.y, 0);
    }

    private void UpdateUINewMask()
    {
        // Set the colour of features to the colour of the mask
        TextMeshProUGUI ColourBacking = LocationShadow.GetComponent<TextMeshProUGUI>();
        ColourBacking.color = CurrentMaskData.TintColour;
    }

    public void SetActiveMaskData(NewColour MaskData)
    {
        CurrentMaskData = MaskData;
        
        ColourBackground.GetComponent<SpriteRenderer>().color    = CurrentMaskData.TintColour;
        ColourBackground.GetComponent<SpriteRenderer>().sprite   = CurrentMaskData.BackgroundSprite;
        MaskReference.GetComponent<SpriteMask>().sprite          = CurrentMaskData.MaskShape;

        // SET UP DISABLING/ ENABLING OBJECTS IN THE MASK
        foreach(GameObject Object in MaskedObjects)
        {
            Object.GetComponent<SpriteRenderer>().color = new Color(0,0,0,0);

            // hide the interact prompt
            InteractPrompt.gameObject.SetActive(false);

            foreach(NewEnemy enemy in CurrentMaskData.InteractableEnemies)
            {
                if(enemy.WorldReference.name == Object.name)
                {
                    Object.GetComponent<SpriteRenderer>().color = enemy.SpriteColour;

                    //show and position the interact prompt
                    InteractPrompt.gameObject.SetActive(true);
                }
            }
        }

        // Update any ui changes based on the colour mask changing
        UpdateUINewMask();
    }

    public void TryInteractWith()
    {
        // Check if there is an object in the mask
        foreach(GameObject Object in MaskedObjects)
        {
            foreach(NewEnemy enemy in CurrentMaskData.InteractableEnemies)
            {
                if(enemy.WorldReference.name == Object.name)
                {
                    // todo pass the enemy into the combat scene
                    EnemyDataLoader.EnemyToLoad = enemy;
                    SceneManager.LoadScene("Combat");
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(!CurrentMaskData){return;}
        GameObject Object = collision.gameObject;
        MaskedObjects.Add(Object);

        foreach(NewEnemy enemy in CurrentMaskData.InteractableEnemies)
        {
            if(enemy.WorldReference.name == Object.name)
            {
                Object.GetComponent<SpriteRenderer>().color = enemy.SpriteColour;
                InteractPrompt.gameObject.SetActive(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if(!CurrentMaskData){return;}

        GameObject Object = collision.gameObject;               
        Object.GetComponent<SpriteRenderer>().color = new Color(0,0,0,0);
        MaskedObjects.Remove(Object);
        InteractPrompt.gameObject.SetActive(false);
    }
}