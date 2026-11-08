using UnityEngine;
using TMPro;
using System.Collections;
using System.Text;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// Put this on an object that is ALWAYS active (e.g. the Canvas or a manager) - NOT on the result panel itself,
// because the panel starts hidden and a hidden object can't run the fade-in.
public class ResultScreen : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject panel;          // the result panel (starts hidden)
    [SerializeField] private CanvasGroup panelGroup;    // optional: CanvasGroup on that panel, used for the fade-in
    [SerializeField] private TMP_Text titleText;        // big title, e.g. "The Realist"
    [SerializeField] private TMP_Text bodyText;         // explanation + speed + notebook lines
    [SerializeField] private Button restartButton;      // the Restart button inside the result panel
    [SerializeField] private float fadeTime = 1.5f;

    [Header("Restart")]
    [Tooltip("Pressing this key also restarts the level.")]
    [SerializeField] private KeyCode restartKey = KeyCode.R;

    [Header("Speed thresholds (seconds)")]
    [SerializeField] private float decisiveSeconds = 5f;   // at or under this = "decisive thinker"
    [SerializeField] private float carefulSeconds = 30f;   // at or over this = "careful thinker"

    [Header("Wording - edit freely")]
    [SerializeField] private string realistTitle = "The Realist";
    [TextArea(2, 4)]
    [SerializeField]
    private string realistText =
        "You saved the one who could help thousands. You look at the big picture, even when the cost is right in front of you.";

    [SerializeField] private string protectorTitle = "The Protector";
    [TextArea(2, 4)]
    [SerializeField]
    private string protectorText =
        "You saved as many lives as you could. Every person counts the same to you, whoever they are.";

    [SerializeField] private string bystanderTitle = "The Bystander";
    [TextArea(2, 4)]
    [SerializeField]
    private string bystanderText =
        "You didn't choose in time, so the train chose for you. Not deciding is still a decision.";

    [Header("Pulled the lever without looking at anyone")]
    [TextArea(1, 3)]
    [SerializeField] private string noLookText = "You didn't even look at them... you monster.";

    [Header("Speed lines")]
    [TextArea(1, 3)]
    [SerializeField] private string decisiveLine = "You decided in {0} seconds. You are a decisive thinker.";
    [TextArea(1, 3)]
    [SerializeField] private string carefulLine = "You took {0} seconds to decide. You weigh every option carefully.";
    [TextArea(1, 3)]
    [SerializeField] private string normalLine = "You decided in {0} seconds.";

    private bool resultShown;   // the result screen is up
    private bool canRestart;    // the fade has finished, so restarting is allowed
    private bool restarting;    // guard so the level only reloads once

    private void Awake()
    {
        if (panel == gameObject)
            Debug.LogError("ResultScreen: this script is on the result panel itself. Move it to an always-active object (e.g. the Canvas).", this);

        // The button still works through Unity's normal UI events if they are set up correctly...
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartLevel);
        else
            Debug.LogWarning("ResultScreen: Restart Button is not assigned.", this);

        if (panel != null)
            panel.SetActive(false);
    }

    private void Update()
    {
        if (!canRestart)
            return;

        // ...but these two do not depend on the EventSystem at all, so they always work.
        if (Input.GetKeyDown(restartKey))
            RestartLevel();
        else if (Input.GetMouseButtonDown(0) && ClickedRestartButton())
            RestartLevel();
    }

    // Keeps the mouse free while the result is up, even if the player controller tries to lock it again
    private void LateUpdate()
    {
        if (!resultShown)
            return;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // Checks the mouse position against the button's rectangle directly (no EventSystem / raycasting needed)
    private bool ClickedRestartButton()
    {
        if (restartButton == null || !restartButton.gameObject.activeInHierarchy)
            return false;

        RectTransform rect = restartButton.transform as RectTransform;
        Canvas canvas = restartButton.GetComponentInParent<Canvas>();
        Camera cam = (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay) ? canvas.worldCamera : null;

        return RectTransformUtility.RectangleContainsScreenPoint(rect, Input.mousePosition, cam);
    }

    public void RestartLevel()
    {
        if (restarting)
            return;
        restarting = true;

        Debug.Log("ResultScreen: Restart clicked");

        int index = SceneManager.GetActiveScene().buildIndex;

        if (index >= 0)
        {
            SceneManager.LoadScene(index);
            return;
        }

#if UNITY_EDITOR
        // Scene isn't in Build Settings: still restart while testing in the editor
        UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode(
            SceneManager.GetActiveScene().path,
            new LoadSceneParameters(LoadSceneMode.Single));
#else
        restarting = false;
        Debug.LogError("ResultScreen: this scene isn't in the Build Settings. Add it under File > Build Profiles > Scene List.", this);
#endif
    }

    // savedTheOne = the train hit the group and spared the single person
    public void Show(bool timedOut, bool savedTheOne, float decisionTime, int lookedAt, int totalPeople)
    {
        string title;
        var body = new StringBuilder();
        bool noLook = !timedOut && totalPeople > 0 && lookedAt <= 0;

        if (noLook)
        {
            // Pulled the lever without looking at anyone: show only this line
            title = "";
            body.Append(noLookText);
        }
        else if (timedOut)
        {
            title = bystanderTitle;
            body.Append(bystanderText);
        }
        else
        {
            title = savedTheOne ? realistTitle : protectorTitle;
            body.Append(savedTheOne ? realistText : protectorText);

            string time = decisionTime.ToString("0.0");
            string speedLine = decisionTime <= decisiveSeconds ? decisiveLine
                             : decisionTime >= carefulSeconds ? carefulLine
                             : normalLine;
            body.Append("\n\n").Append(string.Format(speedLine, time));
        }

        // How much did the player find out before deciding?
        if (totalPeople > 0 && !noLook)
        {
            body.Append("\n\n");
            if (lookedAt <= 0)
                body.Append("You didn't look at anyone before deciding.");
            else if (lookedAt >= totalPeople)
                body.Append("You studied everyone before deciding.");
            else
                body.Append("You looked at " + lookedAt + " of " + totalPeople + " people before deciding.");
        }

        if (titleText != null) titleText.text = title;
        if (bodyText != null) bodyText.text = body.ToString();

        // make sure the mouse is usable on the result screen
        resultShown = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        StopAllCoroutines();
        StartCoroutine(FadeIn());
    }

    private IEnumerator FadeIn()
    {
        canRestart = false;

        if (panel != null)
            panel.SetActive(true);

        if (panelGroup == null)
        {
            canRestart = true;
            yield break;
        }

        panelGroup.alpha = 0f;
        panelGroup.interactable = false;   // the UI button can't be clicked until the fade has finished
        float t = 0f;
        while (t < fadeTime)
        {
            t += Time.unscaledDeltaTime;
            panelGroup.alpha = Mathf.Clamp01(t / fadeTime);
            yield return null;
        }
        panelGroup.alpha = 1f;
        panelGroup.interactable = true;
        canRestart = true;
    }
}