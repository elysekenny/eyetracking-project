using UnityEngine;
using Eyeware.BeamEyeTracker.Unity;
using UnityEngine.UI;
using Unity.VisualScripting;
using System.Collections.Generic;

public class MaskController : BeamEyeTrackerMonoBehaviour
{
    public GameObject MaskReference;
    public GameObject ColourBackground;
    public GameObject MainCamera;


    //PRIVATE ATTRIBUTES
    private NewColour CurrentMaskData;
    private List<GameObject> MaskedObjects = new List<GameObject>();

    void Start()
    {
        
    }

    // map the position of the mask to the position of the eye tracker
    void Update()
    {
        MapMaskPosition();
    }

    private void MapMaskPosition()
    {
        // ignore if there is no eye tracking device connected
        if(betInputDevice == null){return;}

        Vector2 gazeValue = betInputDevice.viewportGazePosition.ReadValue();
        // Clamp gaze position to viewport bounds (0-1)
        gazeValue.x = Mathf.Clamp01(gazeValue.x);
        gazeValue.y = Mathf.Clamp01(gazeValue.y);

        // TRANSLATE THE SCREEN POSITION TO WITHIN THE BOUNDS OF THE CAMERA VIEWPORT
        Vector2 FullScreenPosition   = new Vector2(gazeValue.x *  Screen.width, gazeValue.y * Screen.height);
        Vector2 ClampToCameraView    = Camera.main.WorldToViewportPoint(FullScreenPosition);
        ClampToCameraView.x          = Mathf.Clamp01(ClampToCameraView.x);
        ClampToCameraView.x          = Mathf.Clamp01(ClampToCameraView.x);

        MaskReference.transform.position = new Vector3(Camera.main.ViewportToWorldPoint(ClampToCameraView).x, Camera.main.ViewportToWorldPoint(ClampToCameraView).y, 0);
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
            foreach(NewEnemy enemy in CurrentMaskData.InteractableEnemies)
            {
                if(enemy.WorldReference.name == Object.name)
                {
                    Object.GetComponent<SpriteRenderer>().color = enemy.SpriteColour;
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
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if(!CurrentMaskData){return;}

        GameObject Object = collision.gameObject;               
        Object.GetComponent<SpriteRenderer>().color = new Color(0,0,0,0);
        MaskedObjects.Remove(Object);
    }
}