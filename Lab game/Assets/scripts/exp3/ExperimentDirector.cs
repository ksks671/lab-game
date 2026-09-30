using UnityEngine;
using System.Collections;
using TMPro;

[System.Serializable]
public class QuizStep
{
    [TextArea] public string presenterLine;
    public int contestantIndex;            // which contestant answers (0 = first)
    [TextArea] public string contestantAnswer;
    public float answerAtSeconds = 5f;     // moment on the clock they answer. 10 or more = they run out of time
    public bool isCorrect = true;
}

public class ExperimentDirector : MonoBehaviour
{
    [Header("Player swap")]
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject lookRig;        // starts inactive
    [SerializeField] private Rigidbody lookRigBody;     // kinematic until the fall

    [Header("Cage")]
    [SerializeField] private Animator cageDoor;
    [SerializeField] private string cageDoorTrigger = "Close";
    [SerializeField] private float cageDoorTime = 2f;
    [SerializeField] private Animator bridge;
    [SerializeField] private string bridgeTrigger = "Retract";
    [SerializeField] private float bridgeTime = 3f;

    [Header("Lights (in switch-on order, presenter last)")]
    [SerializeField] private GameObject[] lightsInOrder;
    [SerializeField] private float lightGap = 0.6f;

    [Header("Quiz")]
    [SerializeField] private QuizStep[] steps;
    [SerializeField] private string presenterName = "Presenter";
    [SerializeField] private string[] contestantNames = { "Contestant 1", "Contestant 2" };
    [SerializeField] private float answerTimeLimit = 10f;
    [SerializeField] private int wrongAnswersToLose = 1;
    [SerializeField] private float lineHold = 2f;
    [SerializeField] private float resultHold = 2f;

    [Header("UI + Board")]
    [SerializeField] private TMP_Text lineText;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private Renderer board;
    [SerializeField] private Color neutralColor = Color.grey;
    [SerializeField] private Color correctColor = Color.green;
    [SerializeField] private Color wrongColor = Color.red;

    [Header("Fall")]
    [SerializeField] private TrapDoor[] fallDoors;      // cage floor
    [SerializeField] private TrapDoor escapeHatch;
    [SerializeField] private float escapeHatchDelay = 1.5f;

    private bool started;

    public void StartExperiment()
    {
        if (started) { Debug.Log("Director: already started, ignoring."); return; }
        started = true;
        StartCoroutine(Run());
    }

    private IEnumerator Run()
    {
        // 1. Swap the player for the look rig
        Debug.Log("Director: swapping player for look rig.");
        Vector3 p = lookRig.transform.position;
        p.x = player.transform.position.x;
        p.z = player.transform.position.z;
        lookRig.transform.position = p;

        player.SetActive(false);
        lookRig.SetActive(true);

        SetBoard(neutralColor);
        if (lineText) lineText.text = "";
        if (timerText) timerText.text = "";

        // 2. Cage door, then bridge
        Debug.Log("Director: cage door closing.");
        cageDoor.SetTrigger(cageDoorTrigger);
        yield return new WaitForSeconds(cageDoorTime);

        Debug.Log("Director: bridge retracting.");
        bridge.SetTrigger(bridgeTrigger);
        yield return new WaitForSeconds(bridgeTime);

        // 3. Lights
        foreach (var l in lightsInOrder)
        {
            l.SetActive(true);
            Debug.Log($"Director: light '{l.name}' on.");
            yield return new WaitForSeconds(lightGap);
        }
        yield return new WaitForSeconds(1f);

        // 4. Quiz
        int wrong = 0;
        for (int i = 0; i < steps.Length; i++)
        {
            var step = steps[i];
            Debug.Log($"Director: question {i + 1}.");

            ShowLine(presenterName, step.presenterLine);
            yield return new WaitForSeconds(lineHold);

            float t = answerTimeLimit;
            bool answered = false;

            while (t > 0f)
            {
                t -= Time.deltaTime;
                if (timerText) timerText.text = Mathf.CeilToInt(Mathf.Max(t, 0f)).ToString();

                float elapsed = answerTimeLimit - t;
                if (step.answerAtSeconds < answerTimeLimit && elapsed >= step.answerAtSeconds)
                {
                    answered = true;
                    ShowLine(ContestantName(step.contestantIndex), step.contestantAnswer);
                    break; // clock freezes on the number when they answer
                }
                yield return null;
            }

            if (!answered)
            {
                Debug.Log("Director: time ran out.");
                if (timerText) timerText.text = "0";
            }

            yield return new WaitForSeconds(1.5f);

            bool correct = answered && step.isCorrect;
            SetBoard(correct ? correctColor : wrongColor);
            Debug.Log($"Director: question {i + 1} -> {(correct ? "CORRECT" : "WRONG")}.");
            yield return new WaitForSeconds(resultHold);

            if (!correct) wrong++;
            if (wrong >= wrongAnswersToLose) break;

            SetBoard(neutralColor);
        }

        // 5. The team always loses (the game is rigged)
        Debug.Log("Director: team loses, falling.");
        yield return StartCoroutine(Fall());
    }

    private IEnumerator Fall()
    {
        if (timerText) timerText.text = "";

        lookRigBody.constraints = RigidbodyConstraints.FreezePositionX
                                | RigidbodyConstraints.FreezePositionZ
                                | RigidbodyConstraints.FreezeRotation;
        lookRigBody.isKinematic = false;
        lookRigBody.useGravity = true;

        foreach (var door in fallDoors) door.Open();

        yield return new WaitForSeconds(escapeHatchDelay);

        if (escapeHatch != null)
        {
            Debug.Log("Director: escape hatch opening.");
            escapeHatch.Open();
        }
    }

    private void ShowLine(string speaker, string text)
    {
        if (lineText) lineText.text = $"{speaker}: {text}";
    }

    private string ContestantName(int index)
    {
        if (index >= 0 && index < contestantNames.Length) return contestantNames[index];
        Debug.LogWarning($"Director: no contestant name for index {index}.");
        return "Contestant";
    }

    private void SetBoard(Color c)
    {
        if (board != null) board.material.color = c;
    }
}