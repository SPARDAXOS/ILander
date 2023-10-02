using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.InputSystem.Controls.AxisControl;
using ILanderUtility;

public class CustomizationMenu : MonoBehaviour
{
    //List of available sprites to swap between
    //Func to call and supply the target color and sprite 
    //Game Instance takes data from here instead
    //Func to reset the data in here to default

    //Make these two into SOs

    public enum CustomizationMenuMode
    {
        NORMAL,
        ONLINE
    }


    [SerializeField] private float colorEditStep = 0.1f;


    private PlayerCharactersBundle playerCharactersBundle;
    private CustomizationMenuMode currentMenuMode = CustomizationMenuMode.NORMAL;

    public Color player1TargetColor = Color.white;
    public Color player2TargetColor = Color.white;


    private TextMeshProUGUI player1SpriteSwitch   = null;
    private TextMeshProUGUI player1RedSwitch      = null;
    private TextMeshProUGUI player1GreenSwitch    = null;
    private TextMeshProUGUI player1BlueSwitch     = null;

    private TextMeshProUGUI player2SpriteSwitch   = null;
    private TextMeshProUGUI player2RedSwitch      = null;
    private TextMeshProUGUI player2GreenSwitch    = null;
    private TextMeshProUGUI player2BlueSwitch     = null;

    private Canvas mainCanvas = null;


    private int playerCharacterIndex1 = 0;
    private int playerCharacterIndex2 = 0;

    private Image player1PortraitSprite      = null;
    private Image player2PortraitSprite      = null;
    private Image player1ShipSprite          = null;
    private Image player2ShipSprite          = null;





    private bool initialized = false;

    public void Initialize() {
        if (initialized)
            return;

        playerCharactersBundle = GameInstance.GetInstance().GetPlayerCharactersBundle();

        SetupReferences();
        ApplyStartState();
        ApplyCurrentMenuMode();
        initialized = true;
    }

    private void SetupReferences() {

        mainCanvas = GetComponent<Canvas>();
        Utility.Validate(mainCanvas, "Failed to get reference to Canvas component - CustomizationMenu", true);

        //Rename these to make it less confusing!
        Transform Player1Customizer = transform.Find("Player1Customizer").transform;
        Transform Player2Customizer = transform.Find("Player2Customizer").transform;

        Utility.Validate(Player1Customizer, "Failed to get reference to Player1Customizer - CustomizationMenu", true);
        Utility.Validate(Player2Customizer, "Failed to get reference to Player2Customizer - CustomizationMenu", true);

        Transform Player1ShipSpriteTransform = Player1Customizer.Find("ShipSprite").transform;
        Transform Player2ShipSpriteTransform = Player2Customizer.Find("ShipSprite").transform;

        Utility.Validate(Player1ShipSpriteTransform, "Failed to get reference to ShipSprite1 - CustomizationMenu", true);
        Utility.Validate(Player2ShipSpriteTransform, "Failed to get reference to ShipSprite2 - CustomizationMenu", true);

        player1ShipSprite = Player1ShipSpriteTransform.GetComponent<Image>();
        player2ShipSprite = Player2ShipSpriteTransform.GetComponent<Image>();

        Utility.Validate(player1ShipSprite, "Failed to get reference to player1CustomizationSprite - CustomizationMenu", true);
        Utility.Validate(player2ShipSprite, "Failed to get reference to player2CustomizationSprite - CustomizationMenu", true);

        //Switches
        //Player1
        Transform SpriteSwitch1 = Player1Customizer.Find("SpriteSwitch");
        Transform RedSwitch1    = Player1Customizer.Find("RedSwitch");
        Transform GreenSwitch1  = Player1Customizer.Find("GreenSwitch");
        Transform BlueSwitch1   = Player1Customizer.Find("BlueSwitch");
        Utility.Validate(SpriteSwitch1, "Failed to get reference to SpriteSwitch1 - CustomizationMenu", true);
        Utility.Validate(RedSwitch1, "Failed to get reference to RedSwitch1 - CustomizationMenu", true);
        Utility.Validate(GreenSwitch1, "Failed to get reference to GreenSwitch1 - CustomizationMenu", true);
        Utility.Validate(BlueSwitch1, "Failed to get reference to BlueSwitch1 - CustomizationMenu", true);

        player1SpriteSwitch = SpriteSwitch1.GetComponent<TextMeshProUGUI>();
        player1RedSwitch    = RedSwitch1.GetComponent<TextMeshProUGUI>();
        player1GreenSwitch  = GreenSwitch1.GetComponent<TextMeshProUGUI>();
        player1BlueSwitch   = BlueSwitch1.GetComponent<TextMeshProUGUI>();
        Utility.Validate(player1SpriteSwitch, "Failed to get reference to player1SpriteSwitch - CustomizationMenu", true);
        Utility.Validate(player1RedSwitch, "Failed to get reference to player1RedSwitch - CustomizationMenu", true);
        Utility.Validate(player1GreenSwitch, "Failed to get reference to player1GreenSwitch - CustomizationMenu", true);
        Utility.Validate(player1BlueSwitch, "Failed to get reference to player1BlueSwitch - CustomizationMenu", true);

        Transform PortaitSprite1 = Player1Customizer.Find("PortraitSprite");
        Utility.Validate(PortaitSprite1, "Failed to get reference to PortaitSprite1 - CustomizationMenu", true);
        player1PortraitSprite = PortaitSprite1.GetComponent<Image>();
        Utility.Validate(player1PortraitSprite, "Failed to get reference to player1PortraitSprite - CustomizationMenu", true);


        //Player2
        Transform SpriteSwitch2 = Player2Customizer.Find("SpriteSwitch");
        Transform RedSwitch2 = Player2Customizer.Find("RedSwitch");
        Transform GreenSwitch2 = Player2Customizer.Find("GreenSwitch");
        Transform BlueSwitch2 = Player2Customizer.Find("BlueSwitch");
        Utility.Validate(SpriteSwitch2, "Failed to get reference to SpriteSwitch2 - CustomizationMenu", true);
        Utility.Validate(RedSwitch2, "Failed to get reference to RedSwitch2 - CustomizationMenu", true);
        Utility.Validate(GreenSwitch2, "Failed to get reference to GreenSwitch2 - CustomizationMenu", true);
        Utility.Validate(BlueSwitch2, "Failed to get reference to BlueSwitch2 - CustomizationMenu", true);

        player2SpriteSwitch = SpriteSwitch2.GetComponent<TextMeshProUGUI>();
        player2RedSwitch = RedSwitch2.GetComponent<TextMeshProUGUI>();
        player2GreenSwitch = GreenSwitch2.GetComponent<TextMeshProUGUI>();
        player2BlueSwitch = BlueSwitch2.GetComponent<TextMeshProUGUI>();
        Utility.Validate(player2SpriteSwitch, "Failed to get reference to player2SpriteSwitch - CustomizationMenu", true);
        Utility.Validate(player2RedSwitch, "Failed to get reference to player2RedSwitch - CustomizationMenu", true);
        Utility.Validate(player2GreenSwitch, "Failed to get reference to player2GreenSwitch - CustomizationMenu", true);
        Utility.Validate(player2BlueSwitch, "Failed to get reference to player2BlueSwitch - CustomizationMenu", true);

        Transform PortaitSprite2 = Player2Customizer.Find("PortraitSprite");
        Utility.Validate(PortaitSprite2, "Failed to get reference to PortaitSprite2 - CustomizationMenu", true);
        player2PortraitSprite = PortaitSprite2.GetComponent<Image>();
        Utility.Validate(player2PortraitSprite, "Failed to get reference to player2PortraitSprite - CustomizationMenu", true);
    }
    public void ApplyStartState() {
        player1TargetColor = Color.white;
        player2TargetColor = Color.white;
        playerCharacterIndex1 = 0;
        playerCharacterIndex2 = 0;

        UpdatePlayer1Skin();
        UpdatePlayer2Skin();
    }




    public void SetRenderCameraTarget(Camera target) {
        mainCanvas.worldCamera = target;
    }


    public void SetCustomizationMenuMode(CustomizationMenuMode mode) {
        currentMenuMode = mode;
        ApplyCurrentMenuMode();
    }
    private void ApplyCurrentMenuMode() {
        if (currentMenuMode == CustomizationMenuMode.NORMAL) {

        }
        else if (currentMenuMode == CustomizationMenuMode.ONLINE) {

        }
    }
    public void SetPlayer2CharacterIndex(int index)
    {
        playerCharacterIndex2 = index;
        UpdatePlayer2Skin();
    }

    private void UpdatePlayer1Skin() {
        player1ShipSprite.sprite = playerCharactersBundle.playerCharacters[playerCharacterIndex1].shipSprite;
        player1PortraitSprite.sprite = playerCharactersBundle.playerCharacters[playerCharacterIndex1].portraitSprite;
        player1SpriteSwitch.text = playerCharactersBundle.playerCharacters[playerCharacterIndex1].name;

        player1ShipSprite.color = player1TargetColor;
        player1RedSwitch.text = "R: " + player1TargetColor.r.ToString("F1");
        player1GreenSwitch.text = "G: " + player1TargetColor.g.ToString("F1");
        player1BlueSwitch.text = "B: " + player1TargetColor.b.ToString("F1");
    }
    private void UpdatePlayer2Skin() {
        player2ShipSprite.sprite = playerCharactersBundle.playerCharacters[playerCharacterIndex2].shipSprite;
        player2PortraitSprite.sprite = playerCharactersBundle.playerCharacters[playerCharacterIndex2].portraitSprite;
        player2SpriteSwitch.text = playerCharactersBundle.playerCharacters[playerCharacterIndex2].name;

        player2ShipSprite.color = player2TargetColor;
        player2RedSwitch.text = "R: " + player2TargetColor.r.ToString("F1");
        player2GreenSwitch.text = "G: " + player2TargetColor.g.ToString("F1");
        player2BlueSwitch.text = "B: " + player2TargetColor.b.ToString("F1");
    }




    
    public void SwitchSpriteLeft(int playerIndex) {
        if (playerIndex == 1) {
            playerCharacterIndex1--;
            if (playerCharacterIndex1 < 0)
                playerCharacterIndex1 = playerCharactersBundle.playerCharacters.Length - 1;
            UpdatePlayer1Skin();
            GameInstance.GetInstance().UpdatePlayer2SelectionIndex(playerCharacterIndex1);
        }
        else if (playerIndex == 2) {
            playerCharacterIndex2--;
            if (playerCharacterIndex2 < 0)
                playerCharacterIndex2 = playerCharactersBundle.playerCharacters.Length - 1;
            UpdatePlayer2Skin();
        }
        else
            Debug.LogError("Invalid playerIndex sent to SwitchSpriteLeft - CustomizationMenu : index " + playerIndex);
    }
    public void SwitchSpriteRight(int playerIndex) {
        if (playerIndex == 1) {
            playerCharacterIndex1++;
            if (playerCharacterIndex1 == playerCharactersBundle.playerCharacters.Length)
                playerCharacterIndex1 = 0;
            UpdatePlayer1Skin();
            GameInstance.GetInstance().UpdatePlayer2SelectionIndex(playerCharacterIndex1);
        }
        else if (playerIndex == 2) {
            playerCharacterIndex2++;
            if (playerCharacterIndex2 == playerCharactersBundle.playerCharacters.Length)
                playerCharacterIndex2 = 0;
            UpdatePlayer2Skin();
        }
        else
            Debug.LogError("Invalid playerIndex sent to SwitchSpriteRight - CustomizationMenu : index " + playerIndex);
    }



    public void Player1ColorSwitchMinus(string colorElement) {
        if (colorElement == "red" || colorElement == "RED" || colorElement == "Red") {
            player1TargetColor.r -= colorEditStep;
            Utility.Clamp(ref player1TargetColor.r, 0.0f, 1.0f);
        }
        else if (colorElement == "green" || colorElement == "GREEN" || colorElement == "Green") {
            player1TargetColor.g -= colorEditStep;
            Utility.Clamp(ref player1TargetColor.g, 0.0f, 1.0f);
        }
        else if (colorElement == "blue" || colorElement == "BLUE" || colorElement == "Blue") {
            player1TargetColor.b -= colorEditStep;
            Utility.Clamp(ref player1TargetColor.b, 0.0f, 1.0f);
        }
        else {
            Debug.LogError("Invalid index sent to Player1ColorSwitchMinus - CustomizationMenu : index " + colorElement + "\n Available indices are 1, 2, 3");
            return;
        }

        UpdatePlayer1Skin();
    }
    public void Player1ColorSwitchPlus(string colorElement) {
        if (colorElement == "red" || colorElement == "RED" || colorElement == "Red") {
            player1TargetColor.r += colorEditStep;
            Utility.Clamp(ref player1TargetColor.r, 0.0f, 1.0f);
        }
        else if (colorElement == "green" || colorElement == "GREEN" || colorElement == "Green") {
            player1TargetColor.g += colorEditStep;
            Utility.Clamp(ref player1TargetColor.g, 0.0f, 1.0f);
        }
        else if (colorElement == "blue" || colorElement == "BLUE" || colorElement == "Blue") {
            player1TargetColor.b += colorEditStep;
            Utility.Clamp(ref player1TargetColor.b, 0.0f, 1.0f);
        }
        else {
            Debug.LogError("Invalid index sent to Player1ColorSwitchPlus - CustomizationMenu : index " + colorElement + "\n Available indices are 1, 2, 3");
            return;
        }

        UpdatePlayer1Skin();
    }

    public void Player2ColorSwitchMinus(string colorElement)
    {
        if (colorElement == "red" || colorElement == "RED" || colorElement == "Red")
        {
            player2TargetColor.r -= colorEditStep;
            Utility.Clamp(ref player2TargetColor.r, 0.0f, 1.0f);
        }
        else if (colorElement == "green" || colorElement == "GREEN" || colorElement == "Green")
        {
            player2TargetColor.g -= colorEditStep;
            Utility.Clamp(ref player2TargetColor.g, 0.0f, 1.0f);
        }
        else if (colorElement == "blue" || colorElement == "BLUE" || colorElement == "Blue")
        {
            player2TargetColor.b -= colorEditStep;
            Utility.Clamp(ref player2TargetColor.b, 0.0f, 1.0f);
        }
        else
        {
            Debug.LogError("Invalid index sent to Player2ColorSwitchMinus - CustomizationMenu : index " + colorElement + "\n Available indices are 1, 2, 3");
            return;
        }

        UpdatePlayer2Skin();
    }
    public void Player2ColorSwitchPlus(string colorElement) {
        if (colorElement == "red" || colorElement == "RED" || colorElement == "Red") {
            player2TargetColor.r += colorEditStep;
            Utility.Clamp(ref player2TargetColor.r, 0.0f, 1.0f);
        }
        else if (colorElement == "green" || colorElement == "GREEN" || colorElement == "Green") {
            player2TargetColor.g += colorEditStep;
            Utility.Clamp(ref player2TargetColor.g, 0.0f, 1.0f);
        }
        else if (colorElement == "blue" || colorElement == "BLUE" || colorElement == "Blue") {
            player2TargetColor.b += colorEditStep;
            Utility.Clamp(ref player2TargetColor.b, 0.0f, 1.0f);
        }
        else {
            Debug.LogError("Invalid index sent to Player2ColorSwitchPlus - CustomizationMenu : index " + colorElement + "\n Available indices are 1, 2, 3");
            return;
        }

        UpdatePlayer2Skin();
    }


    public void StartButton() {
        var instance = GameInstance.GetInstance();
        //Its not just sprite index anymore!

        instance.ConfirmCharacterSelection(Player.PlayerType.PLAYER_1, playerCharactersBundle.playerCharacters[playerCharacterIndex1]);
        instance.ConfirmCharacterSelection(Player.PlayerType.PLAYER_2, playerCharactersBundle.playerCharacters[playerCharacterIndex2]);
        instance.SetGameState(GameInstance.GameState.LEVEL_SELECT_MENU);
    }
}
