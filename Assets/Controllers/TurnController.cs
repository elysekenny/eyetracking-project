using UnityEngine;
using Eyeware.BeamEyeTracker.Unity;
using TMPro;
using UnityEngine.UI;
using Unity.VisualScripting;
using System;
using System.Diagnostics.Tracing;
using UnityEngine.InputSystem;
using System.Collections;

/*

    Player health should be passed between combat scenes

 */

public class TurnController :  BeamEyeTrackerMonoBehaviour
{
    private GameObject EnemyLoaded;
    private LoadEnemy EnemyLoadScript;


    public GameObject CombatUI;
    public GameObject BattleBox;
    private GameObject TimerBar;
    private GameObject PlayerHealthBar;
    private GameObject PlayerHealthBarTween;
    private GameObject EnemyHealthBar;
    private GameObject EnemyHealthBarTween;
    private NewEnemy CurrentEnemyData;

    // Active player data
    private float DefMod                 = 1.0f; // multiply the enemy's attack by this value
    private float AtkMod                 = 1.0f; // multiply by the players attack (when selecting special this will be 2.0f)
    private float BaseAttack             = 10.0f;
    private float AttackVariance         = 2.0f; // 8-12 dmg
    private bool WeakpointHighlighted    = false;

    // TIMER STUFF
    private float TimeRemaining          = 0.0f; // this will be specifc to the enemy

    //INPUTS
    InputAction PlayerAttack;
    InputAction PlayerDefend;
    InputAction PlayerSpecial;

    //Enumerators 😮😮😮
    public enum COMBAT_STATES    {NONE, PLAYER_TURN, ENEMY_TURN, PROCESS_RESULTS, START, END }
    public enum PLAYER_ACTIONS   {NONE, ATTACK, DEFEND, SPECIAL}
    public enum TURN_OWNERS      {NONE, PLAYER, ENEMY}

    private COMBAT_STATES GameState          = COMBAT_STATES.NONE;
    private PLAYER_ACTIONS PlayerAction      = PLAYER_ACTIONS.NONE;
    private TURN_OWNERS CurrentInitiator     = TURN_OWNERS.NONE;

    // Coroutine stuff 🥀🥀🥀 
    IEnumerator TimeDelay(int Period){yield return new WaitForSeconds(Period);} 

    private void SetupInput()
    {
        PlayerAttack     =  InputSystem.actions.FindAction("Combat.Attack");
        PlayerDefend     =  InputSystem.actions.FindAction("Combat.Defend");
        PlayerSpecial    =  InputSystem.actions.FindAction("Combat.Special");
    }

    public void Start()
    {
        // Get the enemy loaded
        EnemyLoaded          = GameObject.Find("EnemyData");
        EnemyLoadScript      = EnemyLoaded.GetComponent<LoadEnemy>();
        CurrentEnemyData     = EnemyLoadScript.EnemyToLoad;

        Debug.Log("Starting combat with " + CurrentEnemyData.DisplayName);

        SetupUIAttributes(); // set all the references to the ui attributes requried
        SetupInput();

        InitialiseCombatUI();

        // INTRO CUTSCENE
        GameState = COMBAT_STATES.START;
        PlayerTurnStart();

    }

    private void Update()
    {
        // Round timer visual thread
        // Health bar decrement animation

        if(GameState == COMBAT_STATES.PLAYER_TURN)
        {
            // only read the player input when in their turn
            if(PlayerAttack.WasPressedThisFrame())   {PlayerTryAttack();}
            if(PlayerDefend.WasPressedThisFrame())   {PlayerTryDefend();}
            if(PlayerSpecial.WasPressedThisFrame())  {PlayerTrySpecial();}

            TimeRemaining -= 0.01f;
            TimerBar.transform.localScale = new Vector3(TimerBar.transform.localScale.x - 0.01f, TimerBar.transform.localScale.y, TimerBar.transform.localScale.z);

            if(TimeRemaining <= 0f)
            {
                PlayerAction = PLAYER_ACTIONS.NONE;
                EndTurn();
            }
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

    private void PlayerTurnStart()
    {
        Debug.Log("-- PLAYER TURN START --");

        // reset the modifiers to their default x1
        DefMod = 1.0f;
        AtkMod = 1.0f;

        //UI elements
        BattleBox.transform.Find("Subtitle").gameObject.GetComponent<TextMeshProUGUI>().text         = "Your turn...";  
        BattleBox.transform.Find("SubtitleShadow").gameObject.GetComponent<TextMeshProUGUI>().text   = "Your turn...";  
        BattleBox.transform.Find("PlayerButtons").gameObject.SetActive(true);
        BattleBox.transform.Find("TurnResults").gameObject.SetActive(false);
        BattleBox.transform.Find("TurnResults").gameObject.transform.Find("TextOutput").GetComponent<TextMeshProUGUI>().text = "";


        // start the round timer
        TimeRemaining    = CurrentEnemyData.TurnDuration;
        GameState        = COMBAT_STATES.PLAYER_TURN; //starts the timer and lets input be read in the update loop
        CurrentInitiator = TURN_OWNERS.PLAYER;
    }

    private void PlayerTryAttack()
    {
        Debug.Log("Player use ATTACK action");

        // This action is always available to the player i just put try in the func nmae for love of the game
        PlayerAction = PLAYER_ACTIONS.ATTACK;
        EndTurn();
    }

    private void PlayerTryDefend()
    {
        Debug.Log("Player use DEFEND action");

        // This action is always available to the player i just put try in the func nmae for love of the game
        PlayerAction = PLAYER_ACTIONS.DEFEND;
        EndTurn();
    }

    private void PlayerTrySpecial()
    {
        Debug.Log("Player try SPECIAL action");

        // CHECK IF THE PLAYER HAS A WEAKPOINT HIGHLIGHTED CORRECTLY
        if (WeakpointHighlighted)
        {
            PlayerAction = PLAYER_ACTIONS.SPECIAL;
            EndTurn();
        }
    }

    private void EnemyTakeTurn()
    {
        Debug.Log("-- ENEMY TURN START --");
        GameState = COMBAT_STATES.ENEMY_TURN;
        CurrentInitiator = TURN_OWNERS.ENEMY;

        //UI elements
        BattleBox.transform.Find("Subtitle").gameObject.GetComponent<TextMeshProUGUI>().text         = "Enemy turn...";  
        BattleBox.transform.Find("SubtitleShadow").gameObject.GetComponent<TextMeshProUGUI>().text   = "Enemy turn...";  
        BattleBox.transform.Find("TurnResults").gameObject.SetActive(false);

        // Turn is over
        StartCoroutine(TimeDelay(2));
        EndTurn();
    }

    private void EndTurn()
    {
        // this handles when the turn timer runs out. will update the dialogue box as follows and have a second grace period before the next round starts
        Debug.Log(">> END TURN STATE <<");
        GameState = COMBAT_STATES.PROCESS_RESULTS;

        BattleBox.transform.Find("PlayerButtons").gameObject.SetActive(false);
        BattleBox.transform.Find("TurnResults").gameObject.SetActive(true);

        // PROCESS THE TURN RESULTS
        
        switch(CurrentInitiator)
        {
            case TURN_OWNERS.PLAYER:
                // the turn to process is what the player did
                switch (PlayerAction)
                {
                    case PLAYER_ACTIONS.NONE:
                        break;
                    
                    case PLAYER_ACTIONS.ATTACK:
                        break;

                    case PLAYER_ACTIONS.DEFEND:
                        break;

                    case PLAYER_ACTIONS.SPECIAL:
                        break;
                }

                //SET THE DIALOGUE BOX TO THE CORRECT TEXT
                // ADJUST OPPONENT HEALTH BAR 

                StartCoroutine(TimeDelay(1));
                EnemyTakeTurn();

                break;

            case TURN_OWNERS.ENEMY:
                //PROCESS THE END RESULTS FOR THE ENEMY
                //Set the text to what it needs to be

                StartCoroutine(TimeDelay(3));
                PlayerTurnStart();
                break;
        }
    }
}
