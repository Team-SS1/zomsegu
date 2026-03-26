using UnityEngine;

public class SceneSingleton<T> : MonoBehaviour where T : Component
{
    private static T instance;
    public static T Instance
    {
        get
        {
            if (isQuitting) return null;
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

                instance = obj.GetComponent<T>();

                if (instance == null)
                {
                    Debug.LogError($"{typeof(T).Name} 프리팹에 {typeof(T).Name} 컴포넌트가 없습니다.");
                    return null;
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
    }

    private void OnApplicationQuit()
    {
        isQuitting = true;
    }
}
