using UnityEngine;
using UnityEngine.UI;

// Shows the room code in the corner of the screen while you're in a room, so the host can share it
public class RoomCodeDisplay : MonoBehaviour
{
    [SerializeField] private Text label;

    private string shownCode;

    private void Update()
    {
        string code = RoomManager.Instance.RoomCode;
        if (code == shownCode) return;

        shownCode = code;
        label.enabled = code != null;
        label.text = "Room code: " + code;
    }
}
