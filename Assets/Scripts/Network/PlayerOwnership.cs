using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

// Every computer has a copy of every player. Only your own copy reads your keyboard, shows your camera and has hug arms.
// The listed components start switched off in the prefab, so other players' copies never take over your cursor or camera.
public class PlayerOwnership : NetworkBehaviour
{
    [Tooltip("Turned on only for your own player: input, camera, audio listener.")]
    [SerializeField] private Behaviour[] ownerOnlyComponents;
    [Tooltip("Removed from other players' copies, like the hug arms.")]
    [SerializeField] private GameObject[] ownerOnlyObjects;
    [Tooltip("The visible body. Your own is hidden from your camera but still casts a shadow.")]
    [SerializeField] private Transform body;

    public override void OnNetworkSpawn()
    {
        if (IsOwner) TakeControl();
        else HideOwnerOnlyObjects();
    }

    // Testing a scene on its own: there's no network to follow, so the body moves with normal physics
    public void PlayOffline()
    {
        foreach (NetworkTransform sync in GetComponentsInChildren<NetworkTransform>()) sync.enabled = false;

        foreach (NetworkRigidbody networkBody in GetComponentsInChildren<NetworkRigidbody>())
        {
            networkBody.enabled = false;
            networkBody.GetComponent<Rigidbody>().isKinematic = false;
        }

        TakeControl();
    }

    private void TakeControl()
    {
        foreach (Behaviour component in ownerOnlyComponents) component.enabled = true;
        HideOwnBody();
    }

    // Only changes this computer's copy of you, so everyone else still sees your body
    private void HideOwnBody()
    {
        foreach (Renderer part in body.GetComponentsInChildren<Renderer>())
        {
            part.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
        }
    }

    private void HideOwnerOnlyObjects()
    {
        foreach (GameObject ownerOnly in ownerOnlyObjects) ownerOnly.SetActive(false);
    }
}
