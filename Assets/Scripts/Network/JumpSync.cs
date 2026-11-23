using Unity.Netcode;
using UnityEngine;

// A jump is a single moment, not something that stays true, so it's sent as a message (an RPC) instead of a synced value.
// The owner jumps for real; everyone else is told to make this player's body jump on their screen.
public class JumpSync : NetworkBehaviour
{
    [SerializeField] private PlayerMove move;
    [SerializeField] private RagdollFollow ragdoll;

    public override void OnNetworkSpawn()
    {
        if (IsOwner) move.Jumped += SendJump;
    }

    public override void OnNetworkDespawn()
    {
        move.Jumped -= SendJump;
    }

    private void SendJump()
    {
        JumpRpc();
    }

    // Runs on every computer except the owner's, which already jumped
    [Rpc(SendTo.NotOwner)]
    private void JumpRpc()
    {
        ragdoll.Jump();
    }
}
