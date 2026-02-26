using UnityEngine;
using Eyeware.BeamEyeTracker.Unity;

public class CombatController :  BeamEyeTrackerMonoBehaviour
{
    private NewEnemy EnemyLoaded;
    public void Awake()
    {
        // Get the enemy loaded
        EnemyLoaded = GameObject.Find("EnemyData").GetComponent<LoadEnemy>().EnemyToLoad;
        Debug.Log("Starting combat with " + EnemyLoaded.DisplayName);
    }
}
