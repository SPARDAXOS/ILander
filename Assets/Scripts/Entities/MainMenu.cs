using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MainMenu : MonoBehaviour
{

    //NOTE: Probably need to overarching menu manager or just use the game instance to set everything up

    private bool initialized = false;

    //Probably just delete this! or keep it for API consistency
    public void Initialize() {
        if (initialized)
            return;


        initialized = true;
    }






    public void StartButton() {
        GameInstance.GetGameInstance().SetGameState(GameInstance.GameState.GAMEMODE_MENU);
    }
    public void SettingsButton() {
        GameInstance.GetGameInstance().SetGameState(GameInstance.GameState.SETTINGS_MENU);
    }
    public void QuitButton() {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
