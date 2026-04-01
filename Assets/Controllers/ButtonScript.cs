using UnityEngine;
using UnityEngine.InputSystem;


// this is attached to each button and sends a signal to the ui manager what option to do

public class ButtonScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public UiController.ButtonFunc Function;
    public GameObject UiManager;
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

        // visual
    }

    void OnTriggerExit2D(Collider2D Collision)
    {
        CanSelect = false;

        //visual
    }
}
