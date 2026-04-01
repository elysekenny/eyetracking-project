using UnityEngine;

// this is attached to each button and sends a signal to the ui manager what option to do

public class ButtonScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public UiController.ButtonFunc Function;
    // Update is called once per frame
    void Start()
    {
        
    }
    void Update()
    {
        // check for button press when the option is highlighted
    }

    void OnTriggerEnter2D(Collider2D Collision)
    {
        // eye tracking! haha eye tracking in the main menu why not lets keep this consistent
        Debug.Log(this.name);
    }

    void OnTriggerExit2D(Collider2D Collision)
    {
        Debug.Log("Hover end");
    }
}
