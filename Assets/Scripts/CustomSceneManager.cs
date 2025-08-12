using System;
using System.Collections.Generic;
using DefaultNamespace;
using NaughtyAttributes;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CustomSceneManager : SingletonMonoBehaviour<CustomSceneManager>
{
    public event Action<string> OnSceneLoadStarted;
    public event Action OnGameSceneLoaded;
    public event Action OnGameSceneUnloaded;

    protected override void OnEnable()
    {
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
        base.OnEnable();
    }

    protected override void OnDisable()
    {
        SceneManager.activeSceneChanged -= OnActiveSceneChanged;
        base.OnDisable();
    }

    private void OnActiveSceneChanged(Scene oldScene, Scene newScene)
    {
        if (newScene.name == SceneKeys.GameScene)
            OnGameSceneLoaded?.Invoke();
        else if (oldScene.name == SceneKeys.GameScene)
            OnGameSceneUnloaded?.Invoke();
    }

    public void LoadNetworkScene(string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
    {
        if (!NetworkManager.Singleton.IsHost)
        {
            Debug.LogWarning("Only the host can load a networked scene.");
            return;
        }

        Debug.Log($"Loading network scene: {sceneName}");
        OnSceneLoadStarted?.Invoke(sceneName);
        NetworkManager.Singleton.SceneManager.LoadScene(sceneName, mode);
    }
}