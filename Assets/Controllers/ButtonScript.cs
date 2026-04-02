using System;
using UnityEngine;
using UnityEngine.InputSystem;


// this is attached to each button and sends a signal to the ui manager what option to do

public class ButtonScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("All data")]
    public GameObject UiManager;
    public UiController.ButtonFunc Function;
    public UiController.ScreenTypes Screen;

    [Header("Start Screen")]

    public GameObject BACKGROUND;
    public Sprite BG_HOVER_START;
    public Sprite BG_HOVER_END;

    public GameObject BUTTON;
    public Sprite BUTTON_HOVER_START;
    public Sprite BUTTON_HOVER_END;

    public GameObject BUTTON_PROMPT;
    public GameObject FLOWER_VISUAL;


    [Header("End Screen")]
    public GameObject BACKGROUND_GAMEOVER;
    public Color BG_HOVERSTART_COLOUR;
    public Color BG_HOVEREND_COLOUR;
    public GameObject HIGHLIGHT;
    public GameObject BUTTON_PROMPT_GAMEOVER;
    public GameObject FLOWER_VISUAL_GAMEOVER;


    private UiController _CONTROLLER;
    private bool CanSelect = false;
    InputAction Select;

    // Update is called once per frame
    void Start()
    {
        Select       =  InputSystem.actions.FindAction("Interact");
        _CONTROLLER  = UiManager.GetComponent<UiController>();
    }

    void Update()
    {
        // check for button press when the option is highlighted
        if(Select.WasPressedThisFrame() && CanSelect) { _CONTROLLER.OnButtonPressed(Function);}
    }

    void OnTriggerEnter2D(Collider2D Collision)
    {
        // eye tracking! haha eye tracking in the main menu why not lets keep this consistent
        CanSelect = true;

        switch(Screen)
        {
            case UiController.ScreenTypes.START:
                HoverStart_START();
                break;

            case UiController.ScreenTypes.END:
                HoverStart_END();
                break;
        }
    }

    void OnTriggerExit2D(Collider2D Collision)
    {
        CanSelect = false;

        switch(Screen)
        {
            case UiController.ScreenTypes.START:
                HoverEnd_START();
                break;

            case UiController.ScreenTypes.END:
                HoverEnd_END();
                break;
        }
    }

    private void HoverStart_START()
    {
        BACKGROUND.GetComponent<SpriteRenderer>().sprite     = BG_HOVER_START;
        BUTTON.GetComponent<SpriteRenderer>().sprite         = BUTTON_HOVER_START;
        BUTTON_PROMPT.SetActive(true);
        FLOWER_VISUAL.SetActive(true);
    }

    private void HoverEnd_START()
    {
        BACKGROUND.GetComponent<SpriteRenderer>().sprite     = BG_HOVER_END;
        BUTTON.GetComponent<SpriteRenderer>().sprite         = BUTTON_HOVER_END;
        BUTTON_PROMPT.SetActive(false);
        FLOWER_VISUAL.SetActive(false);
    }

    private void HoverStart_END()
    {
        BACKGROUND_GAMEOVER.GetComponent<SpriteRenderer>().color = BG_HOVERSTART_COLOUR;
        BUTTON_PROMPT_GAMEOVER.SetActive(true);
        FLOWER_VISUAL_GAMEOVER.SetActive(true);
        HIGHLIGHT.SetActive(true);

    }

    private void HoverEnd_END()
    {
        BACKGROUND_GAMEOVER.GetComponent<SpriteRenderer>().color = BG_HOVEREND_COLOUR;
        BUTTON_PROMPT_GAMEOVER.SetActive(false);
        HIGHLIGHT.SetActive(false);
        FLOWER_VISUAL_GAMEOVER.SetActive(false);
    }
}