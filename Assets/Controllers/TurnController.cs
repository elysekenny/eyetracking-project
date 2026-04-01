using UnityEngine;
using Eyeware.BeamEyeTracker.Unity;
using TMPro;
using UnityEngine.UI;
using Unity.VisualScripting;
using System;
using System.Diagnostics.Tracing;
using UnityEngine.InputSystem;
using System.Collections;
using Unity.Mathematics;
using UnityEditor;

/*

    Player health should be passed between combat scenes

 */

public class TurnController :  BeamEyeTrackerMonoBehaviour
{
    private GameObject EnemyLoaded;
    private LoadEnemy EnemyLoadScript;


    public GameObject CombatUI;
    public GameObject BattleBox;
    public GameObject MaskRef;
    public GameObject WeakpointPrefab;
    public GameObject WeakpointSpawnZone;

    private GameObject TimerBar;
    private GameObject PlayerHealthBar;
    private GameObject PlayerHealthBarTween;
    private GameObject EnemyHealthBar;
    private GameObject EnemyHealthBarTween;
    private Image SpecialActionButton;
    private Image SpecialActionButtonShadow;

    private NewEnemy CurrentEnemyData;

    // Active player data 🤑🤑🤑
    private int PlayerMaxHealth          = 200;
    private float DefMod                 = 1.0f; // multiply the enemy's attack by this value
    private float AtkMod                 = 1.0f; // multiply by the players attack (when selecting special this will be 2.0f)
    private int BaseAttack               = 10;
    private int AttackVariance           = 2; // 8-12 dmg
    private bool WeakpointHighlighted    = false;

    // Active enemy data
    private float EnemyDefMod            = 1.0f;
    private float EnemyAtkMod            = 1.0f;

    // TIMER STUFF
    private float TimeRemaining          = 0.0f; // this will be specifc to the enemy

    //INPUTS 🤔🤔🤔
    InputAction PlayerAttack;
    InputAction PlayerDefend;
    InputAction PlayerSpecial;

    //Enumerators 😮😮😮
    public enum COMBAT_STATES    {NONE, PLAYER_TURN, ENEMY_TURN, PROCESS_RESULTS, START, END }
    public enum PLAYER_ACTIONS   {NONE, ATTACK, DEFEND, SPECIAL}
    public enum TURN_OWNERS      {NONE, PLAYER, ENEMY}
    public enum ENEMY_SPECIAL_TYPES {NONE, DAMAGE, DEFEND, HEAL}

    private COMBAT_STATES GameState          = COMBAT_STATES.NONE;
    private PLAYER_ACTIONS PlayerAction      = PLAYER_ACTIONS.NONE;
    private TURN_OWNERS CurrentInitiator     = TURN_OWNERS.NONE;

    // Coroutine stuff 🥀🥀🥀 
    IEnumerator TimeDelay(int Period){yield return new WaitForSeconds(Period);} 

    // Data tracking (health and other values that could change in combat) 😵‍💫😵‍💫😵‍💫
    private int PlayerHealth;
    private int EnemyHealth;

    private GameObject SpawnedWeakpoint;

    private GameObject HealthBarToTween;
    private float TargetNewSize;
    private float TargetNewPos;
    private bool AnimateHealthBar    = false;
    private int AnimDirection        = -1;

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

        // Set and init the starting health attributes
        // (player health bar needs to be initialised at the right amount as its passed in each combat)
        PlayerHealth     = PlayerPrefs.GetInt("PlayerHealth", PlayerMaxHealth); // write the player health at the end of combat
        EnemyHealth      = CurrentEnemyData.Health;

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

            if (WeakpointHighlighted)
            {
                //special button highlighted
                SpecialActionButtonShadow.color  = new Color(0.4f, 0.4f, 0.4f, 1);
                SpecialActionButton.color        = new Color(1, 1, 1, 1);
            }
            else
            {
                // grey out the special button
                SpecialActionButtonShadow.color  = new Color(0.4f, 0.4f, 0.4f, 0.4f);
                SpecialActionButton.color        = new Color(0, 0, 0, 0.5f);
            }

            TimeRemaining -= Time.deltaTime;
            // TimerBar.transform.localScale = new Vector3(TimerBar.transform.localScale.x - Time.deltaTime / CurrentEnemyData.TurnDuration, TimerBar.transform.localScale.y, TimerBar.transform.localScale.z);T
            // timer bar move between 0 and 880 in a turn on the x axis
            float TimerIncrement = CurrentEnemyData.TurnDuration / 880 * Time.deltaTime;
            TimerBar.transform.position = new Vector3(TimerBar.transform.position.x + TimerIncrement, TimerBar.transform.position.y, TimerBar.transform.position.z);

            if(TimeRemaining <= 0f)
            {
                PlayerAction = PLAYER_ACTIONS.NONE;
                EndTurn();
            }
        }

        // health bar visual tween
        // if(AnimateHealthBar){AnimateHealth();}
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
        SpriteRenderer EnemySprite            = CombatUI.gameObject.transform.Find("Enemy").gameObject.transform.Find("EnemySprite").GetComponent<SpriteRenderer>();
        SpriteRenderer EnemySprite_Shadow     = CombatUI.gameObject.transform.Find("Enemy").gameObject.transform.Find("EnemyShadow").GetComponent<SpriteRenderer>();
        SpriteRenderer EnemyMask              = CombatUI.gameObject.transform.Find("Enemy").gameObject.transform.Find("EnemyOverlay").GetComponent<SpriteRenderer>();
        
        EnemySprite.sprite                        = CurrentEnemyData.FullSprite;
        EnemySprite_Shadow.sprite                 = CurrentEnemyData.FullSprite;
        EnemyMask.sprite                          = CurrentEnemyData.FullSprite;
        EnemyMask.color                           = CurrentEnemyData.WeakpointMaskColour;

        EnemySprite.transform.localScale          = CurrentEnemyData.OverrideSpriteScale;
        EnemySprite_Shadow.transform.localScale   = CurrentEnemyData.OverrideSpriteScale;
        EnemyMask.transform.localScale            = CurrentEnemyData.OverrideSpriteScale;

        //Health bar values that differ from enemy to enemy
        EnemyHealthBar.GetComponent<SpriteRenderer>().color          = CurrentEnemyData.FillColour;
        EnemyHealthBarTween.GetComponent<SpriteRenderer>().color     = CurrentEnemyData.TweenColour;

        CombatUI.gameObject.transform.Find("EnemyHealth").gameObject.transform.Find("EnemyHealthBacking").gameObject.GetComponent<SpriteRenderer>().color        = CurrentEnemyData.BackingColour;
        CombatUI.gameObject.transform.Find("EnemyHealth").gameObject.transform.Find("HealthBarMask").gameObject.GetComponent<SpriteMask>().sprite                = CurrentEnemyData.HealthBarMask;
        CombatUI.gameObject.transform.Find("EnemyHealth").gameObject.transform.Find("EnemyHealthBarOverlay").gameObject.GetComponent<SpriteRenderer>().sprite    = CurrentEnemyData.HealthBarOutline;
        CombatUI.gameObject.transform.Find("EnemyHealth").gameObject.transform.Find("TopSprite").gameObject.GetComponent<SpriteRenderer>().sprite                = CurrentEnemyData.HealthBarOutline;
        CombatUI.gameObject.transform.Find("EnemyHealth").gameObject.transform.Find("TopSprite").gameObject.GetComponent<SpriteRenderer>().color                 = CurrentEnemyData.SecondaryTint;
        CombatUI.gameObject.transform.Find("EnemyHealth").gameObject.transform.Find("UnderSprite").gameObject.GetComponent<SpriteRenderer>().sprite              = CurrentEnemyData.HealthBarMask;

        MaskRef.GetComponent<SpriteMask>().sprite    = CurrentEnemyData.HudIcon;
        MaskRef.transform.localScale                 = CurrentEnemyData.SpriteMaskDimensions;
        
        Color colour     = CurrentEnemyData.SpriteColour;
        colour.a         = 0.3f;

        MaskRef.gameObject.transform.Find("OVERLAY").GetComponent<SpriteRenderer>().sprite   = CurrentEnemyData.HudIcon;
        MaskRef.gameObject.transform.Find("OVERLAY").GetComponent<SpriteRenderer>().color    = colour;
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

        //Special button (eye tracking weakpoints)
        SpecialActionButton          = BattleBox.gameObject.transform.Find("PlayerButtons").gameObject.transform.Find("Special").gameObject.GetComponent<Image>();
        SpecialActionButtonShadow    = BattleBox.gameObject.transform.Find("PlayerButtons").gameObject.transform.Find("SpShadow").gameObject.GetComponent<Image>();
    }

    // can be used for player or enemy just needs to pass in the correct ui elements based on use case
    private void UpdateHealthBar(GameObject HealthBar, GameObject HealthAnim, float HealthRemainingPercent)
    {
        // health bar height = 1000 @ full hp and then decrement the ypos by the decrease amount / 2
        // tween the animation down to the target size of the health bar
        float DamageTaken    = 1 - (HealthRemainingPercent / 100);
        float NewYPos        = 1100 * DamageTaken; // 50% health means multiply by 0.5

        if(HealthRemainingPercent / 100 > HealthBar.transform.localScale.y){AnimDirection = 1;}
        else{AnimDirection = -1;}

        HealthBar.transform.position = new Vector3(HealthBar.transform.position.x, -NewYPos, HealthBar.transform.position.z);
        // HealthBar.transform.localScale   = new Vector3( HealthBar.transform.localScale.x, HealthRemainingPercent / 100, 1);

        // HealthBarToTween     = HealthAnim;
        // TargetNewSize        = HealthRemainingPercent / 100;

        // AnimateHealthBar     = true;
        // Debug.Log($"New health percent = {HealthRemainingPercent}%");
    }

    private void AnimateHealth()
    {
        if(HealthBarToTween.transform.localScale.y <= TargetNewSize){AnimateHealthBar = false;} // should be true for healing health too
        // apply to the healthbarToTween
        HealthBarToTween.transform.localScale    = new Vector3(1,  HealthBarToTween.transform.localScale.y + (AnimDirection * Time.deltaTime), 1);
        HealthBarToTween.transform.position      = new Vector3(HealthBarToTween.transform.position.x, HealthBarToTween.transform.position.y + (AnimDirection * Time.deltaTime), HealthBarToTween.transform.position.z);
    }

    // WEAKPOINT STUFF
    public void SetWeakpointVisibility(bool _arg){WeakpointHighlighted = _arg;} // set from the collisions in the mask
    private void SpawnWeakpoint()
    {
        // get a random point in the bounds of the weakpoint bounds, check if it is overlapping with the enemy sprite too and create a new weakpoint prefab instance
        Bounds SpawnRegion = WeakpointSpawnZone.GetComponent<BoxCollider2D>().bounds;
        Vector3 spawnPosition = new Vector3(
            UnityEngine.Random.Range(SpawnRegion.min.x, SpawnRegion.max.x),
            UnityEngine.Random.Range(SpawnRegion.min.y, SpawnRegion.max.y),
            0);
        
        SpawnedWeakpoint = Instantiate(WeakpointPrefab, spawnPosition, new Quaternion());
    }


    private void PlayerTurnStart()
    {
        // Debug.Log("-- PLAYER TURN START --");

        // reset the modifiers to their default x1
        DefMod = 1.0f;
        AtkMod = 1.0f;

        //UI elements
        BattleBox.transform.Find("Subtitle").gameObject.GetComponent<TextMeshProUGUI>().text         = "Your turn...";  
        BattleBox.transform.Find("SubtitleShadow").gameObject.GetComponent<TextMeshProUGUI>().text   = "Your turn...";  
        BattleBox.transform.Find("PlayerButtons").gameObject.SetActive(true);
        BattleBox.transform.Find("TurnResults").gameObject.SetActive(false);
        BattleBox.transform.Find("TurnResults").gameObject.transform.Find("TextOutput").GetComponent<TextMeshProUGUI>().text = "";

        SpawnWeakpoint();

        // start the round timer
        GameState        = COMBAT_STATES.PLAYER_TURN; //starts the timer and lets input be read in the update loop
        CurrentInitiator = TURN_OWNERS.PLAYER;
    }

    private void PlayerTryAttack()
    {
        // Debug.Log("Player use ATTACK action");

        // This action is always available to the player i just put try in the func nmae for love of the game
        PlayerAction = PLAYER_ACTIONS.ATTACK;
        EndTurn();
    }

    private void PlayerTryDefend()
    {
        // Debug.Log("Player use DEFEND action");

        // This action is always available to the player i just put try in the func nmae for love of the game
        PlayerAction = PLAYER_ACTIONS.DEFEND;
        EndTurn();
    }

    private void PlayerTrySpecial()
    {
        // Debug.Log("Player try SPECIAL action");

        // CHECK IF THE PLAYER HAS A WEAKPOINT HIGHLIGHTED CORRECTLY
        if (WeakpointHighlighted)
        {
            PlayerAction = PLAYER_ACTIONS.SPECIAL;
            EndTurn();
        }
    }

    private void PlayerAttacksEnemy(bool IsWeakpoint)
    {
        // get a random integer betweem the base attack +- the variance then multiply by the attack modifier (base of x1 or x2 for weakpoint attack)
        System.Random rand   = new System.Random();
        int AttackValue      = rand.Next(BaseAttack - AttackVariance, BaseAttack + AttackVariance);
        AttackValue          = (int)math.round(AttackValue * AtkMod * EnemyDefMod);

        EnemyHealth -= AttackValue;
        math.clamp(EnemyHealth, 0, CurrentEnemyData.Health); //visually health cannot go below 0 
        float HealthPercent = (float)((float)EnemyHealth / (float)CurrentEnemyData.Health * 100); 
        UpdateHealthBar(EnemyHealthBar, EnemyHealthBarTween, HealthPercent);

        // Battle box dialogue
        string Dialogue = $"You hit {CurrentEnemyData.DisplayName} with your attack... \nIt deals {AttackValue} damage!";
        if(IsWeakpoint){Dialogue = $"You hit {CurrentEnemyData.DisplayName} in the weakpoint\n! IT DEALS {AttackValue} DAMAGE!!!";} //hype and aura
        BattleBox.transform.Find("TurnResults").gameObject.transform.Find("TextOutput").gameObject.GetOrAddComponent<TextMeshProUGUI>().text = Dialogue;
    }

    private void EnemyAttacksPlayer(int AttackBase, bool IsSpecial)
    {
        System.Random rand   = new System.Random();

        if(rand.Next(1,100) <= CurrentEnemyData.CriticalChance){EnemyAtkMod = 2.0f;}

        int AttackValue      = rand.Next(AttackBase - CurrentEnemyData.DamageVariance, AttackBase + CurrentEnemyData.DamageVariance);
        AttackValue          = (int)math.round(AttackValue * EnemyAtkMod * DefMod);

        PlayerHealth -= AttackValue;
        math.clamp(PlayerHealth, 0, PlayerMaxHealth); //visually health cannot go below 0 --> debug player max health is hard coded to be 100 but id like this to be an external variable elsewhere
        float HealthPercent = (float)((float)PlayerHealth / (float)PlayerMaxHealth * 100); 
        UpdateHealthBar(PlayerHealthBar, PlayerHealthBarTween, HealthPercent);

        // Battle box dialogue
        string AttackName = CurrentEnemyData.AttackName;
        if(IsSpecial){AttackName = CurrentEnemyData.SpecialName;}
        string Dialogue = $"{CurrentEnemyData.DisplayName} uses {AttackName}... \nIt deals {AttackValue} damage!";
        BattleBox.transform.Find("TurnResults").gameObject.transform.Find("TextOutput").gameObject.GetOrAddComponent<TextMeshProUGUI>().text = Dialogue;
    }

    private void EnemyTakeTurn()
    {
        // Debug.Log("-- ENEMY TURN START --");
        GameState            = COMBAT_STATES.ENEMY_TURN;
        CurrentInitiator     = TURN_OWNERS.ENEMY;

        // Reset the enemy values that get saved each turn (eg if it defends this has to be used in the player's turn 🥀)
        EnemyDefMod = 1.0f;
        EnemyAtkMod = 1.0f;

        //UI elements
        BattleBox.transform.Find("Subtitle").gameObject.GetComponent<TextMeshProUGUI>().text         = "Enemy turn...";  
        BattleBox.transform.Find("SubtitleShadow").gameObject.GetComponent<TextMeshProUGUI>().text   = "Enemy turn...";  
        BattleBox.transform.Find("TurnResults").gameObject.SetActive(false);

        // Enemy Turn Action determination
        // Use the % chance of doing a special and either do a special or just do an attack if its not a special attack
        System.Random rand = new System.Random();
        if(rand.Next(1, 100) <= CurrentEnemyData.SpecialChance)
        {
            switch(CurrentEnemyData.SpecialType)
            {
                case ENEMY_SPECIAL_TYPES.DAMAGE:
                    EnemyAttacksPlayer((int)CurrentEnemyData.SpecialValue, true);
                    break;

                case ENEMY_SPECIAL_TYPES.DEFEND:
                    EnemyDefMod = CurrentEnemyData.SpecialValue;

                    // Battle box nonsense
                    string Dialogue = $"{CurrentEnemyData.DisplayName} uses {CurrentEnemyData.SpecialName}... \nIt looks like it is fortifying for your next turn!";
                    BattleBox.transform.Find("TurnResults").gameObject.transform.Find("TextOutput").gameObject.GetOrAddComponent<TextMeshProUGUI>().text = Dialogue;
                    break;

                case ENEMY_SPECIAL_TYPES.HEAL:
                    EnemyHealth += (int)CurrentEnemyData.SpecialValue;
                    math.clamp(EnemyHealth, 0, CurrentEnemyData.Health); // they cannot have more health than they have health
                    float HealthPercent = (float)((float) EnemyHealth / (float) CurrentEnemyData.Health * 100);
                    UpdateHealthBar(EnemyHealthBar, EnemyHealthBarTween, HealthPercent); // readjust the health bar

                    // Battle box nonsense
                    string _Dialogue = $"{CurrentEnemyData.DisplayName} uses {CurrentEnemyData.SpecialName}... \nIt heals {CurrentEnemyData.SpecialValue} health back!";
                    BattleBox.transform.Find("TurnResults").gameObject.transform.Find("TextOutput").gameObject.GetOrAddComponent<TextMeshProUGUI>().text = _Dialogue;
                    break;
            }
        }

        else {EnemyAttacksPlayer(CurrentEnemyData.BaseDamage, false);}

        // Turn is over
        Invoke(nameof(EndTurn), 2.0f);
    }

    private void EndTurn()
    {
        // this handles when the turn timer runs out. will update the dialogue box as follows and have a second grace period before the next round starts
        // Debug.Log(">> END TURN STATE <<");
        GameState                        = COMBAT_STATES.PROCESS_RESULTS;
        TimeRemaining                    = CurrentEnemyData.TurnDuration; // RESET THE TIMER FOR THE NEXT PLAYER TURN
        // TimerBar.transform.localScale    = new Vector3(TimerFilledVal, TimerBar.transform.localScale.y, TimerBar.transform.localScale.z);
        TimerBar.transform.position      = new Vector3(0, TimerBar.transform.position.y, TimerBar.transform.position.z);

        //Remove weakpoint and reset at end of turn
        if(SpawnedWeakpoint)
        {
            Destroy(SpawnedWeakpoint);
            SpawnedWeakpoint = null;
        }
        WeakpointHighlighted = false;

        BattleBox.transform.Find("PlayerButtons").gameObject.SetActive(false);
        BattleBox.transform.Find("TurnResults").gameObject.SetActive(true);
        
        switch(CurrentInitiator)
        {
            case TURN_OWNERS.PLAYER:
                // the turn to process is what the player did
                switch (PlayerAction)
                {
                    case PLAYER_ACTIONS.NONE:
                        string dialogue = $"You wasted your time this turn...";
                        BattleBox.transform.Find("TurnResults").gameObject.transform.Find("TextOutput").gameObject.GetOrAddComponent<TextMeshProUGUI>().text = dialogue;
                        break;
                    
                    case PLAYER_ACTIONS.ATTACK:
                        PlayerAttacksEnemy(false);
                        break;

                    case PLAYER_ACTIONS.DEFEND:
                        DefMod = 0.5f;

                         // Battle box nonsense
                        string Dialogue = $"You prepare for an enemy attack...";
                        BattleBox.transform.Find("TurnResults").gameObject.transform.Find("TextOutput").gameObject.GetOrAddComponent<TextMeshProUGUI>().text = Dialogue;
                        break;

                    case PLAYER_ACTIONS.SPECIAL:
                        AtkMod = 2.0f;
                        PlayerAttacksEnemy(true); // note is weakpoint is already handled with AtkMod but this just adds an extra layer of flavour text
                        break;
                }

                Invoke(nameof(ResumeAsEnemy), 1.5f);
                break;

            case TURN_OWNERS.ENEMY:
                Invoke(nameof(ResumeAsPlayer), 1.5f);
                break;
        }
    }

    private void ResumeAsPlayer()
    {
        // check if the player is dead after attacked by the enemy
        if(PlayerHealth <= 0)
        {
            Debug.Log(">> THE PLAYER IS DEAD <<");

            // load the ui screen scene with the game state of LOSE passed in.
        }
        else
        {
            PlayerTurnStart();
        }
    }

    private void ResumeAsEnemy()
    {
        // check if the enemy is dead after the player's turn
        if(EnemyHealth <= 0)
        {
            Debug.Log(">> THE ENEMY IS DEAD <<");
            string SaveDataName  = CurrentEnemyData.EnemyID + "_REMAINING";
            int previousValue    = (int)Convert.ToInt64(PlayerPrefs.GetString(SaveDataName, CurrentEnemyData.TotalInWorld.ToString()));
            string newValue      = (previousValue - 1).ToString();

            PlayerPrefs.SetString(SaveDataName, newValue);
            PlayerPrefs.SetInt("PlayerHealth", PlayerHealth);

            // load the environment scene with in world reference to be destroyed so the player cannot fight it again
        }
        else
        {
            EnemyTakeTurn();
        }
    }
}