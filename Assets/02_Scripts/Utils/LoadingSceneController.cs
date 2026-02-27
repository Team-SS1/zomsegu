using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class LoadingSceneController : MonoBehaviour
{
    [SerializeField] private Image loadingBar;
    [SerializeField] private float fakeLoadingTime = 1.5f;

    private void Start()
    {
        StartCoroutine(LoadNextScene());
    }

    private IEnumerator LoadNextScene()
    {
        string nextScene = SceneLoader.NextSceneName;

        if (string.IsNullOrEmpty(nextScene))
        {
            nextScene = "TitleScene";
        }

        float timer = 0f;
        loadingBar.fillAmount = 0f;

        while (timer < fakeLoadingTime)
        {
            timer += Time.deltaTime;
            loadingBar.fillAmount = timer / fakeLoadingTime;
            yield return null;
        }

        SceneLoader.NextSceneName = null;
        SceneManager.LoadScene(nextScene);
    }
}