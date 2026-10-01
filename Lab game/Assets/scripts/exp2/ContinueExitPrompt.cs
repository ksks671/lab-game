using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ContinueExitPrompt : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ShockDialController dial;
    [SerializeField] private LevelLoader loader;
    [SerializeField] private GameObject promptPanel;
    [SerializeField] private TMP_Text promptText;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button exitButton;

    [Header("Continue / Exit prompts (dial value, 0 to 1)")]
    [SerializeField] private float[] thresholds = { 0.25f, 0.5f, 0.75f };
    [SerializeField]
    private string[] promptLines =
    {
        "Please continue.",
        "The experiment requires that you continue.",
        "It is absolutely essential that you continue."
    };

    [Header("At max")]
    [SerializeField] private float maxValue = 1f;
    [SerializeField] private float maxStayTime = 10f;
    [SerializeField] private string maxLine = "You have no other choice, you must go on.";

    [Header("Going backwards")]
    [SerializeField] private TMP_Text warningText;   // separate text, NOT inside the prompt panel
    [SerializeField]
    private string[] warningLines =
    {
        "Don't go backwards.",
        "The experiment requires that you go forwards.",
        "Going backwards is not an option.",
        "Turn the dial up. Now."
    };
    [SerializeField] private float backwardsTrigger = 0.03f; // how far back counts as "going backwards"
    [SerializeField] private float warningShowTime = 3f;
    [SerializeField] private float warningCooldown = 2f;

    private bool seated, promptOpen, maxReached, exiting;
    private int nextThreshold, warningIndex;
    private float lastValue, backwardsAmount, warningHideTime, nextWarningTime;

    private void Start()
    {
        promptPanel.SetActive(false);
        if (warningText != null) warningText.gameObject.SetActive(false);

        continueButton.onClick.AddListener(OnContinue);
        exitButton.onClick.AddListener(OnExit);
    }

    private void Update()
    {
        // Wait until the player is in the chair (the dial becomes active)
        if (!seated)
        {
            if (!dial.IsActive) return;

            seated = true;
            lastValue = dial.Value;
            FreeCursor();
            Debug.Log("Prompt: player is in the chair, cursor freed.");
        }

        if (warningText != null && warningText.gameObject.activeSelf && Time.time >= warningHideTime)
            warningText.gameObject.SetActive(false);

        if (promptOpen || maxReached || exiting) return;

        // Max comes first
        if (dial.Value >= maxValue - 0.001f)
        {
            StartCoroutine(MaxRoutine());
            return;
        }

        CheckBackwards();

        if (nextThreshold < thresholds.Length
            && thresholds[nextThreshold] < maxValue
            && dial.Value >= thresholds[nextThreshold])
        {
            Debug.Log($"Prompt: dial reached {dial.Value:0.00}, showing prompt {nextThreshold + 1}.");
            ShowPrompt();
        }
    }

    // ---------- Going backwards ----------

    private void CheckBackwards()
    {
        float delta = dial.Value - lastValue;
        lastValue = dial.Value;

        if (delta > 0.0001f) { backwardsAmount = 0f; return; }
        if (delta < -0.0001f) backwardsAmount += -delta;

        if (backwardsAmount >= backwardsTrigger && Time.time >= nextWarningTime)
            ShowWarning();
    }

    private void ShowWarning()
    {
        backwardsAmount = 0f;
        if (warningText == null || warningLines.Length == 0) return;

        int i = Mathf.Min(warningIndex, warningLines.Length - 1);
        warningText.text = warningLines[i];
        warningText.gameObject.SetActive(true);
        warningIndex++;

        warningHideTime = Time.time + warningShowTime;
        nextWarningTime = warningHideTime + warningCooldown;
        Debug.Log($"Prompt: backwards warning {warningIndex}: {warningLines[i]}");
    }

    private void HideWarning()
    {
        if (warningText != null) warningText.gameObject.SetActive(false);
    }

    // ---------- Continue / Exit prompts ----------

    private void ShowPrompt()
    {
        promptOpen = true;
        dial.SetActive(false);   // dial can't move while the prompt is up
        HideWarning();

        if (promptText != null && nextThreshold < promptLines.Length)
            promptText.text = promptLines[nextThreshold];

        continueButton.gameObject.SetActive(true);
        exitButton.gameObject.SetActive(true);
        promptPanel.SetActive(true);
        FreeCursor();
    }

    private void OnContinue()
    {
        Debug.Log("Prompt: CONTINUE pressed.");
        promptPanel.SetActive(false);
        promptOpen = false;

        while (nextThreshold < thresholds.Length && dial.Value >= thresholds[nextThreshold])
            nextThreshold++;

        dial.SetActive(true);
    }

    // ---------- Max: stay, then Exit appears ----------

    private IEnumerator MaxRoutine()
    {
        maxReached = true;
        dial.SetActive(false);   // locked at max
        HideWarning();
        Debug.Log($"Prompt: dial at max, holding the player for {maxStayTime}s.");

        if (promptText != null) promptText.text = maxLine;
        continueButton.gameObject.SetActive(false);
        exitButton.gameObject.SetActive(false);
        promptPanel.SetActive(true);
        FreeCursor();

        yield return new WaitForSeconds(maxStayTime);

        Debug.Log("Prompt: Exit button now available.");
        exitButton.gameObject.SetActive(true);
    }

    private void OnExit()
    {
        if (exiting) return;
        exiting = true;

        Debug.Log($"Prompt: EXIT pressed at dial value {dial.Value:0.00}.");
        promptPanel.SetActive(false);
        loader.LoadNextLevel();
    }

    private void FreeCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}