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
    [SerializeField] private CanvasGroup panelGroup;    // CanvasGroup on that panel, used for the fade-in
    [SerializeField] private TMP_Text titleText;        // big title, e.g. "The Realist"
    [SerializeField] private TMP_Text bodyText;         // explanation + speed + notebook lines
    [SerializeField] private Button restartButton;      // a button inside the result panel
    [SerializeField] private float fadeTime = 1.5f;

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

    private void Awake()
    {
        if (panel == gameObject)
            Debug.LogError("ResultScreen: this script is on the result panel itself. Move it to an always-active object (e.g. the Canvas).", this);

        if (restartButton != null)
            restartButton.onClick.AddListener(RestartLevel);
        else
            Debug.LogWarning("ResultScreen: Restart Button is not assigned.", this);

        if (panel != null)
            panel.SetActive(false);
    }

    public void RestartLevel()
    {
        // reloads the current scene, so the timer, lever, train and notepad all start fresh
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
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
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        StopAllCoroutines();
        StartCoroutine(FadeIn());
    }

    private IEnumerator FadeIn()
    {
        if (panel != null)
            panel.SetActive(true);

        if (panelGroup == null)
            yield break;

        panelGroup.alpha = 0f;
        panelGroup.interactable = false;   // the button can't be clicked until the fade has finished
        float t = 0f;
        while (t < fadeTime)
        {
            t += Time.unscaledDeltaTime;
            panelGroup.alpha = Mathf.Clamp01(t / fadeTime);
            yield return null;
        }
        panelGroup.alpha = 1f;
        panelGroup.interactable = true;
    }
}