using Initialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameModeMenu : MonoBehaviour
{
    public void CoopModeButton() {
        var instance = GameInstance.GetGameInstance();
        instance.SetGameModeSelection(GameInstance.GameMode.COOP);
        instance.SetGameState(GameInstance.GameState.CUSTOMIZATION_MENU);
    }
    public void LanModeButton() {
        var instance = GameInstance.GetGameInstance();
        instance.SetGameModeSelection(GameInstance.GameMode.LAN);
        instance.SetGameState(GameInstance.GameState.CONNECTION_MENU);
    }
}
