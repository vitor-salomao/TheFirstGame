using UnityEngine;

public class PlayerLook : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private float sensitivity = 0.1f;
    [SerializeField] private float maxMinY = 85f;

    private float yaw;
    private float pitch;
    private PlayerActions.OnFootActions onFoot;

    public float Yaw => yaw;

    private void Awake()
    {
        onFoot = new PlayerActions().OnFoot;
    }

    private void OnEnable()
    {
        onFoot.Enable();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnDisable()
    {
        onFoot.Disable();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Start()
    {
        yaw = cameraPivot.eulerAngles.y;
    }

    private void LateUpdate()
    {
        Vector2 look = onFoot.Look.ReadValue<Vector2>();

        yaw += look.x * sensitivity;
        pitch = Mathf.Clamp(pitch - look.y * sensitivity, -maxMinY, maxMinY);

        cameraPivot.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }
}