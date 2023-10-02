using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Networking.Transport;
using Unity.Networking;
using UnityEngine;
using static GameInstance;

public class RpcManager : NetworkBehaviour
{

    [ClientRpc]
    public void ProccedToCustomizationMenuClientRpc() {
        GetInstance().SetGameState(GameState.CUSTOMIZATION_MENU);
    }


    //This is the way to ask and receive stuff
    //[ServerRpc (RequireOwnership = false)]
    //public void GetRpcManagerReferenceServerRpc(ulong senderID) {
    //    Debug.Log("Received request for rpc manager from " + senderID);
    //    ClientRpcParams clientRpcParams = new ClientRpcParams();
    //    clientRpcParams.Send = new ClientRpcSendParams { TargetClientIds = new ulong[] { senderID } };
    //    
    //    RelayRpcManagerReferenceClientRpc(GetInstance().GetRpcManager(), clientRpcParams);
    //}
    //
    //[ClientRpc]
    //public void RelayRpcManagerReferenceClientRpc(NetworkObjectReference reference, ClientRpcParams clientRpcParameters = default)
    //{
    //    GetInstance().CheckReceivedRpcManagerRef(reference);
    //}



    [ClientRpc]
    public void RelayRpcManagerReferenceClientRpc(NetworkObjectReference reference)
    {
        GetInstance().SetReceivedRpcManagerRef(reference);
    }

    [ClientRpc]
    public void RelayPlayerReferenceClientRpc(NetworkObjectReference reference, Player.PlayerType player, ClientRpcParams clientRpcParameters = default)
    {
        Debug.Log("Received player ref rpc for " + player.ToString());
        GetInstance().SetReceivedRpcPlayerRef(reference, player);
    }


    [ServerRpc (RequireOwnership = false)]
    public void UpdatePlayer2SelectionServerRpc(ulong senderID, int index) {

        RelayPlayer2SelectionClientRpc(senderID, index);
    }

    [ClientRpc]
    public void RelayPlayer2SelectionClientRpc(ulong senderID, int index) {
        if (senderID == (ulong)GetInstance().GetClientID())
            return;

        GetInstance().UpdatePlayer2Selection(index);
    }





}
