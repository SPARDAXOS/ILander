using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameInstance;

public class PauseMenu : MonoBehaviour
{
    public void ResumeButton() {
        GetInstance().UnpauseGame();
    }
    public void QuitButton() {
        GetInstance().QuitMatch();
    }
}
