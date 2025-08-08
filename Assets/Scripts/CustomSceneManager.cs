using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CustomSceneManager : SingletonMonoBehaviour<CustomSceneManager>
{
    public event Action<string> OnSceneLoadStarted;
    public event Action<string> OnSceneLoaded;
    
    private void Awake()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += HandleNetworkSceneLoaded;
        }
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= HandleNetworkSceneLoaded;
        }
    }

    private void HandleNetworkSceneLoaded(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        Debug.Log($"Scene loaded: {sceneName}");
        OnSceneLoaded?.Invoke(sceneName);
    }

    /// <summary>
    /// Sadece host tarafından çağrılmalıdır. Sahneyi tüm oyuncular için değiştirir.
    /// </summary>
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