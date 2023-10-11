using UnityEngine;
using static GameInstance;

public class PauseMenu : MonoBehaviour {
    public void ResumeButton() {
        GetGameInstance().UnpauseGame();
    }
    public void QuitButton() {
        GetGameInstance().QuitMatch();
    }
}
