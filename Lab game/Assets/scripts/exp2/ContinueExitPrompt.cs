using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ContinueExitPrompt : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ShockDialController dial;
    [SerializeField] private LevelLoader loader;
    [SerializeField] private GameObject promptPanel;   // panel holding the text and both buttons
    [SerializeField] private TMP_Text promptText;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button exitButton;

    [Header("When it appears (dial value, 0 to 1)")]
    [SerializeField] private float[] thresholds = { 0.25f, 0.5f, 0.75f, 1f };

    [Header("Line shown at each threshold")]
    [SerializeField]
    private string[] promptLines =
    {
        "Please continue.",
        "The experiment requires that you continue.",
        "It is absolutely essential that you continue.",
        "You have no other choice, you must go on."
    };

    private int nextThreshold;
    private bool promptOpen;
    private bool exiting;

    private void Start()
    {
        promptPanel.SetActive(false);
        continueButton.onClick.AddListener(OnContinue);
        exitButton.onClick.AddListener(OnExit);
        FreeCursor();
    }

    private void Update()
    {
        if (promptOpen || exiting) return;
        if (nextThreshold >= thresholds.Length) return;

        if (dial.Value >= thresholds[nextThreshold])
        {
            Debug.Log($"Prompt: dial reached {dial.Value:0.00}, showing prompt {nextThreshold + 1}.");
            ShowPrompt();
        }
    }

    private void ShowPrompt()
    {
        promptOpen = true;
        dial.SetActive(false);        // dial can't move while the prompt is up

        if (promptText != null && nextThreshold < promptLines.Length)
            promptText.text = promptLines[nextThreshold];

        promptPanel.SetActive(true);
        FreeCursor();
    }

    private void OnContinue()
    {
        Debug.Log("Prompt: CONTINUE pressed.");
        promptPanel.SetActive(false);
        promptOpen = false;

        // skip any thresholds already passed so prompts never stack
        while (nextThreshold < thresholds.Length && dial.Value >= thresholds[nextThreshold])
            nextThreshold++;

        dial.SetActive(true);
    }

    private void OnExit()
    {
        if (exiting) return;
        exiting = true;

        Debug.Log($"Prompt: EXIT pressed at dial value {dial.Value:0.00}.");
        promptPanel.SetActive(false);
        loader.LoadNextLevel();       // fades to black, then loads the next scene
    }

    private void FreeCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}