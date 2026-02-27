using UnityEngine;
using Eyeware.BeamEyeTracker.Unity;
using TMPro;
using UnityEngine.UI;

public class CombatController :  BeamEyeTrackerMonoBehaviour
{
    public GameObject CombatUI;
    private GameObject EnemyLoaded;
    private LoadEnemy EnemyLoadScript;
    public void Start()
    {
        // Get the enemy loaded
        EnemyLoaded = GameObject.Find("EnemyData");
        EnemyLoadScript = EnemyLoaded.GetComponent<LoadEnemy>();
        Debug.Log("Starting combat with " + EnemyLoadScript.EnemyToLoad.DisplayName);

        InitialiseCombatUI(EnemyLoadScript.EnemyToLoad);
    }

    private void InitialiseCombatUI(NewEnemy EnemyLoaded)
    {
        // ENEMY NAME
        TextMeshProUGUI DisplayName          = CombatUI.gameObject.transform.Find("Heading").gameObject.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI DisplayName_Shadow   = CombatUI.gameObject.transform.Find("Shadow").gameObject.GetComponent<TextMeshProUGUI>();

        DisplayName.text         = EnemyLoaded.DisplayName;
        DisplayName_Shadow.text  = EnemyLoaded.DisplayName;
        DisplayName.color        = EnemyLoaded.SpriteColour;

        // ENEMY ICON
        Image EnemySprite            = CombatUI.gameObject.transform.Find("EnemySprite").gameObject.GetComponent<Image>();
        Image EnemySprite_Shadow     = CombatUI.gameObject.transform.Find("EnemyShadow").gameObject.GetComponent<Image>();
        
        EnemySprite.sprite                        = EnemyLoaded.FullSprite;
        EnemySprite_Shadow.sprite                 = EnemyLoaded.FullSprite;
        EnemySprite.transform.localScale          = EnemyLoaded.OverrideSpriteScale;
        EnemySprite_Shadow.transform.localScale   = EnemyLoaded.OverrideSpriteScale;
    }
}
