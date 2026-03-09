using UnityEngine;
using UnityEngine.SceneManagement;

public abstract class GlobalSingleton<T> : MonoBehaviour where T : Component
{
    private static T instance;
    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindAnyObjectByType<T>();
                if (instance == null)
                {
                    var prefab = Resources.Load<GameObject>($"Managers/{typeof(T).Name}");
                    if (prefab == null)
                    {
                        Debug.LogError($"Singleton: {typeof(T).Name} 프리팹을 Managers/{typeof(T).Name} 경로에서 찾을 수 없습니다.");
                        return null;
                    }
                    var instance = Instantiate(prefab);
                    instance.name = typeof(T).Name; // 생성된 오브젝트 이름 설정
                    DontDestroyOnLoad(instance);
                    Debug.Log($"{instance.name} dynamically created.");
                }
            }
            return instance;
        }
    }

    protected virtual void Awake()
    {
        if (instance == null)
        {
            instance = this as T;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    protected virtual void OnDestory()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    protected virtual void OnSceneLoaded(Scene scene, LoadSceneMode mode) { }
}