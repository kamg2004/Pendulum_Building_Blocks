using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    // Default settings
    public static bool isRealMode = true;
    public static float chosenLength = 0.5f; // Default length for Virtual Mode

    // --- SCENE 1: MODE SELECTION ---
    public void SelectMode(bool real)
    {
        isRealMode = real;

        if (isRealMode)
        {
            // REAL MODE: Go to Length Menu first
            Debug.Log("Real Mode Selected -> Going to Length Menu");
            SceneManager.LoadScene("Menu");
        }
        else
        {
            // VIRTUAL MODE: Go straight to Simulation
            // (You can set a default length here if you want, e.g., 0.5 meters)
            chosenLength = 0.5f; 
            Debug.Log("Virtual Mode Selected -> Going straight to Simulation");
            SceneManager.LoadScene("Main");
        }
    }

    // --- SCENE 2: LENGTH SELECTION (Only for Real Mode) ---
    public void SelectLength(float length)
    {
        chosenLength = length;
        Debug.Log("Length Selected: " + chosenLength);
        SceneManager.LoadScene("Main");
    }
}