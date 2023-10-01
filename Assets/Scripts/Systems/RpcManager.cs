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
