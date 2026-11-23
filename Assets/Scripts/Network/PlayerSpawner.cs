using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

// The host spawns one player for each person once they've loaded this scene
public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private NetworkObject playerPrefab;
    [SerializeField] private Transform[] spawnPoints;

    private NetworkManager network;

    private void Start()
    {
        network = NetworkManager.Singleton;

        // Pressing Play on this scene directly: just give yourself a player
        if (network == null || !network.IsListening)
        {
            SpawnOffline();
            return;
        }

        if (!network.IsServer) return;

        network.SceneManager.OnLoadComplete += OnPlayerLoadedScene;
        network.SceneManager.OnSynchronizeComplete += OnPlayerJoinedLate;

        // Anyone who finished loading before this script started
        foreach (ulong clientId in network.ConnectedClientsIds) SpawnPlayer(clientId);
    }

    private void OnDestroy()
    {
        if (network == null || network.SceneManager == null) return;

        network.SceneManager.OnLoadComplete -= OnPlayerLoadedScene;
        network.SceneManager.OnSynchronizeComplete -= OnPlayerJoinedLate;
    }

    private void OnPlayerLoadedScene(ulong clientId, string sceneName, LoadSceneMode mode)
    {
        if (sceneName == gameObject.scene.name) SpawnPlayer(clientId);
    }

    private void OnPlayerJoinedLate(ulong clientId)
    {
        SpawnPlayer(clientId);
    }

    private void SpawnPlayer(ulong clientId)
    {
        if (network.ConnectedClients[clientId].PlayerObject != null) return;

        Transform point = GetSpawnPoint(clientId);
        NetworkObject player = Instantiate(playerPrefab, point.position, point.rotation);
        player.SpawnAsPlayerObject(clientId, destroyWithScene: true);
    }

    private void SpawnOffline()
    {
        Transform point = GetSpawnPoint(0);
        NetworkObject player = Instantiate(playerPrefab, point.position, point.rotation);
        player.GetComponent<PlayerOwnership>().PlayOffline();
    }

    private Transform GetSpawnPoint(ulong clientId)
    {
        return spawnPoints[(int)(clientId % (ulong)spawnPoints.Length)];
    }
}
