using UnityEngine;
using UnityEngine.UI;

public class MouseLockController : MonoBehaviour
{
    private bool buttonsOnScreen = false;

    void Update()
    {
        // Check if any buttons are visible on any canvas
        buttonsOnScreen = CheckIfAnyButtonsOnScreen();

        // If no buttons are visible and the app is focused, lock the mouse cursor to the game window
        if (!buttonsOnScreen && Application.isFocused)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;  // Hide the cursor
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;  // Show the cursor
        }
    }

    bool CheckIfAnyButtonsOnScreen()
    {
        // Find all Canvas objects in the scene
        Canvas[] canvases = FindObjectsOfType<Canvas>();

        // Loop through each canvas and check for buttons
        foreach (Canvas canvas in canvases)
        {
            // Find all Button components in the canvas
            Button[] buttons = canvas.GetComponentsInChildren<Button>(true); // true to include inactive buttons

            // Check if any buttons are active in the hierarchy
            foreach (Button button in buttons)
            {
                if (button.gameObject.activeInHierarchy) // Button is visible and active
                {
                    return true;  // A button is visible on the screen
                }
            }
        }

        return false;  // No active buttons found
    }
}
