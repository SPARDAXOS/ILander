using ILanderUtility;
using TMPro;
using UnityEngine;
using static GameInstance;

public class MatchDirector : MonoBehaviour
{
    [Range(1, 10)]
    [SerializeField] uint pointsToWin = 2;

    [Tooltip("Round time limit in seconds")]
    [Range(1, 1000)]
    [SerializeField] uint roundTimeLimit = 2;

    private bool initialized = false;
    public bool matchStarted = false;

    public uint player1Score = 0;
    public uint player2Score = 0;
    public Player.PlayerType winner = Player.PlayerType.NONE;

    public float roundTimer = 0.0f;

    private TextMeshProUGUI roundTimerText = null;
    private TextMeshProUGUI player1ScoreText = null;
    private TextMeshProUGUI player2ScoreText = null;

    private Animation animationComp = null;

    public void Initialize() {
        if (initialized)
            return;


        SetupReferences();
        initialized = true;
    }
    public void Tick() {
        if (!initialized) {
            Debug.LogWarning("Attempted to tick an uninitialized entity " + gameObject.name);
            return;
        }
        if (!matchStarted)
            return;


        UpdateRoundTimer();
    }
    private void SetupReferences() {

        animationComp = GetComponent<Animation>();
        Utility.Validate(animationComp, "Failed to get component Animation - MatchDirector", true);

        Transform HUDTransform = transform.Find("HUD");
        Utility.Validate(HUDTransform, "Failed to get reference to HUD - MatchDirector", true);

        Transform roundTimerTransform = HUDTransform.Find("RoundTimer");
        Utility.Validate(roundTimerTransform, "Failed to get reference to RoundTimer - MatchDirector", true);

        roundTimerText = roundTimerTransform.GetComponent<TextMeshProUGUI>();
        Utility.Validate(roundTimerText, "Failed to get component TextMeshProUGUI in RoundTimer - MatchDirector", true);

        var ScoreTransform = HUDTransform.Find("Score");
        Utility.Validate(ScoreTransform, "Failed to get reference to Score - MatchDirector", true);

        var player1ScoreTextTransform = ScoreTransform.Find("Player1Score");
        var player2ScoreTextTransform = ScoreTransform.Find("Player2Score");
        Utility.Validate(player1ScoreTextTransform, "Failed to get reference to Player1Score - MatchDirector", true);
        Utility.Validate(player2ScoreTextTransform, "Failed to get reference to Player2Score - MatchDirector", true);

        player1ScoreText = player1ScoreTextTransform.GetComponent<TextMeshProUGUI>();
        player2ScoreText = player2ScoreTextTransform.GetComponent<TextMeshProUGUI>();
        Utility.Validate(player1ScoreText, "Failed to get component TextMeshProUGUI in player1ScoreText - MatchDirector", true);
        Utility.Validate(player2ScoreText, "Failed to get component TextMeshProUGUI in player2ScoreText - MatchDirector", true);
    }


    //This class will start the match when the game instance wants to? and WILL end it on its own.
    //This func is meant for restarting the timer
    public void StartRoundTimer() {
        roundTimer = roundTimeLimit;
        UpdateRoundTimerText();
    }
    private void UpdateRoundTimer() {
        if (roundTimer > 0.0f) {
            roundTimer -= Time.deltaTime;
            UpdateRoundTimerText();
            if (roundTimer < 0.0f) {
                roundTimer = 0.0f;
                Timeout();
            }
        }
        else
            Timeout();
    }
    private void Timeout() {
        var player1 = GetInstance().GetPlayer1Script();
        var player2 = GetInstance().GetPlayer2Script();

        if (player1.GetCurrentHealth() > player2.GetCurrentHealth())
            ScorePoint(Player.PlayerType.PLAYER_1);
        else if (player1.GetCurrentHealth() < player2.GetCurrentHealth())
            ScorePoint(Player.PlayerType.PLAYER_2);

        //Or draw!
    }
    private void UpdateRoundTimerText() {
        roundTimerText.text = ((int)roundTimer).ToString();
    }





    //Score Text anim
    public void StartScoreUpdate() {
        if (animationComp.isPlaying)
            return;

        animationComp.Play("ScoreUpdate");
    }
    public void ScoreUpdateRefresh() {
        player1ScoreText.text = player1Score.ToString();
        player2ScoreText.text = player2Score.ToString();
    }
    public void ScoreUpdateOver() {
        if (IsWinnerDecided())
            EndMatch();
        else
            StartNewRound();
    }



    //StartMatch is to be called by game instance after countdown
    //EndMatch will be called by director when match is over and it will call the game instance to end match
    //QutMatch is to be called by game instance in case of disconnection or quitting.

    private void StartNewRound() {
        //Redo the whole process again? 
        //Players invisible
        //Players input off
        //Reset player data
        //Reset players to initial spawn pos
        //Trigger countdown
        //Reenable stuff on countdown callback!

        //SetRoundTimerVisibility(false); //here?
        GetInstance().StartNewRound();
        StartRoundTimer();
    }
    public void StartMatch() {
        ResetMatchData();
        matchStarted = true;
        StartRoundTimer();
        SetRoundTimerVisibility(true);
    }
    public void EndMatch() {
        SetRoundTimerVisibility(false);
        matchStarted = false;
        GetInstance().EndMatch();
    }
    public void QuitMatch() {

    }

    public void SetRoundTimerVisibility(bool state) {
        roundTimerText.gameObject.SetActive(state);
    }


    private void ResetMatchData() {
        player1Score = 0;
        player2Score = 0;
        ScoreUpdateRefresh(); //Per match reset

        winner = Player.PlayerType.NONE;
        roundTimer = 0.0f;
    }



    public void ScorePoint(Player.PlayerType type) {
        if (type == Player.PlayerType.NONE || winner != Player.PlayerType.NONE)
            return;

        if (type == Player.PlayerType.PLAYER_1) {
            player1Score++;
            if (player1Score == pointsToWin)
                winner = Player.PlayerType.PLAYER_1;
        }
        else if (type == Player.PlayerType.PLAYER_2) {
            player2Score++;
            if (player2Score == pointsToWin)
                winner = Player.PlayerType.PLAYER_2;
        }

        StartScoreUpdate(); //Triggers score update at each score point!
    }
    private bool IsWinnerDecided() {
        if (winner != Player.PlayerType.NONE)
            return true;
        return false;
    }
    public Player.PlayerType GetWinner() {
        return winner;
    }
    public bool HasMatchStarted() {
        return matchStarted;
    }

}
