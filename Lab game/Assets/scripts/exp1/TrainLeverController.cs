using UnityEngine;
using System.Collections;

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


    [Header("Door")]
    [SerializeField] private Animator doorAnimator;
    [SerializeField] private string doorOpenTrigger = "Open";
    [SerializeField] private float doorExtraDelay = 1f; // pause after the train finishes

    [SerializeField] private string leverStateName = "change";

    private float timer;
    private bool sequenceFinished;

    private void Start()
    {
        timer = timeLimit;

        if (leverAnimator == null)
            Debug.LogError("Lever Animator is not assigned!", this);

        if (trainAnimator == null)
            Debug.LogError("Train Animator is not assigned!", this);
    }

    private void Update()
    {
        if (sequenceFinished)
            return;

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            StraightRoute();
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("LEFT CLICK DETECTED");
            CheckLeverClick();
        }
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

    private void RightRoute()
    {
        if (sequenceFinished)
            return;

        sequenceFinished = true;

        if (leverAnimator == null || trainAnimator == null)
            return;

        leverAnimator.SetTrigger(leverPullTrigger);

        StartCoroutine(PlayRightTrain());
    }

    private void StraightRoute()
    {
        if (sequenceFinished)
            return;

        sequenceFinished = true;

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
        yield return OpenDoor();
    }

    private IEnumerator PlayStraightTrain()
    {
        yield return WaitForLeverAnimation();

        trainAnimator.SetTrigger(trainStraightTrigger);
        yield return WaitForTrainAnimation();
        yield return OpenDoor();
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

    private IEnumerator OpenDoor()
    {
        yield return new WaitForSeconds(doorExtraDelay);

        if (doorAnimator == null)
        {
            Debug.LogWarning("Door Animator is not assigned!", this);
            yield break;
        }

        Debug.Log("Door opening.");
        doorAnimator.SetTrigger(doorOpenTrigger);
    }
}

