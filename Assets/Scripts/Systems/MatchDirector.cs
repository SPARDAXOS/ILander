using ILanderUtility;
using TMPro;
using UnityEngine;
using static GameInstance;

public class MatchDirector : MonoBehaviour {
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
    [SerializeField] uint roundTimeLimit = 60;


    private GameMode currentGameMode = GameMode.NONE;
    private bool initialized = false;
    private bool matchStarted = false;
    private bool isRoundTimerRunning = false;

    private uint player1Score = 0;
    private uint player2Score = 0;
    private MatchResults matchResults = MatchResults.NONE;

    private float roundTimer = 0.0f;

    private TextMeshProUGUI roundTimerText   = null;
    private TextMeshProUGUI player1ScoreText = null;
    private TextMeshProUGUI player2ScoreText = null;

    private Animation animationComp = null;


    public void Initialize() {
        if (initialized)
            return;

        SetupReferences();
        SetRoundTimerState(false);
        SetRoundTimerVisibility(false);
        initialized = true;
    }
    public void Tick() {
        if (!initialized) {
            Debug.LogWarning("Attempted to tick an uninitialized entity " + gameObject.name);
            return;
        }
        if (!matchStarted)
            return;

        currentGameMode = GetGameInstance().GetCurrentGameMode();
        if (isRoundTimerRunning) {
            if (currentGameMode == GameMode.COOP)
                UpdateRoundTimer();
            else if (currentGameMode == GameMode.LAN) {
                if (GetGameInstance().GetNetworkManagerScript().IsHost)
                    UpdateRoundTimer();
            }
        }
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

    public void SetRoundTimerState(bool state) {
        isRoundTimerRunning = state;
    }
    public void SetRoundTimerVisibility(bool state) {
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
            if (currentGameMode == GameMode.LAN && GetGameInstance().GetNetworkManagerScript().IsHost)
                GetGameInstance().GetRpcManagerScript().UpdateRoundTimerServerRpc(GetGameInstance().GetClientID(), roundTimer);

            if (roundTimer < 0.0f) {
                roundTimer = 0.0f;
                Timeout();
            }
        }
        else
            Timeout();
    }
    private void UpdateRoundTimerText() {
        roundTimerText.text = ((int)roundTimer).ToString();
    }
    private void Timeout() {
        var player1 = GetGameInstance().GetPlayer1Script();
        var player2 = GetGameInstance().GetPlayer2Script();

        if (player1.GetCurrentHealth() > player2.GetCurrentHealth())
            ScorePoint(Player.PlayerType.PLAYER_1);
        else if (player1.GetCurrentHealth() < player2.GetCurrentHealth())
            ScorePoint(Player.PlayerType.PLAYER_2);
        else
            ScoreDrawPoints();

        GetGameInstance().GetPlayer1Script().SetInvincible(true);
        GetGameInstance().GetPlayer2Script().SetInvincible(true);
    }
    public void ReceiveRoundTimerRpc(float value) {
        roundTimer = value;
        UpdateRoundTimerText();
        if (roundTimer < 0.0f) {
            roundTimer = 0.0f;
            Timeout();
        }
    }

    private void ScoreDrawPoints() {
        if (animationComp.isPlaying)
            return;

        player1Score++;
        player2Score++;

        if (player1Score == pointsToWin && player2Score == pointsToWin)
            matchResults = MatchResults.DRAW;
        else if (player1Score == pointsToWin)
            matchResults = MatchResults.PLAYER_1_WINS;
        else if (player2Score == pointsToWin)
            matchResults = MatchResults.PLAYER_2_WINS;

        SetRoundTimerState(false);
        SetRoundTimerVisibility(false);
        StartScoreUpdate(); //Triggers score update at each score point!
    }
    public void ScorePoint(Player.PlayerType type) {
        if (animationComp.isPlaying)
            return;

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
        SetRoundTimerVisibility(false);
        StartScoreUpdate(); //Triggers score update at each score point!
    }
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

    private void ResetMatchData() {
        player1Score = 0;
        player2Score = 0;
        ScoreUpdateRefresh(); //Per match reset - will update to 0 - 0
        matchResults = MatchResults.NONE;
        roundTimer = 0.0f;
    }
    private void StartNewRound() {
        GetGameInstance().StartNewRound();
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
        SetRoundTimerVisibility(false);
        GetGameInstance().EndMatch();
    }
    public void QuitMatch() {
        matchStarted = false;
        SetRoundTimerState(false);
        SetRoundTimerVisibility(false);
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
