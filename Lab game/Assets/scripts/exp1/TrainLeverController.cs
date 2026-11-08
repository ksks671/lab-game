using UnityEngine;
using System.Collections;
using TMPro; // NEW

public class TrainLeverController : MonoBehaviour
{
    [Header("Animators")]
    [SerializeField] private Animator leverAnimator;
    [SerializeField] private Animator trainAnimator;

    [Header("Trigger Names")]
    [SerializeField] private string leverPullTrigger = "LeverPull";
    [SerializeField] private string trainRightTrigger = "TrainRight";
    [SerializeField] private string trainStraightTrigger = "TrainStraight";

    [Header("Settings")]
    [SerializeField] private float timeLimit = 60f;

    [SerializeField] private Transform leverObject;

    [Header("Timer UI")] // NEW
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color warningColor = Color.red;
    [SerializeField] private float warningThreshold = 10f; // turns red under this many seconds

    [Header("Result")] // NEW
    [SerializeField] private ResultScreen resultScreen;
    [Tooltip("Ticked: pulling the lever runs over the SINGLE person (and does nothing = the group of 4 is hit). Unticked: the other way round.")]
    [SerializeField] private bool leverHitsSingleTrack = true;
    [SerializeField] private float resultDelay = 1f; // pause after the train finishes before the result appears
    [Tooltip("Middle mouse button deliberately lets the train run without pulling the lever. Waiting until the timer ends counts as freezing instead.")]
    [SerializeField] private int letItRunMouseButton = 2; // 0 = left, 1 = right, 2 = middle

    [SerializeField] private string leverStateName = "change";

    private float timer;
    private bool sequenceFinished;
    private bool straightWasTimeout; // true = the player froze, false = they pressed the "let it run" key

    // captured the moment the player decides (or runs out of time)
    private float decisionTime;
    private int lookedAt;
    private int totalPeople;

    private void Start()
    {
        timer = timeLimit;

        if (leverAnimator == null)
            Debug.LogError("Lever Animator is not assigned!", this);

        if (trainAnimator == null)
            Debug.LogError("Train Animator is not assigned!", this);

        if (timerText == null)
            Debug.LogWarning("Timer Text is not assigned!", this);

        if (resultScreen == null)
            Debug.LogWarning("Result Screen is not assigned!", this);

        UpdateTimerUI(); // NEW
    }

    private void Update()
    {
        if (sequenceFinished)
            return;

        timer -= Time.deltaTime;
        UpdateTimerUI(); // NEW

        if (timer <= 0f)
        {
            StraightRoute(true); // ran out of time
            return;
        }

        if (Input.GetMouseButtonDown(letItRunMouseButton))
        {
            StraightRoute(false); // deliberate choice to do nothing
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("LEFT CLICK DETECTED");
            CheckLeverClick();
        }
    }

    // NEW
    private void UpdateTimerUI()
    {
        if (timerText == null)
            return;

        float displayTime = Mathf.Max(timer, 0f);
        int minutes = Mathf.FloorToInt(displayTime / 60f);
        int seconds = Mathf.CeilToInt(displayTime % 60f);

        // CeilToInt can give 60, so roll it over into the minutes
        if (seconds == 60)
        {
            minutes++;
            seconds = 0;
        }

        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        timerText.color = displayTime <= warningThreshold ? warningColor : normalColor;
    }

    private void CheckLeverClick()
    {
        if (Camera.main == null)
        {
            Debug.LogError("No Main Camera found!");
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Debug.Log("Raycast hit: " + hit.transform.name);

            if (hit.transform == leverObject ||
                hit.transform.IsChildOf(leverObject))
            {
                Debug.Log("LEVER CLICKED!");

                RightRoute();
            }
        }
        else
        {
            Debug.Log("Raycast hit nothing.");
        }
    }

    // NEW: record how long it took and how many people the player looked at
    private void CaptureDecision()
    {
        decisionTime = timeLimit - Mathf.Max(timer, 0f);

        PersonProfile[] people = FindObjectsByType<PersonProfile>(FindObjectsSortMode.None);
        totalPeople = people.Length;
        lookedAt = 0;
        foreach (PersonProfile p in people)
            if (p.discovered)
                lookedAt++;

        Debug.Log("Decision after " + decisionTime.ToString("0.0") + "s, looked at " + lookedAt + " of " + totalPeople);
    }

    private void RightRoute()
    {
        if (sequenceFinished)
            return;

        sequenceFinished = true;
        CaptureDecision(); // NEW

        if (leverAnimator == null || trainAnimator == null)
            return;

        leverAnimator.SetTrigger(leverPullTrigger);

        StartCoroutine(PlayRightTrain());
    }

    private void StraightRoute(bool timedOut)
    {
        if (sequenceFinished)
            return;

        straightWasTimeout = timedOut;
        sequenceFinished = true;
        CaptureDecision(); // NEW

        if (leverAnimator == null || trainAnimator == null)
            return;

        leverAnimator.SetTrigger(leverPullTrigger);

        StartCoroutine(PlayStraightTrain());
    }

    private IEnumerator WaitForLeverAnimation()
    {
        yield return null;

        // wait for the lever state to start, with a safety timeout so it can never hang
        float waited = 0f;
        while (!leverAnimator.GetCurrentAnimatorStateInfo(0).IsName(leverStateName))
        {
            waited += Time.deltaTime;
            if (waited > 5f)
            {
                Debug.LogWarning("Lever state '" + leverStateName + "' never started. Check the state name in the Animator.", this);
                yield break;
            }
            yield return null;
        }

        while (leverAnimator.IsInTransition(0))
            yield return null;

        yield return new WaitForSeconds(leverAnimator.GetCurrentAnimatorStateInfo(0).length);
    }

    private IEnumerator PlayRightTrain()
    {
        yield return WaitForLeverAnimation();

        trainAnimator.SetTrigger(trainRightTrigger);
        yield return WaitForTrainAnimation();

        // the player pulled the lever (a deliberate choice)
        yield return ShowResult(false, leverHitsSingleTrack);
    }

    private IEnumerator PlayStraightTrain()
    {
        yield return WaitForLeverAnimation();

        trainAnimator.SetTrigger(trainStraightTrigger);
        yield return WaitForTrainAnimation();

        // the train went straight: either the player froze (timer ran out) or deliberately let it run
        yield return ShowResult(straightWasTimeout, !leverHitsSingleTrack);
    }

    private IEnumerator WaitForTrainAnimation()
    {
        yield return null; // let the trigger take effect

        while (trainAnimator.IsInTransition(0))
            yield return null;

        float length = trainAnimator.GetCurrentAnimatorStateInfo(0).length;
        Debug.Log("Train animation playing, length: " + length);
        yield return new WaitForSeconds(length);
    }

    // NEW
    private IEnumerator ShowResult(bool timedOut, bool trainHitSingle)
    {
        yield return new WaitForSeconds(resultDelay);

        if (resultScreen == null)
        {
            Debug.LogWarning("Result Screen is not assigned, so no result can be shown.", this);
            yield break;
        }

        // the "realist" saved the single person, so the train hit the group
        bool savedTheOne = !trainHitSingle;
        resultScreen.Show(timedOut, savedTheOne, decisionTime, lookedAt, totalPeople);
    }
}