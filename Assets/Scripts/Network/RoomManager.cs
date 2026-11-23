using System;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.SceneManagement;

// Creates and joins rooms using a short code. Lives on the NetworkManager object, so it stays between scenes.
public class RoomManager : MonoBehaviour
{
    [Tooltip("The scene everyone goes to after creating or joining a room. It must be in the build's scene list.")]
    [SerializeField] private string lobbyScene = "Lobby";
    [SerializeField] private int maxPlayers = 4;

    public static RoomManager Instance { get; private set; }

    public ISession Room { get; private set; }
    public string RoomCode => Room != null ? Room.Code : null;

    private void Awake()
    {
        Instance = this;
    }

    // The host creates the room and takes everyone to the lobby
    public async Task CreateRoom()
    {
        await SignIn();

        SessionOptions options = new SessionOptions { MaxPlayers = maxPlayers }.WithRelayNetwork();
        Room = await MultiplayerService.Instance.CreateSessionAsync(options);
        Debug.Log($"Created room {Room.Code}");

        GUIUtility.systemCopyBuffer = Room.Code;
        NetworkManager.Singleton.SceneManager.LoadScene(lobbyScene, LoadSceneMode.Single);
    }

    // Joining puts you in whatever scene the host is in
    public async Task JoinRoom(string code)
    {
        await SignIn();

        Room = await MultiplayerService.Instance.JoinSessionByCodeAsync(code.Trim().ToUpperInvariant());
        Debug.Log($"Joined room {Room.Code}");
    }

    // Unity's online services need you signed in. Anonymous sign-in needs no account.
    private async Task SignIn()
    {
        if (UnityServices.State == ServicesInitializationState.Uninitialized)
        {
            // A random profile, so two copies of the game on one PC count as different players
            string profile = "Player" + Guid.NewGuid().ToString("N").Substring(0, 8);
            await UnityServices.InitializeAsync(new InitializationOptions().SetProfile(profile));
        }

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
    }
}
