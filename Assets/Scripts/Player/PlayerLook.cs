using UnityEngine;

public class PlayerLook : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform head;
    [SerializeField] private float sensitivity = 0.2f;
    [SerializeField] private float lookSmoothing = 25f;
    [SerializeField] private float maxY = 70f;
    [SerializeField] private float minY = -50f;

    [Header("Follow")]
    [SerializeField] private float eyeHeight = 0.1f;
    [SerializeField] private float forwardOffset = 0.4f;
    [SerializeField] private float followSmoothTime = 0.05f;

    public float yaw;
    private float pitch;
    private Vector3 smoothedHead;
    private Vector2 smoothedLook;
    private Vector3 followVelocity;

    private PlayerActions.OnFootActions onFoot;

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
        yaw = playerCamera.transform.eulerAngles.y;
        smoothedHead = head.position;
    }

    private void LateUpdate()
    {
        Look();
        FollowHead();
    }

    private void Look()
    {
        Vector2 raw = onFoot.Look.ReadValue<Vector2>();

        float t = 1f - Mathf.Exp(-lookSmoothing * Time.deltaTime);
        smoothedLook = Vector2.Lerp(smoothedLook, raw, t);

        yaw += smoothedLook.x * sensitivity;
        pitch = Mathf.Clamp(pitch - smoothedLook.y * sensitivity, -maxY, -minY);

        playerCamera.transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    private void FollowHead()
    {
        smoothedHead = Vector3.SmoothDamp(smoothedHead, head.position, ref followVelocity, followSmoothTime);

        Vector3 flatForward = Quaternion.Euler(0f, yaw, 0f) * Vector3.forward;
        playerCamera.transform.position = smoothedHead + Vector3.up * eyeHeight + flatForward * forwardOffset;
    }
}