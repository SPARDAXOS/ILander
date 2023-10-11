using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameInstance;

public class PauseMenu : MonoBehaviour
{
    public void ResumeButton() {
        GetGameInstance().UnpauseGame();
    }
    public void QuitButton() {
        Debug.Log("GAME QUIT! - through Quit button!");
        GetGameInstance().QuitMatch();
    }
}
