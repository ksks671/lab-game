using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelLoader : MonoBehaviour
{
    [Header("Optional fade (black panel with a Canvas Group)")]
    [SerializeField] private CanvasGroup fadeGroup;
    [SerializeField] private float fadeTime = 1f;

    private bool loading;

    private void Start()
    {
        // fade in from black when the scene starts
        if (fadeGroup != null) StartCoroutine(Fade(1f, 0f));
    }

    public void LoadNextLevel()
    {
        int next = SceneManager.GetActiveScene().buildIndex + 1;

        if (next >= SceneManager.sceneCountInBuildSettings)
        {
            Debug.LogWarning("LevelLoader: this is the last scene in Build Settings, nothing to load.", this);
            return;
        }

        Debug.Log("LevelLoader: loading next level, index " + next);
        StartCoroutine(LoadRoutine(next, null));
    }

    public void LoadLevelByIndex(int buildIndex)
    {
        Debug.Log("LevelLoader: loading index " + buildIndex);
        StartCoroutine(LoadRoutine(buildIndex, null));
    }

    public void LoadLevelByName(string sceneName)
    {
        Debug.Log("LevelLoader: loading scene " + sceneName);
        StartCoroutine(LoadRoutine(-1, sceneName));
    }

    private IEnumerator LoadRoutine(int index, string sceneName)
    {
        if (loading) yield break;
        loading = true;

        if (fadeGroup != null) yield return Fade(0f, 1f);

        Time.timeScale = 1f;

        if (!string.IsNullOrEmpty(sceneName)) SceneManager.LoadScene(sceneName);
        else SceneManager.LoadScene(index);
    }

    private IEnumerator Fade(float from, float to)
    {
        fadeGroup.gameObject.SetActive(true);
        float t = 0f;

        while (t < fadeTime)
        {
            t += Time.unscaledDeltaTime;
            fadeGroup.alpha = Mathf.Lerp(from, to, t / fadeTime);
            yield return null;
        }

        fadeGroup.alpha = to;
    }
}