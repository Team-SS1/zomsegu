using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] private string mainSceneName = "MainScene";
    public static string NextSceneName;
    public void LoadMainScene()
    {
        NextSceneName = mainSceneName;
        SceneManager.LoadScene("LoadingScene");
    }
}

// 현재로선 TitleScene -> LoadingScene -> MainScene으로 가는 구조를 우선적으로 만들었습니다. 