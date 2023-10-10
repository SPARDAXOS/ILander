using ILanderUtility;
using TMPro;
using UnityEngine;
using static GameInstance;

public class MatchDirector : MonoBehaviour
{
    public enum MatchResults {
        NONE,
        DRAW,
        PLAYER_1_WINS,
        PLAYER_2_WINS
    }

    [Range(1, 10)]
    [SerializeField] uint pointsToWin = 2;

    [Tooltip("Round time limit in seconds")]
    [Range(1, 1000)]
    [SerializeField] uint roundTimeLimit = 2;

    private bool initialized = false;
    public bool matchStarted = false;
    private bool isRoundTimerRunning = false;

    public uint player1Score = 0;
    public uint player2Score = 0;
    public MatchResults matchResults = MatchResults.NONE;

    public float roundTimer = 0.0f;

    private TextMeshProUGUI roundTimerText = null;
    private TextMeshProUGUI player1ScoreText = null;
    private TextMeshProUGUI player2ScoreText = null;

    private Animation animationComp = null;

    public void Initialize() {
        if (initialized)
            return;


        SetupReferences();
        SetRoundTimerState(false);
        initialized = true;
    }
    public void Tick() {
        if (!initialized) {
            Debug.LogWarning("Attempted to tick an uninitialized entity " + gameObject.name);
            return;
        }
        if (!matchStarted)
            return;

        if (isRoundTimerRunning)
            UpdateRoundTimer();
    }
    private void SetupReferences() {

        animationComp = GetComponent<Animation>();
        Utility.Validate(animationComp, "Failed to get component Animation - MatchDirector", Utility.ValidationLevel.ERROR, true);

        Transform HUDTransform = transform.Find("HUD");
        Utility.Validate(HUDTransform, "Failed to get reference to HUD - MatchDirector", Utility.ValidationLevel.ERROR, true);

        Transform roundTimerTransform = HUDTransform.Find("RoundTimer");
        Utility.Validate(roundTimerTransform, "Failed to get reference to RoundTimer - MatchDirector", Utility.ValidationLevel.ERROR, true);

        roundTimerText = roundTimerTransform.GetComponent<TextMeshProUGUI>();
        Utility.Validate(roundTimerText, "Failed to get component TextMeshProUGUI in RoundTimer - MatchDirector", Utility.ValidationLevel.ERROR, true);

        var ScoreTransform = HUDTransform.Find("Score");
        Utility.Validate(ScoreTransform, "Failed to get reference to Score - MatchDirector", Utility.ValidationLevel.ERROR, true);

        var player1ScoreTextTransform = ScoreTransform.Find("Player1Score");
        var player2ScoreTextTransform = ScoreTransform.Find("Player2Score");
        Utility.Validate(player1ScoreTextTransform, "Failed to get reference to Player1Score - MatchDirector", Utility.ValidationLevel.ERROR, true);
        Utility.Validate(player2ScoreTextTransform, "Failed to get reference to Player2Score - MatchDirector", Utility.ValidationLevel.ERROR, true);

        player1ScoreText = player1ScoreTextTransform.GetComponent<TextMeshProUGUI>();
        player2ScoreText = player2ScoreTextTransform.GetComponent<TextMeshProUGUI>();
        Utility.Validate(player1ScoreText, "Failed to get component TextMeshProUGUI in player1ScoreText - MatchDirector", Utility.ValidationLevel.ERROR, true);
        Utility.Validate(player2ScoreText, "Failed to get component TextMeshProUGUI in player2ScoreText - MatchDirector", Utility.ValidationLevel.ERROR, true);
    }



    //NOTE: Consider registering death at the start of the anim since timer could run out during the running of the anim before it registers death!
    //Turning it off is managed by this class while turning it on is managed by GameInstance cause of countdown and setup by GameInstance
    public void SetRoundTimerState(bool state) {
        isRoundTimerRunning = state;
        roundTimerText.gameObject.SetActive(state);
    }
    public void ResetRoundTimer() {
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
        else
            ScoreDrawPoints();

        Debug.Log("Timeout!");

        //ADD INVINCIBILITY DURING THIS TO PLAYERS TO NOT SCORE TWICE!
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
        GetInstance().StartNewRound();
        ResetRoundTimer();
    }
    public void StartMatch() {
        matchStarted = true;
        ResetMatchData();
        ResetRoundTimer();
    }
    public void EndMatch() {
        matchStarted = false;
        SetRoundTimerState(false);
        GetInstance().EndMatch();
    }
    public void QuitMatch() {
        matchStarted = false;
        SetRoundTimerState(false);
    }




    private void ResetMatchData() {
        player1Score = 0;
        player2Score = 0;
        ScoreUpdateRefresh(); //Per match reset - will update to 0 - 0
        matchResults = MatchResults.NONE;
        roundTimer = 0.0f;
    }


    private void ScoreDrawPoints() {
        player1Score++;
        player2Score++;

        if (player1Score == pointsToWin && player2Score == pointsToWin)
            matchResults = MatchResults.DRAW;
        else if (player1Score == pointsToWin)
            matchResults = MatchResults.PLAYER_1_WINS;
        else if (player2Score == pointsToWin)
            matchResults = MatchResults.PLAYER_2_WINS;

        SetRoundTimerState(false);
        StartScoreUpdate(); //Triggers score update at each score point!
    }
    public void ScorePoint(Player.PlayerType type) {
        if (type == Player.PlayerType.NONE || matchResults != MatchResults.NONE)
            return;

        if (type == Player.PlayerType.PLAYER_1) {
            player1Score++;
            if (player1Score == pointsToWin)
                matchResults = MatchResults.PLAYER_1_WINS;
        }
        else if (type == Player.PlayerType.PLAYER_2) {
            player2Score++;
            if (player2Score == pointsToWin)
                matchResults = MatchResults.PLAYER_2_WINS;
        }

        SetRoundTimerState(false);
        StartScoreUpdate(); //Triggers score update at each score point!
    }
    private bool IsWinnerDecided() {
        if (matchResults != MatchResults.NONE)
            return true;
        return false;
    }
    public MatchResults GetWinner() {
        return matchResults;
    }
    public bool HasMatchStarted() {
        return matchStarted;
    }
}
