using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.InputSystem.Controls.AxisControl;

public class CustomizationMenu : MonoBehaviour
{
    //List of available sprites to swap between
    //Func to call and supply the target color and sprite 
    //Game Instance takes data from here instead
    //Func to reset the data in here to default

    //Make these two into SOs
    [SerializeField] private PlayerSkinsBundle playerSkins;
    [SerializeField] private float colorEditStep = 0.1f;

    public Color player1TargetColor = Color.white;
    public Color player2TargetColor = Color.white;

    private Sprite player1TargetSprite = null;
    private Sprite player2TargetSprite = null;

    private TextMeshProUGUI player1SpriteSwitch   = null;
    private TextMeshProUGUI player1RedSwitch      = null;
    private TextMeshProUGUI player1GreenSwitch    = null;
    private TextMeshProUGUI player1BlueSwitch     = null;

    private TextMeshProUGUI player2SpriteSwitch   = null;
    private TextMeshProUGUI player2RedSwitch      = null;
    private TextMeshProUGUI player2GreenSwitch    = null;
    private TextMeshProUGUI player2BlueSwitch     = null;

    private Canvas mainCanvas = null;


    private int player1SpriteIndex = 0;
    private int player2SpriteIndex = 0;

    private int player1ColorIndex = 0;
    private int player2ColorIndex = 0;

    private Image player1PortraitSprite      = null;
    private Image player2PortraitSprite      = null;
    private Image player1CustomizationSprite = null;
    private Image player2CustomizationSprite = null;





    private bool initialized = false;

    public void Initialize() {
        if (initialized)
            return;

        SetupReferences();
        SetupStartState();
        initialized = true;
    }

    private void SetupReferences() {

        mainCanvas = GetComponent<Canvas>();
        GameInstance.Validate(mainCanvas, "Failed to get reference to Canvas component - CustomizationMenu");

        //Rename these to make it less confusing!
        Transform Player1Customizer = transform.Find("Player1Customizer").transform;
        Transform Player2Customizer = transform.Find("Player2Customizer").transform;

        GameInstance.Validate(Player1Customizer, "Failed to get reference to Player1Customizer - CustomizationMenu");
        GameInstance.Validate(Player2Customizer, "Failed to get reference to Player2Customizer - CustomizationMenu");

        player1CustomizationSprite = Player1Customizer.GetComponent<Image>();
        player2CustomizationSprite = Player2Customizer.GetComponent<Image>();

        GameInstance.Validate(player1CustomizationSprite, "Failed to get reference to player1CustomizationSprite - CustomizationMenu");
        GameInstance.Validate(player2CustomizationSprite, "Failed to get reference to player2CustomizationSprite - CustomizationMenu");

        //Switches
        //Player1
        Transform SpriteSwitch1 = Player1Customizer.Find("SpriteSwitch");
        Transform RedSwitch1    = Player1Customizer.Find("RedSwitch");
        Transform GreenSwitch1  = Player1Customizer.Find("GreenSwitch");
        Transform BlueSwitch1   = Player1Customizer.Find("BlueSwitch");
        GameInstance.Validate(SpriteSwitch1, "Failed to get reference to SpriteSwitch1 - CustomizationMenu");
        GameInstance.Validate(RedSwitch1, "Failed to get reference to RedSwitch1 - CustomizationMenu");
        GameInstance.Validate(GreenSwitch1, "Failed to get reference to GreenSwitch1 - CustomizationMenu");
        GameInstance.Validate(BlueSwitch1, "Failed to get reference to BlueSwitch1 - CustomizationMenu");

        player1SpriteSwitch = SpriteSwitch1.GetComponent<TextMeshProUGUI>();
        player1RedSwitch    = RedSwitch1.GetComponent<TextMeshProUGUI>();
        player1GreenSwitch  = GreenSwitch1.GetComponent<TextMeshProUGUI>();
        player1BlueSwitch   = BlueSwitch1.GetComponent<TextMeshProUGUI>();
        GameInstance.Validate(player1SpriteSwitch, "Failed to get reference to player1SpriteSwitch - CustomizationMenu");
        GameInstance.Validate(player1RedSwitch, "Failed to get reference to player1RedSwitch - CustomizationMenu");
        GameInstance.Validate(player1GreenSwitch, "Failed to get reference to player1GreenSwitch - CustomizationMenu");
        GameInstance.Validate(player1BlueSwitch, "Failed to get reference to player1BlueSwitch - CustomizationMenu");

        Transform PortaitSprite1 = Player1Customizer.Find("PortraitSprite");
        GameInstance.Validate(PortaitSprite1, "Failed to get reference to PortaitSprite1 - CustomizationMenu");
        player1PortraitSprite = PortaitSprite1.GetComponent<Image>();
        GameInstance.Validate(player1PortraitSprite, "Failed to get reference to player1PortraitSprite - CustomizationMenu");


        //Player2
        Transform SpriteSwitch2 = Player2Customizer.Find("SpriteSwitch");
        Transform RedSwitch2 = Player2Customizer.Find("RedSwitch");
        Transform GreenSwitch2 = Player2Customizer.Find("GreenSwitch");
        Transform BlueSwitch2 = Player2Customizer.Find("BlueSwitch");
        GameInstance.Validate(SpriteSwitch2, "Failed to get reference to SpriteSwitch2 - CustomizationMenu");
        GameInstance.Validate(RedSwitch2, "Failed to get reference to RedSwitch2 - CustomizationMenu");
        GameInstance.Validate(GreenSwitch2, "Failed to get reference to GreenSwitch2 - CustomizationMenu");
        GameInstance.Validate(BlueSwitch2, "Failed to get reference to BlueSwitch2 - CustomizationMenu");

        player2SpriteSwitch = SpriteSwitch2.GetComponent<TextMeshProUGUI>();
        player2RedSwitch = RedSwitch2.GetComponent<TextMeshProUGUI>();
        player2GreenSwitch = GreenSwitch2.GetComponent<TextMeshProUGUI>();
        player2BlueSwitch = BlueSwitch2.GetComponent<TextMeshProUGUI>();
        GameInstance.Validate(player2SpriteSwitch, "Failed to get reference to player2SpriteSwitch - CustomizationMenu");
        GameInstance.Validate(player2RedSwitch, "Failed to get reference to player2RedSwitch - CustomizationMenu");
        GameInstance.Validate(player2GreenSwitch, "Failed to get reference to player2GreenSwitch - CustomizationMenu");
        GameInstance.Validate(player2BlueSwitch, "Failed to get reference to player2BlueSwitch - CustomizationMenu");

        Transform PortaitSprite2 = Player2Customizer.Find("PortraitSprite");
        GameInstance.Validate(PortaitSprite2, "Failed to get reference to PortaitSprite2 - CustomizationMenu");
        player2PortraitSprite = PortaitSprite2.GetComponent<Image>();
        GameInstance.Validate(player2PortraitSprite, "Failed to get reference to player2PortraitSprite - CustomizationMenu");
    }
    public void SetupStartState() {
        player1TargetColor = Color.white;
        player2TargetColor = Color.white;
        player1SpriteIndex = 0;
        player2SpriteIndex = 0;

        UpdatePlayer1Skin();
        UpdatePlayer2Skin();
    }


    //IMPORTANT NOTE: Separate the ship sprite actor and rework stuff.
    //Then maybe set the size of it to the size of the sprite that i set to it.
    //Of course after making it smaller!

    public void SetRenderCameraTarget(Camera target) {
        mainCanvas.worldCamera = target;
    }



    private void UpdatePlayer1Skin() {
        player1CustomizationSprite.sprite = playerSkins.skins[player1SpriteIndex].shipSprite;
        player1PortraitSprite.sprite = playerSkins.skins[player1SpriteIndex].portraitSprite;
        player1SpriteSwitch.text = playerSkins.skins[player1SpriteIndex].name;

        player1CustomizationSprite.color = player1TargetColor;
        player1RedSwitch.text = "R: " + player1TargetColor.r.ToString("F1");
        player1GreenSwitch.text = "G: " + player1TargetColor.g.ToString("F1");
        player1BlueSwitch.text = "B: " + player1TargetColor.b.ToString("F1");
    }
    private void UpdatePlayer2Skin() {
        player2CustomizationSprite.sprite = playerSkins.skins[player2SpriteIndex].shipSprite;
        player2PortraitSprite.sprite = playerSkins.skins[player2SpriteIndex].portraitSprite;
        player2SpriteSwitch.text = playerSkins.skins[player2SpriteIndex].name;

        player2CustomizationSprite.color = player2TargetColor;
        player2RedSwitch.text = "R: " + player2TargetColor.r.ToString("F1");
        player2GreenSwitch.text = "G: " + player2TargetColor.g.ToString("F1");
        player2BlueSwitch.text = "B: " + player2TargetColor.b.ToString("F1");
    }




    
    public void SwitchSpriteLeft(int playerIndex) {
        if (playerIndex == 1) {
            player1SpriteIndex--;
            if (player1SpriteIndex < 0)
                player1SpriteIndex = playerSkins.skins.Length - 1;
            UpdatePlayer1Skin();
        }
        else if (playerIndex == 2) {
            player2SpriteIndex--;
            if (player2SpriteIndex < 0)
                player2SpriteIndex = playerSkins.skins.Length - 1;
            UpdatePlayer2Skin();
        }
        else
            Debug.LogError("Invalid playerIndex sent to SwitchSpriteLeft - CustomizationMenu : index " + playerIndex);
    }
    public void SwitchSpriteRight(int playerIndex) {
        if (playerIndex == 1) {
            player1SpriteIndex++;
            if (player1SpriteIndex == playerSkins.skins.Length)
                player1SpriteIndex = 0;
            UpdatePlayer1Skin();
        }
        else if (playerIndex == 2) {
            player2SpriteIndex++;
            if (player2SpriteIndex == playerSkins.skins.Length)
                player2SpriteIndex = 0;
            UpdatePlayer2Skin();
        }
        else
            Debug.LogError("Invalid playerIndex sent to SwitchSpriteRight - CustomizationMenu : index " + playerIndex);
    }



    public void Player1ColorSwitchMinus(string colorElement) {
        if (colorElement == "red" || colorElement == "RED" || colorElement == "Red") {
            player1TargetColor.r -= colorEditStep;
            GameInstance.Clamp(ref player1TargetColor.r, 0.0f, 1.0f);
        }
        else if (colorElement == "green" || colorElement == "GREEN" || colorElement == "Green") {
            player1TargetColor.g -= colorEditStep;
            GameInstance.Clamp(ref player1TargetColor.g, 0.0f, 1.0f);
        }
        else if (colorElement == "blue" || colorElement == "BLUE" || colorElement == "Blue") {
            player1TargetColor.b -= colorEditStep;
            GameInstance.Clamp(ref player1TargetColor.b, 0.0f, 1.0f);
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
            GameInstance.Clamp(ref player1TargetColor.r, 0.0f, 1.0f);
        }
        else if (colorElement == "green" || colorElement == "GREEN" || colorElement == "Green") {
            player1TargetColor.g += colorEditStep;
            GameInstance.Clamp(ref player1TargetColor.g, 0.0f, 1.0f);
        }
        else if (colorElement == "blue" || colorElement == "BLUE" || colorElement == "Blue") {
            player1TargetColor.b += colorEditStep;
            GameInstance.Clamp(ref player1TargetColor.b, 0.0f, 1.0f);
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
            GameInstance.Clamp(ref player2TargetColor.r, 0.0f, 1.0f);
        }
        else if (colorElement == "green" || colorElement == "GREEN" || colorElement == "Green")
        {
            player2TargetColor.g -= colorEditStep;
            GameInstance.Clamp(ref player2TargetColor.g, 0.0f, 1.0f);
        }
        else if (colorElement == "blue" || colorElement == "BLUE" || colorElement == "Blue")
        {
            player2TargetColor.b -= colorEditStep;
            GameInstance.Clamp(ref player2TargetColor.b, 0.0f, 1.0f);
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
            GameInstance.Clamp(ref player2TargetColor.r, 0.0f, 1.0f);
        }
        else if (colorElement == "green" || colorElement == "GREEN" || colorElement == "Green") {
            player2TargetColor.g += colorEditStep;
            GameInstance.Clamp(ref player2TargetColor.g, 0.0f, 1.0f);
        }
        else if (colorElement == "blue" || colorElement == "BLUE" || colorElement == "Blue") {
            player2TargetColor.b += colorEditStep;
            GameInstance.Clamp(ref player2TargetColor.b, 0.0f, 1.0f);
        }
        else {
            Debug.LogError("Invalid index sent to Player2ColorSwitchPlus - CustomizationMenu : index " + colorElement + "\n Available indices are 1, 2, 3");
            return;
        }

        UpdatePlayer2Skin();
    }


    public void StartButton() {
        GameInstance.GetInstance().SetGameState(GameInstance.GameState.PLAYING);
    }





}
