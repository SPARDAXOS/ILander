using ILanderUtility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GameInstance;

public class CustomizationMenu : MonoBehaviour
{
    public enum CustomizationMenuMode
    {
        NORMAL,
        ONLINE
    }


    private PlayerCharactersBundle playerCharactersBundle;
    private CustomizationMenuMode currentMenuMode = CustomizationMenuMode.NORMAL;

    private Color player1TargetColor = Color.white;
    private Color player2TargetColor = Color.white;

    private Image player1PortraitSprite = null;
    private Image player2PortraitSprite = null;
    private Image player1ShipSprite     = null;
    private Image player2ShipSprite     = null;

    private TextMeshProUGUI player1SpriteSwitch   = null;
    private GameObject player1SpriteSwitchLeft = null;
    private GameObject player1SpriteSwitchRight = null;

    private TextMeshProUGUI player2SpriteSwitch   = null;
    private GameObject player2SpriteSwitchLeft    = null;
    private GameObject player2SpriteSwitchRight   = null;

    private Button player1ColorPickerButton = null;
    private Button player1SpectrumButton    = null;
    private RectTransform player1SpectrumButtonRectTransform = null;

    private Button player2ColorPickerButton = null;
    private Button player2SpectrumButton    = null;
    private RectTransform player2SpectrumButtonRectTransform = null;

    private GameObject startButtonGameObject = null;
    private GameObject readyButtonGameObject = null;
    private TextMeshProUGUI readyButtonText = null;

    private GameObject player1ReadyCheckGameObject = null;
    private GameObject player2ReadyCheckGameObject = null;

    private Texture2D spectrumSprite = null;


    private Canvas mainCanvas = null;

    private bool spectrumButtonHeld = false;
    private bool spectrumButtonExit = false;
    private int colorPickerTarget = -1;

    private int playerCharacterIndex1 = 0;
    private int playerCharacterIndex2 = 0;


    private bool player1ReadyCheck = false;
    private bool player2ReadyCheck = false;

    private bool initialized = false;



    public void Initialize() {
        if (initialized)
            return;

        playerCharactersBundle = GetInstance().GetPlayerCharactersBundle();
        SetupReferences();
        SetupStartState();
        ApplyCurrentMenuMode();
        initialized = true;
    }
    public void Tick() {
        if (!initialized) {
            Debug.LogError("Attempted to tick uninitialized entity - " + gameObject.name);
            return;
        }

        if (spectrumButtonHeld)
            UpdateColorPickerTarget();
    }

    private void SetupReferences() {

        //Break this into smaller functions!

        mainCanvas = GetComponent<Canvas>();
        Utility.Validate(mainCanvas, "Failed to get reference to Canvas component - CustomizationMenu", true);

        startButtonGameObject = transform.Find("StartButton").gameObject;
        readyButtonGameObject = transform.Find("ReadyButton").gameObject;
        Utility.Validate(startButtonGameObject, "Failed to get reference to StartButton - CustomizationMenu", true);
        Utility.Validate(readyButtonGameObject, "Failed to get reference to ReadyButton - CustomizationMenu", true);


        Transform readyTextTransform = readyButtonGameObject.transform.Find("ReadyText");
        Utility.Validate(readyTextTransform, "Failed to get reference to ReadyText - CustomizationMenu", true);

        readyButtonText = readyTextTransform.GetComponent<TextMeshProUGUI>();
        Utility.Validate(readyButtonText, "Failed to get component TextMeshProUGUI for readyButtonText - CustomizationMenu", true);

        //Rename these to make it less confusing!
        Transform Player1Customizer = transform.Find("Player1Customizer").transform;
        Transform Player2Customizer = transform.Find("Player2Customizer").transform;

        Utility.Validate(Player1Customizer, "Failed to get reference to Player1Customizer - CustomizationMenu", true);
        Utility.Validate(Player2Customizer, "Failed to get reference to Player2Customizer - CustomizationMenu", true);

        Transform Player1ShipSpriteTransform = Player1Customizer.Find("ShipSprite").transform;
        Transform Player2ShipSpriteTransform = Player2Customizer.Find("ShipSprite").transform;
        Utility.Validate(Player1ShipSpriteTransform, "Failed to get reference to ShipSprite1 - CustomizationMenu", true);
        Utility.Validate(Player2ShipSpriteTransform, "Failed to get reference to ShipSprite2 - CustomizationMenu", true);


        //ReadyChecks
        player1ReadyCheckGameObject = Player1Customizer.Find("ReadyCheck").gameObject;
        player2ReadyCheckGameObject = Player2Customizer.Find("ReadyCheck").gameObject;
        Utility.Validate(player1ReadyCheckGameObject, "Failed to get reference to ReadyCheck1 - CustomizationMenu", true);
        Utility.Validate(player2ReadyCheckGameObject, "Failed to get reference to ReadyCheck2 - CustomizationMenu", true);



        //Player1 ColorPicker
        Transform Player1ColorButtonTransform = Player1ShipSpriteTransform.Find("ColorPickerButton");
        Transform Player1SpectrumButtonTransform = Player1ShipSpriteTransform.Find("SpectrumButton");
        Utility.Validate(Player1ColorButtonTransform, "Failed to get reference to ColorPickerButton1 - CustomizationMenu", true);
        Utility.Validate(Player1SpectrumButtonTransform, "Failed to get reference to SpectrumButton1 - CustomizationMenu", true);

        player1ReadyCheckGameObject.SetActive(false);
        player2ReadyCheckGameObject.SetActive(false);

        player1ColorPickerButton = Player1ColorButtonTransform.GetComponent<Button>();
        player1SpectrumButton = Player1SpectrumButtonTransform.GetComponent<Button>();
        Utility.Validate(player1ColorPickerButton, "Failed to get component Button for player1ColorButton - CustomizationMenu", true);
        Utility.Validate(player1SpectrumButton, "Failed to get component Button for player1SpectrumButton - CustomizationMenu", true);

        //Player2 ColorPicker
        Transform Player2ColorButtonTransform = Player2ShipSpriteTransform.Find("ColorPickerButton");
        Transform Player2SpectrumButtonTransform = Player2ShipSpriteTransform.Find("SpectrumButton");
        Utility.Validate(Player2ColorButtonTransform, "Failed to get reference to ColorPickerButton2 - CustomizationMenu", true);
        Utility.Validate(Player2SpectrumButtonTransform, "Failed to get reference to SpectrumButton2 - CustomizationMenu", true);

        player2ColorPickerButton = Player2ColorButtonTransform.GetComponent<Button>();
        player2SpectrumButton = Player2SpectrumButtonTransform.GetComponent<Button>();
        Utility.Validate(player2ColorPickerButton, "Failed to get component Button for player2ColorButton - CustomizationMenu", true);
        Utility.Validate(player2SpectrumButton, "Failed to get component Button for player2SpectrumButton - CustomizationMenu", true);

        player1SpectrumButtonRectTransform = player1SpectrumButton.GetComponent<RectTransform>();
        player2SpectrumButtonRectTransform = player2SpectrumButton.GetComponent<RectTransform>();
        Utility.Validate(player1SpectrumButtonRectTransform, "Failed to get component RectTransform for player1SpectrumButton - CustomizationMenu", true);
        Utility.Validate(player2SpectrumButtonRectTransform, "Failed to get component RectTransform for player2SpectrumButton - CustomizationMenu", true);

        //SpectrumTexture
        Image player1SpectrumButtonImage = Player1SpectrumButtonTransform.GetComponent<Image>();
        Utility.Validate(player1SpectrumButtonImage, "Failed to get component Image for player1SpectrumButtonImage - CustomizationMenu", true);
        player1SpectrumButtonImage.alphaHitTestMinimumThreshold = 0.1f; //In case of circular color spectrum. 

        //SpectrumSprite
        spectrumSprite = player1SpectrumButtonImage.sprite.texture;
        Utility.Validate(spectrumSprite, "Failed to get texture for spectrumSprite - CustomizationMenu", true);

        Image player2SpectrumButtonImage = Player2SpectrumButtonTransform.GetComponent<Image>();
        Utility.Validate(player2SpectrumButtonImage, "Failed to get component Image for player2SpectrumButtonImage - CustomizationMenu", true);
        player2SpectrumButtonImage.alphaHitTestMinimumThreshold = 0.1f; //In case of circular color spectrum. 


        //Color Pickers Initial State
        player1ColorPickerButton.gameObject.SetActive(true);
        player2ColorPickerButton.gameObject.SetActive(true);
        player1SpectrumButton.gameObject.SetActive(false);
        player2SpectrumButton.gameObject.SetActive(false);



        //Sprites
        player1ShipSprite = Player1ShipSpriteTransform.GetComponent<Image>();
        player2ShipSprite = Player2ShipSpriteTransform.GetComponent<Image>();
        Utility.Validate(player1ShipSprite, "Failed to get component Image for player1ShipSprite - CustomizationMenu", true);
        Utility.Validate(player2ShipSprite, "Failed to get component image for player2ShipSprite - CustomizationMenu", true);

        //Switches
        //Player1
        Transform SpriteSwitch1 = Player1Customizer.Find("SpriteSwitch");
        Utility.Validate(SpriteSwitch1, "Failed to get reference to SpriteSwitch1 - CustomizationMenu", true);


        player1SpriteSwitch = SpriteSwitch1.GetComponent<TextMeshProUGUI>();
        Utility.Validate(player1SpriteSwitch, "Failed to get reference to player1SpriteSwitch - CustomizationMenu", true);

        player1SpriteSwitchLeft = SpriteSwitch1.Find("LeftButton").gameObject;
        player1SpriteSwitchRight = SpriteSwitch1.Find("RightButton").gameObject;
        Utility.Validate(player1SpriteSwitchLeft, "Failed to get reference to LeftButton1 - CustomizationMenu", true);
        Utility.Validate(player1SpriteSwitchRight, "Failed to get reference to RightButton1 - CustomizationMenu", true);


        Transform PortaitSprite1 = Player1Customizer.Find("PortraitSprite");
        Utility.Validate(PortaitSprite1, "Failed to get reference to PortaitSprite1 - CustomizationMenu", true);
        player1PortraitSprite = PortaitSprite1.GetComponent<Image>();
        Utility.Validate(player1PortraitSprite, "Failed to get reference to player1PortraitSprite - CustomizationMenu", true);


        //Player2
        Transform SpriteSwitch2 = Player2Customizer.Find("SpriteSwitch");
        Utility.Validate(SpriteSwitch2, "Failed to get reference to SpriteSwitch2 - CustomizationMenu", true);


        player2SpriteSwitch = SpriteSwitch2.GetComponent<TextMeshProUGUI>();
        Utility.Validate(player2SpriteSwitch, "Failed to get reference to player2SpriteSwitch - CustomizationMenu", true);

        player2SpriteSwitchLeft  = SpriteSwitch2.Find("LeftButton").gameObject;
        player2SpriteSwitchRight = SpriteSwitch2.Find("RightButton").gameObject;
        Utility.Validate(player2SpriteSwitchLeft, "Failed to get reference to LeftButton2 - CustomizationMenu", true);
        Utility.Validate(player2SpriteSwitchRight, "Failed to get reference to RightButton2 - CustomizationMenu", true);


        Transform PortaitSprite2 = Player2Customizer.Find("PortraitSprite");
        Utility.Validate(PortaitSprite2, "Failed to get reference to PortaitSprite2 - CustomizationMenu", true);
        player2PortraitSprite = PortaitSprite2.GetComponent<Image>();
        Utility.Validate(player2PortraitSprite, "Failed to get reference to player2PortraitSprite - CustomizationMenu", true);
    }


    public void SetupStartState() {
        player1TargetColor = Color.white;
        player2TargetColor = Color.white;
        playerCharacterIndex1 = 0;
        playerCharacterIndex2 = 0;

        player1ReadyCheckGameObject.SetActive(false);
        player2ReadyCheckGameObject.SetActive(false);

        //They get deactivated by the ready check!
        player1SpriteSwitchLeft.SetActive(true);
        player1SpriteSwitchRight.SetActive(true);
        player1ColorPickerButton.gameObject.SetActive(true);

        readyButtonText.text = "Ready";
        player1ReadyCheck = false;
        player2ReadyCheck = false;

        UpdatePlayer1ShipSprite();
        UpdatePlayer2ShipSprite();
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
            startButtonGameObject.SetActive(true);
            readyButtonGameObject.SetActive(false);

            player2ColorPickerButton.gameObject.SetActive(true);
            player2SpriteSwitchLeft.gameObject.SetActive(true);
            player2SpriteSwitchRight.gameObject.SetActive(true);
        }
        else if (currentMenuMode == CustomizationMenuMode.ONLINE) {
            startButtonGameObject.SetActive(false);
            readyButtonGameObject.SetActive(true);

            player2ColorPickerButton.gameObject.SetActive(false);
            player2SpriteSwitchLeft.gameObject.SetActive(false);
            player2SpriteSwitchRight.gameObject.SetActive(false);
        }
    }
    public void ReceivePlayer2CharacterIndexRpc(int index) {
        playerCharacterIndex2 = index;
        UpdatePlayer2ShipSprite();
    }
    public void ReceivePlayer2ReadyCheckRpc(bool ready) {
        player2ReadyCheck = ready;
        UpdatePlayer2ReadyCheck();
    }
    public void ReceivePlayer2ColorSelectionRpc(Color color) {
        player2TargetColor = color;
        UpdatePlayer2ShipSprite();
    }

    private void CheckPlayersStatus() {
        if (player1ReadyCheck && player2ReadyCheck) {
            var instance = GetInstance();
            instance.SetCharacterSelection(Player.PlayerType.PLAYER_1, playerCharactersBundle.playerCharacters[playerCharacterIndex1], player1TargetColor);
            instance.SetCharacterSelection(Player.PlayerType.PLAYER_2, playerCharactersBundle.playerCharacters[playerCharacterIndex2], player2TargetColor);
            instance.SetGameState(GameState.LEVEL_SELECT_MENU);
            
            
            //BUG: Do something in case the transtion animation breaks. Like set all their positions to the defaults the moment an anim ends!.
        }
    }

    private void UpdatePlayer1ReadyCheck() {
        if (player1ReadyCheck)
            player1ReadyCheckGameObject.SetActive(true);
        else
            player1ReadyCheckGameObject.SetActive(false);

        var rpcManger = GetInstance().GetRpcManagerScript();
        rpcManger.UpdatePlayer2ReadyCheckServerRpc((ulong)GetInstance().GetClientID(), player1ReadyCheck);
        CheckPlayersStatus();
    }
    private void UpdatePlayer2ReadyCheck() {
        if (player2ReadyCheck)
            player2ReadyCheckGameObject.SetActive(true);
        else
            player2ReadyCheckGameObject.SetActive(false);
        CheckPlayersStatus();
    }
    private void UpdatePlayer1ShipSprite() {
        player1ShipSprite.sprite = playerCharactersBundle.playerCharacters[playerCharacterIndex1].shipSprite;
        player1PortraitSprite.sprite = playerCharactersBundle.playerCharacters[playerCharacterIndex1].portraitSprite;
        player1SpriteSwitch.text = playerCharactersBundle.playerCharacters[playerCharacterIndex1].name;

        player1ShipSprite.color = player1TargetColor;
    }
    private void UpdatePlayer2ShipSprite() {
        player2ShipSprite.sprite = playerCharactersBundle.playerCharacters[playerCharacterIndex2].shipSprite;
        player2PortraitSprite.sprite = playerCharactersBundle.playerCharacters[playerCharacterIndex2].portraitSprite;
        player2SpriteSwitch.text = playerCharactersBundle.playerCharacters[playerCharacterIndex2].name;

        player2ShipSprite.color = player2TargetColor;
    }
    private Color CalculatePixelColor(RectTransform rectTransform) {
        if (!rectTransform || spectrumButtonExit)
            return Color.white;

        Vector2 Delta;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, Input.mousePosition, mainCanvas.worldCamera, out Delta);
        float width = rectTransform.rect.width;
        float height = rectTransform.rect.height;
        Delta += new Vector2(width / 2, height / 2);

        float x = Mathf.Clamp(Delta.x / width, 0.0f, 1.0f);
        float y = Mathf.Clamp(Delta.y / height, 0.0f, 1.0f);

        int texX = Mathf.RoundToInt(x * spectrumSprite.width);
        int texY = Mathf.RoundToInt(y * spectrumSprite.height);

        return spectrumSprite.GetPixel(texX, texY);
    }
    private void UpdateColorPickerTarget() {
        if (colorPickerTarget == 1) {
            player1TargetColor = CalculatePixelColor(player1SpectrumButtonRectTransform);
            UpdatePlayer1ShipSprite();
        }
        else if (colorPickerTarget == 2) {
            player2TargetColor = CalculatePixelColor(player2SpectrumButtonRectTransform);
            UpdatePlayer2ShipSprite();
        }
        else
            Debug.LogError("Invalid colorPickerTarget - CustomizationMenu : index " + colorPickerTarget);
    }



    public void SwitchSpriteLeft(int playerIndex) {
        if (playerIndex == 1) {
            playerCharacterIndex1--;
            if (playerCharacterIndex1 < 0)
                playerCharacterIndex1 = playerCharactersBundle.playerCharacters.Length - 1;
            UpdatePlayer1ShipSprite();
            if (currentMenuMode == CustomizationMenuMode.ONLINE) {
                var rpcManger = GetInstance().GetRpcManagerScript();
                rpcManger.UpdatePlayer2SelectionServerRpc((ulong)GetInstance().GetClientID(), playerCharacterIndex1);
            }
        }
        else if (playerIndex == 2) {
            playerCharacterIndex2--;
            if (playerCharacterIndex2 < 0)
                playerCharacterIndex2 = playerCharactersBundle.playerCharacters.Length - 1;
            UpdatePlayer2ShipSprite();
        }
        else
            Debug.LogError("Invalid playerIndex sent to SwitchSpriteLeft - CustomizationMenu : index " + playerIndex);
    }
    public void SwitchSpriteRight(int playerIndex) {
        if (playerIndex == 1) {
            playerCharacterIndex1++;
            if (playerCharacterIndex1 == playerCharactersBundle.playerCharacters.Length)
                playerCharacterIndex1 = 0;
            UpdatePlayer1ShipSprite();
            if (currentMenuMode == CustomizationMenuMode.ONLINE) {
                var rpcManger = GetInstance().GetRpcManagerScript();
                rpcManger.UpdatePlayer2SelectionServerRpc((ulong)GetInstance().GetClientID(), playerCharacterIndex1);
            }
        }
        else if (playerIndex == 2) {
            playerCharacterIndex2++;
            if (playerCharacterIndex2 == playerCharactersBundle.playerCharacters.Length)
                playerCharacterIndex2 = 0;
            UpdatePlayer2ShipSprite();
        }
        else
            Debug.LogError("Invalid playerIndex sent to SwitchSpriteRight - CustomizationMenu : index " + playerIndex);
    }



    public void StartButton() {
        var instance = GetInstance();
        instance.SetCharacterSelection(Player.PlayerType.PLAYER_1, playerCharactersBundle.playerCharacters[playerCharacterIndex1], player1TargetColor);
        instance.SetCharacterSelection(Player.PlayerType.PLAYER_2, playerCharactersBundle.playerCharacters[playerCharacterIndex2], player2TargetColor);
        instance.SetGameState(GameState.LEVEL_SELECT_MENU);
    }
    public void ColorPickerButton(int playerIndex) {
        if (playerIndex == 1) {
            player1ColorPickerButton.gameObject.SetActive(false);
            player1SpriteSwitch.gameObject.SetActive(false);
            player1SpectrumButton.gameObject.SetActive(true);
        }
        else if (playerIndex == 2) {
            player2ColorPickerButton.gameObject.SetActive(false);
            player2SpriteSwitch.gameObject.SetActive(false);
            player2SpectrumButton.gameObject.SetActive(true);
        }
        else
            Debug.LogError("Invalid playerIndex sent to ColorPickerButton - CustomizationMenu : index " + playerIndex);
    }

    public void SpectrumButtonDown(int playerIndex) {
        if (playerIndex != 1 && playerIndex != 2)
            Debug.LogError("Invalid playerIndex sent to SpectrumButtonDown - CustomizationMenu : index " + playerIndex);

        colorPickerTarget = playerIndex;
        spectrumButtonHeld = true;
    }
    public void SpectrumButtonUp(int playerIndex) {
        if (playerIndex == 1) {
            player1ColorPickerButton.gameObject.SetActive(true);
            player1SpriteSwitch.gameObject.SetActive(true);
            player1SpectrumButton.gameObject.SetActive(false);
        } 
        else if (playerIndex == 2) {
            player2ColorPickerButton.gameObject.SetActive(true);
            player2SpriteSwitch.gameObject.SetActive(true);
            player2SpectrumButton.gameObject.SetActive(false);
        } 
        else
            Debug.LogError("Invalid playerIndex sent to SpectrumButtonUp - CustomizationMenu : index " + playerIndex);

        colorPickerTarget = -1;
        spectrumButtonHeld = false;
        spectrumButtonExit = false;

        if (currentMenuMode == CustomizationMenuMode.ONLINE)
            GetInstance().GetRpcManagerScript().UpdatePlayer2ColorSelectionServerRpc(GetInstance().GetClientID(), player1TargetColor);

    }
    public void SpectrumButtonExit() {
        if (spectrumButtonHeld)
            spectrumButtonExit = true;
    }
    public void SpectrumButtonEnter() {
        if (spectrumButtonHeld)
            spectrumButtonExit = false;
    }


    public void ToggleReadyCheck() {
        player1ReadyCheck ^= true;
        player1ColorPickerButton.gameObject.SetActive(!player1ReadyCheck);
        player1SpriteSwitchLeft.SetActive(!player1ReadyCheck);
        player1SpriteSwitchRight.SetActive(!player1ReadyCheck);

        if (player1ReadyCheck)
            readyButtonText.text = "Unready";
        else
            readyButtonText.text = "Ready";


        UpdatePlayer1ReadyCheck();
    }
}
