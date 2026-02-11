using UnityEngine;
using Eyeware.BeamEyeTracker.Unity;
using UnityEngine.UI;

public class MaskController : BeamEyeTrackerMonoBehaviour
{
    public GameObject MaskReference;

    void Start()
    {
        
    }

    // GET THE CENTRAL EYE POSITION OF THE PLAYER, MOVE THE MASK (smooth it)
    void Update()
    {
        if(betInputDevice == null){return;}

        Vector2 gazeValue = betInputDevice.viewportGazePosition.ReadValue();
          // Clamp gaze position to viewport bounds (0-1)
        gazeValue.x = Mathf.Clamp01(gazeValue.x);
        gazeValue.y = Mathf.Clamp01(gazeValue.y);

        MaskReference.transform.position = new Vector2(gazeValue.x * Screen.width, gazeValue.y * Screen.height);

    }
}
