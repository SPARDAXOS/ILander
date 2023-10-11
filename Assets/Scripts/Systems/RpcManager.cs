using Unity.Netcode;
using UnityEngine;
using static GameInstance;

public class RpcManager : NetworkBehaviour {
    public enum PlayerHealthProcess {
        ADDITION,
        SUBTRACTION
    }
    public enum PlayerFuelProcess {
        ADDITION,
        SUBTRACTION
    }


    //Health Calculations
    [ServerRpc (RequireOwnership = false)]
    public void ExecutePlayerHealthProcessServerRpc(PlayerHealthProcess process, Player.PlayerType player, float amount) {
        if (player == Player.PlayerType.NONE)
            return;

        Player script = null;
        PlayerCharacterData data = new PlayerCharacterData();

        if (player == Player.PlayerType.PLAYER_1) {
            script = GetGameInstance().GetPlayer1Script();
            data = script.GetPlayerData();
        }
        else if (player == Player.PlayerType.PLAYER_2) {
            script = GetGameInstance().GetPlayer2Script();
            data = script.GetPlayerData();
        }

        float currentHealth = script.GetCurrentHealth();
        float healthCap = data.statsData.healthCap;
        if (process == PlayerHealthProcess.ADDITION) {
            currentHealth += amount;
            if (currentHealth > healthCap)
                currentHealth = healthCap;
        }
        else if (process == PlayerHealthProcess.SUBTRACTION) {
            currentHealth -= amount;
            if (currentHealth <= 0.0f)
                currentHealth = 0.0f;
        }

        RelayPlayerHealthResultsClientRpc(player, currentHealth, currentHealth / healthCap);
    }
    [ClientRpc]
    public void RelayPlayerHealthResultsClientRpc(Player.PlayerType player, float health, float percentage) {

        GetGameInstance().GetPlayer1Script().ReceivePlayerHealthProcessRpc(player, health, percentage);
        GetGameInstance().GetPlayer2Script().ReceivePlayerHealthProcessRpc(player, health, percentage);
    }


    //Fuel Calculations
    [ServerRpc(RequireOwnership = false)]
    public void ExecutePlayerFuelProcessServerRpc(PlayerFuelProcess process, Player.PlayerType player, float amount) {
        if (player == Player.PlayerType.NONE)
            return;

        Player script = null;
        PlayerCharacterData data = new PlayerCharacterData();

        if (player == Player.PlayerType.PLAYER_1) {
            script = GetGameInstance().GetPlayer1Script();
            data = script.GetPlayerData();
        }
        else if (player == Player.PlayerType.PLAYER_2) {
            script = GetGameInstance().GetPlayer2Script();
            data = script.GetPlayerData();
        }

        float currentFuel = script.GetCurrentFuel();
        float fuelCap = data.statsData.fuelCap;
        if (process == PlayerFuelProcess.ADDITION) {
            currentFuel += amount;
            if (currentFuel > fuelCap)
                currentFuel = fuelCap;
        }
        else if (process == PlayerFuelProcess.SUBTRACTION) {
            currentFuel -= amount;
            if (currentFuel <= 0.0f)
                currentFuel = 0.0f;
        }

        RelayPlayerFuelResultsClientRpc(player, currentFuel, currentFuel / fuelCap);
    }
    [ClientRpc]
    public void RelayPlayerFuelResultsClientRpc(Player.PlayerType player, float fuel, float percentage) {

        GetGameInstance().GetPlayer1Script().ReceivePlayerFuelProcessRpc(player, fuel, percentage);
        GetGameInstance().GetPlayer2Script().ReceivePlayerFuelProcessRpc(player, fuel, percentage);
    }


    //Connection
    [ClientRpc]
    public void ProccedToCustomizationMenuClientRpc() {
        GetGameInstance().SetGameState(GameState.CUSTOMIZATION_MENU);
    }


    //References
    [ClientRpc]
    public void RelayRpcManagerReferenceClientRpc(NetworkObjectReference reference) {
        GetGameInstance().SetReceivedRpcManagerRef(reference);
    }
    [ClientRpc]
    public void RelayPlayerReferenceClientRpc(NetworkObjectReference reference, Player.PlayerType player, ClientRpcParams clientRpcParameters = default) {
        GetGameInstance().SetReceivedPlayerRefRpc(reference, player);
    }
    [ClientRpc]
    public void RelayPlayerSpawnPositionClientRpc(Vector3 spawnPoint, ClientRpcParams clientRpcParameters = default) {
        GetGameInstance().SetReceivedPlayerSpawnPointRpc(spawnPoint);
    }


    //Projectile Spawning
    [ServerRpc (RequireOwnership = false)]
    public void UpdateProjectileSpawnRequestServerRpc(ulong senderID, Player.PlayerType playerType, Projectile.ProjectileType projectileType) {
        RelayProjectileSpawnRequestClientRpc(senderID, playerType, projectileType);
    }
    [ClientRpc]
    public void RelayProjectileSpawnRequestClientRpc(ulong senderID, Player.PlayerType playerType, Projectile.ProjectileType projectileType) {
        if (GetGameInstance().GetClientID() == senderID || playerType == Player.PlayerType.NONE)
                return;

        var script = GetGameInstance().GetCurrentLevelScript();
        if (!script) {
            Debug.LogWarning("Received projectile spawn request while level was null");
            return;
        }

        if (script.ReceiveProjectileSpawnRequest(playerType, projectileType)) {
            if (playerType == Player.PlayerType.PLAYER_1)
                GetGameInstance().GetPlayer1Script().ReceivePickupUsageConfirmationRpc();
            else if (playerType == Player.PlayerType.PLAYER_2)
                GetGameInstance().GetPlayer2Script().ReceivePickupUsageConfirmationRpc();
        }
    }


    //Round Timer Update
    [ServerRpc (RequireOwnership = true)]
    public void UpdateRoundTimerServerRpc(ulong senderID, float value) {
        RelayRoundTimerClientRpc(senderID, value);
    }
    [ClientRpc]
    public void RelayRoundTimerClientRpc(ulong senderID, float value) {
        if (senderID == GetGameInstance().GetClientID())
            return;

        GetGameInstance().GetMatchDirector().ReceiveRoundTimerRpc(value);
    }


    //Pickup Deactivation
    [ServerRpc(RequireOwnership = true)]
    public void DeactivatePickupSpawnsServerRpc(ulong senderID) {
        RelayDeactivatePickupSpawnsClientRpc(senderID);
    }
    [ClientRpc]
    public void RelayDeactivatePickupSpawnsClientRpc(ulong senderID) {
        if (senderID == GetGameInstance().GetClientID())
            return;

        var level = GetGameInstance().GetCurrentLevelScript();
        if (level == null)
            return;


        level.DeactivateAllPickups();
    }


    //Pickup Spawning
    [ServerRpc(RequireOwnership = true)]
    public void UpdatePickupSpawnsServerRpc(ulong senderID, int ID, int spawnIndex) {
        RelayPickupSpawnRequestClientRpc(senderID, ID, spawnIndex);
    }
    [ClientRpc]
    public void RelayPickupSpawnRequestClientRpc(ulong senderID, int ID, int spawnIndex) {
        if (senderID == GetGameInstance().GetClientID())
            return;

        var level = GetGameInstance().GetCurrentLevelScript();
        if (level == null)
            return;

        level.ReceivePickupSpawnRequestRpc(ID, spawnIndex);
    }


    //Player 2 Character Selection
    [ServerRpc (RequireOwnership = false)]
    public void UpdatePlayer2SelectionServerRpc(ulong senderID, int index) {

        RelayPlayer2SelectionClientRpc(senderID, index);
    }
    [ClientRpc] 
    public void RelayPlayer2SelectionClientRpc(ulong senderID, int index) {
        if (senderID == GetGameInstance().GetClientID())
            return;

        GetGameInstance().GetCustomizationMenuScript().ReceivePlayer2CharacterIndexRpc(index);
    }


    //Player 2 Color Selection
    [ServerRpc(RequireOwnership = false)]
    public void UpdatePlayer2ColorSelectionServerRpc(ulong senderID, Color color) {
        RelayPlayer2ColorSelectionClientRpc(senderID, color);
    }
    [ClientRpc]
    public void RelayPlayer2ColorSelectionClientRpc(ulong senderID, Color color) {
        if (senderID == GetGameInstance().GetClientID())
            return;

        GetGameInstance().GetCustomizationMenuScript().ReceivePlayer2ColorSelectionRpc(color);
    }


    //Player 2 Ready Check
    [ServerRpc(RequireOwnership = false)]
    public void UpdatePlayer2ReadyCheckServerRpc(ulong senderID, bool ready) {
        RelayPlayer2ReadyCheckClientRpc(senderID, ready);
    }
    [ClientRpc]
    public void RelayPlayer2ReadyCheckClientRpc(ulong senderID, bool ready) {
        if (senderID == GetGameInstance().GetClientID())
            return;

        GetGameInstance().GetCustomizationMenuScript().ReceivePlayer2ReadyCheckRpc(ready);
    }


    //Level Select Preview
    [ServerRpc(RequireOwnership = false)]
    public void UpdateSelectedLevelPreviewServerRpc(ulong senderID, int index) {
        RelaySelectedLevelPreviewClientRpc(senderID, index);
    }
    [ClientRpc]
    public void RelaySelectedLevelPreviewClientRpc(ulong senderID, int index) {
        if (senderID == (ulong)GetGameInstance().GetClientID())
            return;

        GetGameInstance().GetLevelSelectMenuScript().ReceiveSelectedLevelPreviewRpc(index);
    }
    

    //Level Selector Role
    [ClientRpc]
    public void RelayLevelSelectorRoleClientRpc(ClientRpcParams clientRpcParameters = default) {
        GetGameInstance().GetLevelSelectMenuScript().ActivateStartButton();
    }


    //Selected Level
    [ServerRpc(RequireOwnership = false)]
    public void UpdateSelectedLevelIndexServerRpc(ulong senderID, int index) {
        RelaySelectedLevelIndexClientRpc(senderID, index);
    }
    [ClientRpc]
    public void RelaySelectedLevelIndexClientRpc(ulong senderID, int index) {
        if (senderID == (ulong)GetGameInstance().GetClientID())
            return;

        GetGameInstance().GetLevelSelectMenuScript().ReceiveLevelSelectionRpc(index);
    }


    //Input
    [ServerRpc(RequireOwnership = false)]
    public void CalculatePlayer2PositionServerRpc(float input) { 
        GetGameInstance().GetPlayer2Script().ProccessReceivedMovementRpc(input);
    }
    [ServerRpc(RequireOwnership = false)]
    public void CalculatePlayer2RotationServerRpc(float input) {
        GetGameInstance().GetPlayer2Script().ProccessReceivedRotationRpc(input);
    }
}
