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

            if (hit.transform == transform ||
                hit.transform.IsChildOf(transform))
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

    private IEnumerator PlayRightTrain()
    {
        yield return WaitForLeverAnimation();

        trainAnimator.SetTrigger(trainRightTrigger);
    }

    private IEnumerator PlayStraightTrain()
    {
        yield return WaitForLeverAnimation();

        trainAnimator.SetTrigger(trainStraightTrigger);
    }

    private IEnumerator WaitForLeverAnimation()
    {
        yield return null;

        while (!leverAnimator.GetCurrentAnimatorStateInfo(0).IsName("LeverPull"))
        {
            yield return null;
        }

        float animationLength =
            leverAnimator.GetCurrentAnimatorStateInfo(0).length;

        yield return new WaitForSeconds(animationLength);
    }
}

