using UnityEngine;
using Eyeware.BeamEyeTracker.Unity;
using UnityEngine.UI;

public class MaskController : BeamEyeTrackerMonoBehaviour
{
    public GameObject MaskReference;
    public GameObject ColourBackground;
    

    private NewColour CurrentMaskData;

    void Start()
    {
        
    }

    // map the position of the mask to the position of the eye tracker
    void Update()
    {
        if(betInputDevice == null){return;}

        Vector2 gazeValue = betInputDevice.viewportGazePosition.ReadValue();
        // Clamp gaze position to viewport bounds (0-1)
        gazeValue.x = Mathf.Clamp01(gazeValue.x);
        gazeValue.y = Mathf.Clamp01(gazeValue.y);

        MaskReference.transform.position = new Vector2(gazeValue.x * Screen.width, gazeValue.y * Screen.height);

    }

    public void SetActiveMaskData(NewColour MaskData)
    {
        CurrentMaskData = MaskData;
        
        ColourBackground.GetComponent<SpriteRenderer>().color    = CurrentMaskData.TintColour;
        MaskReference.GetComponent<SpriteMask>().sprite          = CurrentMaskData.MaskShape;
    }
}