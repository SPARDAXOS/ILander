using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Networking.Transport;
using Unity.Networking;
using UnityEngine;
using static GameInstance;

public class RpcManager : NetworkBehaviour
{
    private CustomizationMenu customizationMenuScript = null;
    private LevelSelectMenu levelSelectMenu = null;

    public bool initialized = false;

    public void Initialize() {
        if (initialized)
            return;


        SetupReferences();
        initialized = true;
    }
    private void SetupReferences() {

        customizationMenuScript = GetInstance().GetCustomizationMenuScript();
        levelSelectMenu = GetInstance().GetLevelSelectMenuScript();

    }


    [ClientRpc]
    public void ProccedToCustomizationMenuClientRpc() {
        GetInstance().SetGameState(GameState.CUSTOMIZATION_MENU);
    }


    //TODO: Avoid the senderID == self check if possible and use params instead to send 1 less packet each time!



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

        customizationMenuScript.SetPlayer2CharacterIndex(index);
    }


    [ServerRpc(RequireOwnership = false)]
    public void UpdatePlayer2ReadyCheckServerRpc(ulong senderID, bool ready) {

        RelayPlayer2ReadyCheckClientRpc(senderID, ready);
    }




    [ClientRpc]
    public void RelayPlayer2ReadyCheckClientRpc(ulong senderID, bool ready) {
        if (senderID == (ulong)GetInstance().GetClientID())
            return;

        customizationMenuScript.SetPlayer2ReadyCheck(ready);
    }


    [ServerRpc(RequireOwnership = false)]
    public void UpdateSelectedLevelIndexServerRpc(ulong senderID, int index) {

        RelaySelectedLevelIndexClientRpc(index);
    }
    [ClientRpc]
    public void RelaySelectedLevelIndexClientRpc(int index, ClientRpcParams clientRpcParameters = default) {

    }

    [ClientRpc]
    public void RelayLevelSelectorRoleClientRpc(ClientRpcParams clientRpcParameters = default) {
        levelSelectMenu.ActivateStartButton();
    }



}
