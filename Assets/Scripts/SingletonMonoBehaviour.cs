using UnityEngine;
using UnityEngine.SceneManagement;


public class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
{
    protected virtual bool _isPermanent => true;

    public static T Instance
    {
        get
        {
            if (_instance != null)
                return _instance;

            return null;
        }
    }

    private static T _instance;

    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;

            if (!_isPermanent)
                return;

            if (transform.parent != null)
                transform.SetParent(null);

            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this as T)
        {
            Destroy(gameObject);
        }
    }

    protected virtual void OnEnable()
    {
        SceneManager.activeSceneChanged += OnSceneChange;
    }

    protected virtual void OnDisable()
    {
        SceneManager.activeSceneChanged -= OnSceneChange;
    }

    protected virtual void OnSceneChange(Scene unloadedScene, Scene loadedScene)
    {
    }

    protected virtual void OnDestroy()
    {
        if (_instance == this as T)
            _instance = null;
    }
}