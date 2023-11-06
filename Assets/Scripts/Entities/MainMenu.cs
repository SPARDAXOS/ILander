using UnityEditor;
using UnityEngine;

public class MainMenu : MonoBehaviour {

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
