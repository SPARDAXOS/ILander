using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.InputSystem.Controls.AxisControl;

public class CustomizationMenu : MonoBehaviour
{
    //List of available sprites to swap between
    //Func to call and supply the target color and sprite 

    [SerializeField] private Sprite[] skins;
    [SerializeField] private float colorIncreaseStep = 0.1f;
    [SerializeField] private float colorDecreaseStep = 0.1f;

    public Color player1TargetColor = Color.white;
    public Color player2TargetColor = Color.white;

    private Sprite player1TargetSprite = null;
    private Sprite player2TargetSprite = null;

    private int player1SpriteIndex = 0;
    private int player2SpriteIndex = 0;


    private Image player1CustomizationSprite = null;
    private Image player2CustomizationSprite = null;

    private bool initialized = false;

    public void Initialize() {
        if (initialized)
            return;

        SetupReferences();
        initialized = true;
    }

    private void SetupReferences() {
        Transform Player1Customizer = transform.Find("Player1SpriteCustomizer").transform;
        Transform Player2Customizer = transform.Find("Player2SpriteCustomizer").transform;

        if (!Player1Customizer)
            GameInstance.GetInstance().Abort("Failed to get reference to Player1Customizer - CustomizationMenu");
        if (!Player2Customizer)
            GameInstance.GetInstance().Abort("Failed to get reference to Player2Customizer - CustomizationMenu");

        player1CustomizationSprite = Player1Customizer.GetComponent<Image>();
        player2CustomizationSprite = Player2Customizer.GetComponent<Image>();

        if (!player1CustomizationSprite)
            GameInstance.GetInstance().Abort("Failed to get reference to player1CustomizationSprite - CustomizationMenu");
        if (!player2CustomizationSprite)
            GameInstance.GetInstance().Abort("Failed to get reference to player2CustomizationSprite - CustomizationMenu");
    }




    private void UpdatePlayer1Sprite() {
        player1CustomizationSprite.sprite = skins[player1SpriteIndex];
        player1CustomizationSprite.color = player1TargetColor;
    }
    private void UpdatePlayer2Sprite()
    {
        player2CustomizationSprite.sprite = skins[player2SpriteIndex];
        player2CustomizationSprite.color = player2TargetColor;
    }





    public void SwitchSpriteLeft(int playerIndex) {
        if (playerIndex == 1) {
            player1SpriteIndex--;
            if (player1SpriteIndex < 0)
                player1SpriteIndex = skins.Length - 1;
            UpdatePlayer1Sprite();
        }
        else if (playerIndex == 2) {
            player2SpriteIndex--;
            if (player2SpriteIndex < 0)
                player2SpriteIndex = skins.Length - 1;
            UpdatePlayer2Sprite();
        }
        else
            Debug.LogError("Invalid playerIndex sent to SwitchSpriteLeft - CustomizationMenu : index " + playerIndex);
    }
    public void SwitchSpriteRight(int playerIndex) {
        if (playerIndex == 1) {
            player1SpriteIndex++;
            if (player1SpriteIndex == skins.Length)
                player1SpriteIndex = 0;
            UpdatePlayer1Sprite();
        }
        else if (playerIndex == 2) {
            player2SpriteIndex++;
            if (player2SpriteIndex == skins.Length)
                player2SpriteIndex = 0;
            UpdatePlayer2Sprite();
        }
        else
            Debug.LogError("Invalid playerIndex sent to SwitchSpriteRight - CustomizationMenu : index " + playerIndex);
    }

    public void SwitchRedLeft(int playerIndex) {
        if (playerIndex == 1) {
            player1TargetColor.r -= colorDecreaseStep;
            player1TargetColor.g += colorDecreaseStep;
            Clamp(ref player1TargetColor.g, 0.0f, 1.0f);
            player1TargetColor.b += colorDecreaseStep;
            Clamp(ref player1TargetColor.b, 0.0f, 1.0f);

            UpdatePlayer1Sprite();
        }
        else if (playerIndex == 2) {
            player2TargetColor.r -= colorDecreaseStep;
            player2TargetColor.g += colorDecreaseStep;
            Clamp(ref player1TargetColor.b, 0.0f, 1.0f);
            player2TargetColor.b += colorDecreaseStep;
            Clamp(ref player1TargetColor.b, 0.0f, 1.0f);
            UpdatePlayer2Sprite();
        }
        else
            Debug.LogError("Invalid playerIndex sent to SwitchRedLeft - CustomizationMenu : index " + playerIndex);
    }
    public void SwitchRedRight(int playerIndex) {
        if (playerIndex == 1) {
            player1TargetColor.r += colorIncreaseStep;
            player1TargetColor.g -= colorIncreaseStep;
            player1TargetColor.b -= colorIncreaseStep;
            UpdatePlayer1Sprite();
        }
        else if (playerIndex == 2) {
            player2TargetColor.r += colorIncreaseStep;
            player2TargetColor.g -= colorIncreaseStep;
            player2TargetColor.b -= colorIncreaseStep;
            UpdatePlayer2Sprite();
        }
        else
            Debug.LogError("Invalid playerIndex sent to SwitchRedRight - CustomizationMenu : index " + playerIndex);
    }


    private void Clamp(ref float target, float min, float max) {
        if (target > max)
            target = max;
        if (target < min)
            target = min;
    }

    public void StartButton() {
        GameInstance.GetInstance().SetGameState(GameInstance.GameState.PLAYING);
    }
}
