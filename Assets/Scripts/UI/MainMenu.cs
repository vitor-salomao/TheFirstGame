using System;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("Main")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private Button playButton;
    [SerializeField] private Button quitButton;

    [Header("Rooms")]
    [SerializeField] private GameObject roomPanel;
    [SerializeField] private Button createButton;
    [SerializeField] private InputField codeInput;
    [SerializeField] private Button joinButton;
    [SerializeField] private Button backButton;
    [SerializeField] private Text statusText;

    private void OnEnable()
    {
        playButton.onClick.AddListener(ShowRooms);
        quitButton.onClick.AddListener(Quit);
        createButton.onClick.AddListener(CreateRoom);
        joinButton.onClick.AddListener(JoinRoom);
        backButton.onClick.AddListener(ShowMain);
    }

    private void OnDisable()
    {
        playButton.onClick.RemoveListener(ShowRooms);
        quitButton.onClick.RemoveListener(Quit);
        createButton.onClick.RemoveListener(CreateRoom);
        joinButton.onClick.RemoveListener(JoinRoom);
        backButton.onClick.RemoveListener(ShowMain);
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        ShowMain();
    }

    private void ShowMain()
    {
        mainPanel.SetActive(true);
        roomPanel.SetActive(false);
    }

    private void ShowRooms()
    {
        mainPanel.SetActive(false);
        roomPanel.SetActive(true);
        SetBusy(false, "");
    }

    // async void is fine for button clicks: errors are caught and shown below
    private async void CreateRoom()
    {
        SetBusy(true, "Creating room...");

        try
        {
            await RoomManager.Instance.CreateRoom();
        }
        catch (Exception error)
        {
            Debug.LogException(error);
            SetBusy(false, "Couldn't create a room: " + error.Message);
        }
    }

    private async void JoinRoom()
    {
        string code = codeInput.text.Trim();
        if (code.Length == 0)
        {
            statusText.text = "Type in a room code first.";
            return;
        }

        SetBusy(true, $"Joining room {code}...");

        try
        {
            await RoomManager.Instance.JoinRoom(code);
        }
        catch (Exception error)
        {
            Debug.LogException(error);
            SetBusy(false, "Couldn't join: " + error.Message);
        }
    }

    // Stops double clicks while waiting for the internet
    private void SetBusy(bool busy, string message)
    {
        createButton.interactable = !busy;
        joinButton.interactable = !busy;
        backButton.interactable = !busy;
        statusText.text = message;
    }

    private void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
