using Unity.Netcode;
using UnityEngine;
using static GameInstance;




public class RpcManager : NetworkBehaviour {
    private CustomizationMenu customizationMenuScript = null;
    private LevelSelectMenu levelSelectMenuScript = null;

    public bool initialized = false;

    public void Initialize() {
        if (initialized)
            return;


        SetupReferences();
        initialized = true;
    }
    private void SetupReferences() {

        customizationMenuScript = GetInstance().GetCustomizationMenuScript();
        levelSelectMenuScript = GetInstance().GetLevelSelectMenuScript();
        //I COULD CACHE MORE HERE OTHERWISE ITS WEIRD!
    }


    [ClientRpc]
    public void ProccedToCustomizationMenuClientRpc() {
        GetInstance().SetGameState(GameState.CUSTOMIZATION_MENU);
    }
    [ClientRpc]
    public void ProccedToMatchStartClientRpc(ulong senderID) {
        if (GetInstance().GetClientID() == senderID)
            return;

        GetInstance().ProccedToMatchStartRpc();
    }

    [ClientRpc]
    public void RelayLevelSelectorRoleClientRpc(ClientRpcParams clientRpcParameters = default) {
        levelSelectMenuScript.ActivateStartButton();
    }


    [ClientRpc]
    public void RelayRpcManagerReferenceClientRpc(NetworkObjectReference reference) {
        GetInstance().SetReceivedRpcManagerRef(reference);
    }
    [ClientRpc]
    public void RelayPlayerReferenceClientRpc(NetworkObjectReference reference, Player.PlayerType player, ClientRpcParams clientRpcParameters = default) {
        GetInstance().SetReceivedPlayerRefRpc(reference, player);
    }
    [ClientRpc]
    public void RelayPlayerSpawnPositionClientRpc(Vector3 spawnPoint, ClientRpcParams clientRpcParameters = default) {
        GetInstance().SetReceivedPlayerSpawnPointRpc(spawnPoint);
    }

    [ServerRpc (RequireOwnership = false)]
    public void UpdateProjectileSpawnRequestServerRpc(ulong senderID, Player.PlayerType playerType, Projectile.ProjectileType projectileType) {
        RelayProjectileSpawnRequestClientRpc(senderID, playerType, projectileType);
    }
    [ClientRpc]
    public void RelayProjectileSpawnRequestClientRpc(ulong senderID, Player.PlayerType playerType, Projectile.ProjectileType projectileType) {
        if (GetInstance().GetClientID() == senderID)
                return;

        //BUG: Can still shoot and boost while input disabled!

        var script = GetInstance().GetCurrentLevelScript();
        if (!script) {
            Debug.LogWarning("Received projectile spawn request while level was null");
            return;
        }

        //Try to play muzzle flash from here!
        //if(playerType == Player.PlayerType.PLAYER_1)
            //GetInstance().GetPlayer1Script().PlayMuzzleFlashAnim(projectileType.ToString())

        script.ReceiveProjectileSpawnRequest(playerType, projectileType);
    }

    [ServerRpc (RequireOwnership = true)]
    public void UpdateRoundTimerServerRpc(ulong senderID, float value) {
        RelayRoundTimerClientRpc(senderID, value);
    }
    [ClientRpc]
    public void RelayRoundTimerClientRpc(ulong senderID, float value) {
        if (senderID == GetInstance().GetClientID())
            return;

        GetInstance().GetMatchDirector().ReceiveRoundTimerRpc(value);
    }



    [ServerRpc(RequireOwnership = true)]
    public void UpdatePickupIDsServerRpc(ulong senderID, int[] data) {
        RelayPickupIDsClientRpc(senderID, data);
    }
    [ClientRpc]
    public void RelayPickupIDsClientRpc(ulong senderID, int[] data) {
        if (senderID == GetInstance().GetClientID())
            return;

        var script = GetInstance().GetCurrentLevelScript();
        if (!script) {
            Debug.LogWarning("Received pickup IDs while level was null"); //Message out dated!
            return;
        }

        script.ReceivePickupIDsRpc(data);
    }


    [ServerRpc(RequireOwnership = true)]
    public void DeactivatePickupSpawnsServerRpc(ulong senderID) {
        RelayDeactivatePickupSpawnsClientRpc(senderID);
    }
    [ClientRpc]
    public void RelayDeactivatePickupSpawnsClientRpc(ulong senderID) {
        if (senderID == GetInstance().GetClientID())
            return;

        var level = GetInstance().GetCurrentLevelScript();
        if (level == null)
            return;


        level.DeactivateAllPickups();
    }


    [ServerRpc(RequireOwnership = true)]
    public void UpdatePickupSpawnsServerRpc(ulong senderID, int ID, int spawnIndex) {
        RelayPickupSpawnRequestClientRpc(senderID, ID, spawnIndex);
    }
    [ClientRpc]
    public void RelayPickupSpawnRequestClientRpc(ulong senderID, int ID, int spawnIndex) {
        if (senderID == GetInstance().GetClientID())
            return;

        var level = GetInstance().GetCurrentLevelScript();
        if (level == null)
            return;

        level.ReceivePickupSpawnRequestRpc(ID, spawnIndex);
    }


    [ServerRpc (RequireOwnership = false)]
    public void UpdatePlayer2SelectionServerRpc(ulong senderID, int index) {

        RelayPlayer2SelectionClientRpc(senderID, index);
    }
    [ClientRpc] 
    public void RelayPlayer2SelectionClientRpc(ulong senderID, int index) {
        if (senderID == GetInstance().GetClientID())
            return;

        customizationMenuScript.ReceivePlayer2CharacterIndexRpc(index);
    }



    [ServerRpc(RequireOwnership = false)]
    public void UpdatePlayer2ColorSelectionServerRpc(ulong senderID, Color color) {
        RelayPlayer2ColorSelectionClientRpc(senderID, color);
    }
    [ClientRpc]
    public void RelayPlayer2ColorSelectionClientRpc(ulong senderID, Color color) {
        if (senderID == GetInstance().GetClientID())
            return;

        customizationMenuScript.ReceivePlayer2ColorSelectionRpc(color);
    }


    [ServerRpc(RequireOwnership = false)]
    public void UpdatePlayer2ReadyCheckServerRpc(ulong senderID, bool ready) {
        RelayPlayer2ReadyCheckClientRpc(senderID, ready);
    }
    [ClientRpc]
    public void RelayPlayer2ReadyCheckClientRpc(ulong senderID, bool ready) {
        if (senderID == GetInstance().GetClientID())
            return;

        customizationMenuScript.ReceivePlayer2ReadyCheckRpc(ready);
    }


    [ServerRpc(RequireOwnership = false)]
    public void UpdateSelectedLevelPreviewServerRpc(ulong senderID, int index) {
        RelaySelectedLevelPreviewClientRpc(senderID, index);
    }
    [ClientRpc]
    public void RelaySelectedLevelPreviewClientRpc(ulong senderID, int index) {
        if (senderID == (ulong)GetInstance().GetClientID())
            return;

        levelSelectMenuScript.ReceiveSelectedLevelPreviewRpc(index);
    }


    [ServerRpc(RequireOwnership = false)]
    public void UpdateSelectedLevelIndexServerRpc(ulong senderID, int index) {
        RelaySelectedLevelIndexClientRpc(senderID, index);
    }
    [ClientRpc]
    public void RelaySelectedLevelIndexClientRpc(ulong senderID, int index) {
        if (senderID == (ulong)GetInstance().GetClientID())
            return;

        levelSelectMenuScript.ReceiveLevelSelectionRpc(index);
    }


    [ServerRpc(RequireOwnership = false)]
    public void UpdatePlayer2PositionServerRpc(float input) { 
        GetInstance().GetPlayer2Script().ProccessReceivedMovementRpc(input);
    }
    [ServerRpc(RequireOwnership = false)]
    public void UpdatePlayer2RotationServerRpc(float input) {
        GetInstance().GetPlayer2Script().ProccessReceivedRotationRpc(input);
    }

}
