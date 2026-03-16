using UnityEngine;
using UnityEngine.SceneManagement;

public abstract class GlobalSingleton<T> : MonoBehaviour where T : Component
{
    private static T instance;

    public static T Instance
    {
        get
        {
            if (isQuitting) return null;
            if (instance == null)
            {
                instance = FindAnyObjectByType<T>();

                if (instance == null)
                {
                    GameObject prefab = Resources.Load<GameObject>($"Managers/{typeof(T).Name}");
                    if (prefab == null)
                    {
                        Debug.LogError($"Singleton: {typeof(T).Name} 프리팹을 Managers/{typeof(T).Name} 경로에서 찾을 수 없습니다.");
                        return null;
                    }

                    GameObject obj = Instantiate(prefab);
                    obj.name = typeof(T).Name;
                    DontDestroyOnLoad(obj);

                    instance = obj.GetComponent<T>();

                    if (instance == null)
                    {
                        Debug.LogError($"{typeof(T).Name} 프리팹에 {typeof(T).Name} 컴포넌트가 없습니다.");
                        return null;
                    }
                }
            }

            return instance;
        }
    }

    private static bool isQuitting = false;

    protected virtual void Awake()
    {
        if (instance == null)
        {
            instance = this as T;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // instance가 null이 아니고, 새로 만들어진 게 아닐 때
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    protected virtual void OnDestroy()
    {
        if (instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            if (!isQuitting) instance = null;
        }
    }

    protected virtual void OnSceneLoaded(Scene scene, LoadSceneMode mode) { }

    private void OnApplicationQuit()
    {
        isQuitting = true;
    }

    // 플레이 시작 시 이전 play 정보 없게 하기
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        instance = null;
        isQuitting = false;
    }
}