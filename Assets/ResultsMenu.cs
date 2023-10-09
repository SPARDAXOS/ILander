using UnityEngine;
using ILanderUtility;
using static GameInstance;
using TMPro;
using UnityEngine.UI;

public class ResultsMenu : MonoBehaviour
{
    private bool initialized = false;


    private float returnTimer = 0.0f;


    private Image player1PortraitSprite = null;
    private Image player2PortraitSprite = null;

    private TextMeshProUGUI player1ResultsText = null;
    private TextMeshProUGUI player2ResultsText = null;

    private TextMeshProUGUI notificationText = null;

    private Canvas mainCanvas = null;

    //I could do an animation where the text is massive and it closes in to land on characters! then gets their color!


    public void Initialize() {
        if (initialized)
            return;

        SetupReferences();
        initialized = true;
    }
    public void Tick() {
        if (!initialized) {
            Debug.LogWarning("Attempted to tick uninitialized entity " + gameObject.name);
            return;
        }


        UpdateReturnTimer();
    }
    private void SetupReferences() {

        mainCanvas = GetComponent<Canvas>();
        Utility.Validate(mainCanvas, "Failed to get reference to component mainCanvas - ResultsMenu", true);

        Transform notificationTextTransform = transform.Find("NotificationText");
        Utility.Validate(notificationTextTransform, "Failed to get reference to NotificationText - ResultsMenu", true);

        notificationText = notificationTextTransform.GetComponent<TextMeshProUGUI>();
        Utility.Validate(notificationText, "Failed to get reference to component TextMeshProUGUI in notificationTextTransform - ResultsMenu", true);


        Transform player1ResultsTransform = transform.Find("Player1Results");
        Transform player2ResultsTransform = transform.Find("Player2Results");
        Utility.Validate(player1ResultsTransform, "Failed to get reference to Player1Results - ResultsMenu", true);
        Utility.Validate(player2ResultsTransform, "Failed to get reference to Player2Results - ResultsMenu", true);

        Transform player1PortraitSpriteTransform = player1ResultsTransform.Find("PortraitSprite");
        Transform player2PortraitSpriteTransform = player2ResultsTransform.Find("PortraitSprite");
        Utility.Validate(player1PortraitSpriteTransform, "Failed to get reference to PortraitSprite1 - ResultsMenu", true);
        Utility.Validate(player2PortraitSpriteTransform, "Failed to get reference to PortraitSprite2 - ResultsMenu", true);

        player1PortraitSprite = player1PortraitSpriteTransform.GetComponent<Image>();
        player2PortraitSprite = player2PortraitSpriteTransform.GetComponent<Image>();
        Utility.Validate(player1PortraitSprite, "Failed to get reference to component Image in player1PortraitSpriteTransform - ResultsMenu", true);
        Utility.Validate(player2PortraitSprite, "Failed to get reference to component Image in player2PortraitSpriteTransform - ResultsMenu", true);

        Transform ResultsTransform1 = player1ResultsTransform.Find("Results");
        Transform ResultsTransform2 = player2ResultsTransform.Find("Results");
        Utility.Validate(ResultsTransform1, "Failed to get reference to Results1 - ResultsMenu", true);
        Utility.Validate(ResultsTransform2, "Failed to get reference to Results2 - ResultsMenu", true);

        Transform ResultsTextTransform1 = ResultsTransform1.Find("ResultsText");
        Transform ResultsTextTransform2 = ResultsTransform2.Find("ResultsText");
        Utility.Validate(ResultsTextTransform1, "Failed to get reference to ResultsText1 - ResultsMenu", true);
        Utility.Validate(ResultsTextTransform2, "Failed to get reference to ResultsText2 - ResultsMenu", true);

        player1ResultsText = ResultsTextTransform1.GetComponent<TextMeshProUGUI>();
        player2ResultsText = ResultsTextTransform2.GetComponent<TextMeshProUGUI>();
        Utility.Validate(player1ResultsText, "Failed to get reference to component TextMeshProUGUI in player1ResultsText - ResultsMenu", true);
        Utility.Validate(player2ResultsText, "Failed to get reference to component TextMeshProUGUI in player2ResultsText - ResultsMenu", true);
    }
    public void SetRenderCameraTarget(Camera target) {
        mainCanvas.worldCamera = target;
    }


    //Use match results instead of just player type since i need draw !
    public void SetWinner(Player.PlayerType type) {
        //^^
        if (type == Player.PlayerType.NONE) {
            player1ResultsText.text = "Draw";
            player2ResultsText.text = "Draw";
            player1ResultsText.color = Color.yellow;
            player2ResultsText.color = Color.yellow;
        }

        if (type == Player.PlayerType.PLAYER_1) {
            player1ResultsText.text = "Winner";
            player2ResultsText.text = "Loser";
            player1ResultsText.color = Color.green;
            player2ResultsText.color = Color.red;
        }
        else if (type == Player.PlayerType.PLAYER_2) {
            player1ResultsText.text = "Loser";
            player2ResultsText.text = "Winner";
            player1ResultsText.color = Color.red;
            player2ResultsText.color = Color.green;
        }
    }
    public void SetPlayerPortrait(Player.PlayerType type, Sprite portrait) {
        if (type == Player.PlayerType.NONE)
            return;

        if (type == Player.PlayerType.PLAYER_1)
            player1PortraitSprite.sprite = portrait;
        else if (type == Player.PlayerType.PLAYER_2)
            player2PortraitSprite.sprite = portrait;
    }

    public void StartReturnTimer(float duration) {
        returnTimer = duration;
        notificationText.text = "Returning to main menu in " + (int)returnTimer + " ..";
    }
    private void UpdateReturnTimer() {
        if (returnTimer > 0.0f) {
            returnTimer -= Time.deltaTime;
            notificationText.text = "Returning to main menu in " + (int)returnTimer + " ..";
            if (returnTimer <= 0.0f) {
                returnTimer = 0.0f;
                GetInstance().SetGameState(GameState.MAIN_MENU); //Questionable! calls special func cause some stuff needs to happen! THIS! Destroy players if offline, other wise do network stuff!
            }
        }
    }
}
