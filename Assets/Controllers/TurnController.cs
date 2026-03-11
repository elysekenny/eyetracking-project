using UnityEngine;
using Eyeware.BeamEyeTracker.Unity;
using TMPro;
using UnityEngine.UI;
using Unity.VisualScripting;
using System;
using System.Diagnostics.Tracing;

/*

    Player health should be passed between combat scenes

 */

public class TurnController :  BeamEyeTrackerMonoBehaviour
{
    public GameObject CombatUI;
    private GameObject EnemyLoaded;
    private LoadEnemy EnemyLoadScript;
    private GameObject TimerBar;
    private GameObject PlayerHealthBar;
    private GameObject PlayerHealthBarTween;
    private GameObject EnemyHealthBar;
    private GameObject EnemyHealthBarTween;
    private NewEnemy CurrentEnemyData;

    // Active player data
    private float DefMod             = 1.0f; // multiply the enemy's attack by this value
    private float AtkMod             = 1.0f; // multiply by the players attack (when selecting special this will be 2.0f)
    private float BaseAttack         = 10.0f;
    private float AttackVariance     = 2.0f; // 8-12 dmg

    // TIMER STUFF
    private float TimeRemaining          = 0.0f; // this will be specifc to the enemy

    //Enumerators
    public enum COMBAT_STATES {NONE, PLAYER_TURN, ENEMY_TURN, PROCESS_RESULTS, START, END }
    public enum PLAYER_ACTIONS {NONE, ATTACK, DEFEND, SPECIAL}

    private COMBAT_STATES GameState = COMBAT_STATES.NONE;
    private PLAYER_ACTIONS PlayerAction = PLAYER_ACTIONS.NONE;

    private void SetupInput()
    {
        
    }

    public void Start()
    {
        // Get the enemy loaded
        EnemyLoaded          = GameObject.Find("EnemyData");
        EnemyLoadScript      = EnemyLoaded.GetComponent<LoadEnemy>();
        CurrentEnemyData     = EnemyLoadScript.EnemyToLoad;

        Debug.Log("Starting combat with " + CurrentEnemyData.DisplayName);

        SetupUIAttributes(); // set all the references to the ui attributes requried
        InitialiseCombatUI();

        // INTRO CUTSCENE
        GameState = COMBAT_STATES.START;
        PlayerTurn();

    }

    private void Update()
    {
        // Round timer visual thread
        // Health bar decrement animation
        if(TimeRemaining <= 0f && GameState == COMBAT_STATES.PLAYER_TURN)
        {
            EndTurn();
        }
        else if(GameState == COMBAT_STATES.PLAYER_TURN)
        {
            TimeRemaining -= Time.deltaTime;
            TimerBar.transform.localScale = new Vector3(Time.deltaTime * TimeRemaining, TimerBar.transform.localScale.y, TimerBar.transform.localScale.z);
        }

        if(GameState == COMBAT_STATES.PLAYER_TURN)
        {
            // only read the player input when in their turn
        }
    }

    private void InitialiseCombatUI()
    {
        // ENEMY NAME
        TextMeshProUGUI DisplayName          = CombatUI.gameObject.transform.Find("Heading").gameObject.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI DisplayName_Shadow   = CombatUI.gameObject.transform.Find("Shadow").gameObject.GetComponent<TextMeshProUGUI>();

        DisplayName.text         = CurrentEnemyData.DisplayName;
        DisplayName_Shadow.text  = CurrentEnemyData.DisplayName;
        DisplayName.color        = CurrentEnemyData.SpriteColour;

        // ENEMY ICON
        Image EnemySprite            = CombatUI.gameObject.transform.Find("EnemySprite").gameObject.GetComponent<Image>();
        Image EnemySprite_Shadow     = CombatUI.gameObject.transform.Find("EnemyShadow").gameObject.GetComponent<Image>();
        
        EnemySprite.sprite                        = CurrentEnemyData.FullSprite;
        EnemySprite_Shadow.sprite                 = CurrentEnemyData.FullSprite;
        EnemySprite.transform.localScale          = CurrentEnemyData.OverrideSpriteScale;
        EnemySprite_Shadow.transform.localScale   = CurrentEnemyData.OverrideSpriteScale;
    }

    private void SetupUIAttributes()
    {
        TimerBar              = CombatUI.gameObject.transform.Find("RoundTimer").gameObject.transform.Find("TimerBar").gameObject;

        // Player health assets
        PlayerHealthBar       = CombatUI.gameObject.transform.Find("PlayerHealth").gameObject.transform.Find("PlayerHealthBar").gameObject;
        PlayerHealthBarTween  = CombatUI.gameObject.transform.Find("PlayerHealth").gameObject.transform.Find("PlayerHealthAnimation").gameObject;

        // Enemy health assets
        EnemyHealthBar        = CombatUI.gameObject.transform.Find("EnemyHealth").gameObject.transform.Find("EnemyHealthBar").gameObject;
        EnemyHealthBarTween   = CombatUI.gameObject.transform.Find("EnemyHealth").gameObject.transform.Find("EnemyHealthAnimation").gameObject;
    }

    // can be used for player or enemy just needs to pass in the correct ui elements based on use case
    private void DecrementHealth(GameObject HealthBar, GameObject HealthAnim, float HealthRemainingPercent)
    {
        // health bar height = 1000 @ full hp and then decrement the ypos by the decrease amount / 2
        // tween the animation down to the target size of the health bar
        float NewHeight  = 1000 * (HealthRemainingPercent / 100); // 50% health means multiply by 0.5
        float NewYPos    = NewHeight / 2;

        HealthBar.transform.position     = new Vector3(HealthBar.transform.position.x, NewYPos, HealthBar.transform.position.z);
        HealthBar.transform.localScale   = new Vector3(1, HealthRemainingPercent / 100, 1);

        // lerp the values of the health anim to the new y and the new health percent
    }

    private void PlayerTurn()
    {
        // reset the modifiers to their default x1
        DefMod = 1.0f;
        AtkMod = 1.0f;

        // start the round timer
        TimeRemaining    = CurrentEnemyData.TurnDuration;
        GameState        = COMBAT_STATES.PLAYER_TURN; //starts the timer and lets input be read in the update loop
    }

    private void EndTurn()
    {
        // this handles when the turn timer runs out. will update the dialogue box as follows and have a second grace period before the next round starts
        Debug.Log("TIMER HAS RAN OUT");
        GameState = COMBAT_STATES.PROCESS_RESULTS;

        // PROCESS THE TURN RESULTS
    }
}
