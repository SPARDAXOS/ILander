using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class GameInstance : MonoBehaviour
{


    //NOTE: Break this into ApplicationStatus and GameStatus
    //NOTE: Instead create a initialization steps?`state? not sure


    public enum ApplicationState
    {
        STOPPED = 0,
        INITIALIZING,
        RUNNING
    }
    public enum GameState
    {
        NONE = 0,
        MAIN_MENU,
        SETTINGS_MENU,
        GAMEMODE_MENU,
        CONNECTION_MENU,
        CUSTOMIZATION_MENU,
        LEVEL_SELECT_MENU,
        LOADING_SCREEN,
        PAUSE_MENU,
        PLAYING
    }
    public enum GameMode
    {
        NONE = 0,
        COOP,
        LAN
    }
    private enum LoadingScreenProcess {
        NONE = 0,
        LOADING_LEVEL
    }

    public ApplicationState currentApplicationStatus = ApplicationState.STOPPED;
    public GameState currentGameState = GameState.NONE;
    public GameMode currentGameMode = GameMode.NONE;
    private LoadingScreenProcess currentLoadingScreenProcess = LoadingScreenProcess.NONE;

    //The most pritle part of the loading process.
    private const string assetsBundleKey       = "MainAssetsBundle"; 
    private const string levelsBundleKey           = "MainLevelsBundle";
    private const string gameSettingsLabel         = "MainGameSettings";
    private const string playerCharactersBundleKey = "MainPlayerCharactersBundle";

    private static GameInstance instance;

    //Temp PUBLICS!


    //Dont like names,  game, characters
    private LevelsBundle levelsBundle = null;
    public AssetsBundle assetsBundle = null;
    public GameSettings gameSettings = null;
    public PlayerCharactersBundle playerCharactersBundle = null;





    public Dictionary<string, AsyncOperationHandle<GameObject>> loadedAssets = new Dictionary<string, AsyncOperationHandle<GameObject>>();

    //Terrible names
    private AsyncOperationHandle<GameObject> currentLoadedLevelHandle;
    private GameObject currentLoadedLevel = null;
    private Level currentLoadedLevelScript = null;


    //Sus
    private bool initializationInProgress = false;
    //Shortyen these names
    private bool assetsBundleLoadingInProgress = false;
    private bool levelsBundleLoadingInProgress = false;
    private bool gameSettingsLoadingInProgress = false;
    private bool playerCharactersBundleLoadingInProgress = false;

    private bool assetsLoadingInProgress = false;

    private bool assetsLoaded = false;
    private bool gameInitialized = false;



    private GameObject player1;
    private GameObject player2;
    private GameObject mainCamera;
    private GameObject mainMenu;
    private GameObject settingsMenu;
    private GameObject gameModeMenu;
    private GameObject connectionMenu;
    private GameObject customizationMenu;
    private GameObject LevelSelectMenu;
    private GameObject loadingScreen;
    private GameObject transitionMenu;
    private GameObject countdownMenu;
    private GameObject eventSystem;
    private GameObject networkManager;

    private Player player1Script;
    private Player player2Script;
    private MainCamera mainCameraScript;
    private MainMenu mainMenuScript;
    private SettingsMenu settingsMenuScript;
    private GameModeMenu gameModeMenuScript;
    private ConnectionMenu connectionMenuScript;
    private CustomizationMenu customizationMenuScript;
    private LevelSelectMenu LevelSelectMenuScript;
    private LoadingScreen loadingScreenScript;
    private TransitionMenu transitionMenuScript;
    private CountdownMenu countdownMenuScript;
    private NetworkManager networkManagerScript;

    private Camera mainCameraComponent;

    void Update() {
        if (networkManagerScript)
        {
            Debug.Log(networkManagerScript.IsConnectedClient);
        }
        switch (currentApplicationStatus) {
            case ApplicationState.INITIALIZING:
                UpdateInitializingStatus();
            break;
            case ApplicationState.RUNNING:
                RunGame();
            break;
        }
    }
    private void UpdateInitializingStatus()
    {
        if (gameInitialized) {
            currentApplicationStatus = ApplicationState.RUNNING;
            return;                                         
        }

        //Initialization steps:
        //Load Settings
        //Load PlayerCharacters
        //Load LevelsBundle
        //Load AssetsBundle
        //Load Assets
        //Create Entities
        //Setup Entities
        //Setup Main Menu State

        //Step 1 of this func is to check whether all necessary bundles have been loaded. Could be broken into two functions!
        if (!assetsBundle) {
            if (assetsBundleLoadingInProgress)
                Debug.Log("Waiting on GameAssetsBundle to load!");
            else
                Debug.LogError("Unable to load assets. \n GameAssetsBundle is missing!");
            return;
        }
        if (!levelsBundle) {
            if (levelsBundleLoadingInProgress)
                Debug.Log("Waiting on GameLevelsBundle to load!");
            else
                Debug.LogError("Unable to load assets. \n GameLevelsBundle is missing!");
            return;
        }
        if (!gameSettings) {
            if (gameSettingsLoadingInProgress)
                Debug.Log("Waiting on GameSettings to load!");
            else
                Debug.LogError("Unable to load assets. \n GameSettings is missing!");
            return;
        }
        if (!playerCharactersBundle) {
            if (playerCharactersBundleLoadingInProgress)
                Debug.Log("Waiting on PlayerCharactersBundle to load!");
            else
                Debug.LogError("Unable to load assets. \n PlayerCharactersBundle is missing!");
            return;
        }


        //Step 2 of this func is to load assets, check their loading status then create entities.
        //NOTE: Too many unnecessary checks all over the place!. Is this obsession with safety worth it?
        if (!assetsLoaded && !assetsLoadingInProgress)
            LoadGameAssets();
        else if (assetsLoadingInProgress)
            CheckAssetsLoadingStatus();
        else if (assetsLoaded && !gameInitialized)
        {
            CreateEntities();
            SetupEntities();
            SetupMainMenuState();
            //HERE! need to setup GameStartState! then run the game!
            gameInitialized = true;
            currentApplicationStatus = ApplicationState.RUNNING;
            Debug.Log("Finished Initializing Game!");
        }
    }
    private void RunGame() {
        //If any actions need to be taken during any of the game states, they should be added here!

        //Wait wot. This is called every single frame even if im in the menu? no its just confusing!
        switch (currentGameState) {
            case GameState.LOADING_SCREEN:
                UpdateLoadingState();
                break;
            case GameState.PLAYING:
                UpdatePlayingState();
                break;

        }
    }



    private void UpdatePlayingState() {

        //Questionable
        if (countdownMenuScript.IsAnimationPlaying())
            countdownMenuScript.Tick();

        player1Script.Tick();
        player2Script.Tick();
        mainCameraScript.Tick();
    }
    private void UpdateLoadingState() {
        float value = 0.0f;

        if (currentLoadingScreenProcess == LoadingScreenProcess.LOADING_LEVEL) //Its only to figure out which value for the bar to get!
            value = currentLoadedLevelHandle.PercentComplete;

        loadingScreenScript.SetLoadingBarValue(value);
    }




    private void Awake() {
        if (!instance) {
            instance = this;
            DontDestroyOnLoad(gameObject);
            return;
        }

        Debug.LogWarning("Instance of 'GameInstance' already exists!");
        Destroy(gameObject);
    }
    private void OnDestroy() {
        if (loadedAssets.Count == 0)
            return;
        foreach (var entry in loadedAssets)
            Addressables.Release(entry.Value);

        //Unload loaded level!
        //Unload Settings file!
        //Unload characters file!
    }

    //??? check vid
    public static GameInstance GetInstance() {
        return instance;
    }
    public void Abort(string errorMessage) {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
        Debug.LogError(errorMessage);
#else
        Application.Quit();
#endif
    }



    public void Initialize() {
        //ADD OTHERS!
        if (assetsBundle && levelsBundle) { //Assets too? maybe just use the init bool instead?
            Debug.Log("Game is already initalized!");
            return;
        }
        if (initializationInProgress) {
            Debug.Log("Initalization already in progress!");
            return;
        }

        Debug.Log("Started Initializing Game!");
        currentApplicationStatus = ApplicationState.INITIALIZING;
        initializationInProgress = true;

        LoadGameSettings();
        LoadPlayerCharactersBundle();
        LoadLevelsBundle();
        LoadGameAssetsBundle();
    }



    private void LoadGameSettings() {
        if (gameSettings) {
            Debug.Log("GameSetting is already loaded!");
            return;
        }
        Debug.Log("Started Loading GameSettings!");
        AssetLabelReference GameSettingsLabel = new AssetLabelReference
        {
            labelString = gameSettingsLabel
        };
        Addressables.LoadAssetAsync<GameSettings>(GameSettingsLabel).Completed += GameSettingsLoadingCallback;
        gameSettingsLoadingInProgress = true;
    }
    private void LoadPlayerCharactersBundle() {
        if (playerCharactersBundle) {
            Debug.Log("PlayerCharactersBundle is already loaded!");
            return;
        }
        Debug.Log("Started Loading PlayerCharactersBundle!");
        AssetLabelReference playerCharactersBundleLabel = new AssetLabelReference
        {
            labelString = playerCharactersBundleKey
        };
        Addressables.LoadAssetAsync<PlayerCharactersBundle>(playerCharactersBundleLabel).Completed += PlayerCharactersBundleLoadingCallback;
        playerCharactersBundleLoadingInProgress = true;
    }
    private void LoadLevelsBundle(){
        if (levelsBundle)
        {
            Debug.Log("LevelsBundle is already loaded!");
            return;
        }
        Debug.Log("Started Loading LevelsBundle!");
        AssetLabelReference LevelsBundleLabel = new AssetLabelReference
        {
            labelString = levelsBundleKey
        };
        Addressables.LoadAssetAsync<LevelsBundle>(LevelsBundleLabel).Completed += GameLevelsBundleLoadingCallback;
        levelsBundleLoadingInProgress = true;
    }
    private void LoadGameAssetsBundle() {
        if (assetsBundle) {
            Debug.Log("AssetsBundle is already loaded!");
            return;
        }
        Debug.Log("Started Loading AssetsBundle!");
        AssetLabelReference AssetsLabel = new AssetLabelReference {
            labelString = assetsBundleKey
        };
        Addressables.LoadAssetAsync<AssetsBundle>(AssetsLabel).Completed += GameAssetsBundleLoadingCallback;
        assetsBundleLoadingInProgress = true;
    }
    private void LoadGameAssets() {
        Debug.Log("Started Loading Assets!");
        foreach (var entry in assetsBundle.assets) {
            if (entry.name.Length == 0) //Skip empty entries in the bundle.
                continue;

            var handle = Addressables.LoadAssetAsync<GameObject>(entry.reference);
            handle.Completed += GameObjectLoadingCallback;
            loadedAssets.Add(entry.name, handle);
        }
        assetsLoadingInProgress = true;
    }
    private void CheckAssetsLoadingStatus()
    {
        bool checkResults = true;
        
        foreach(var entry in loadedAssets) {
            if (entry.Value.Status != AsyncOperationStatus.Succeeded)
                checkResults = false;
        }

        if (checkResults) {
            Debug.Log("Finished Loading Assets!");
            assetsLoadingInProgress = false;
            assetsLoaded = true;
        }
    }


    private void CreateEntities() {
        Debug.Log("Started Creating Entities!");


        eventSystem = Instantiate(loadedAssets["EventSystem"].Result);

        networkManager = Instantiate(loadedAssets["NetworkManager"].Result);
        networkManagerScript = networkManager.GetComponent<NetworkManager>();
        networkManager.SetActive(false);

        player1 = Instantiate(loadedAssets["Player"].Result);
        player1.name = "Player1";
        player1.SetActive(false);
        player1Script = player1.GetComponent<Player>();
        player1Script.Initialize();
        player1Script.SetPlayerType(Player.PlayerType.PLAYER_1);

        player2 = Instantiate(loadedAssets["Player"].Result);
        player2.name = "Player2";
        player2.SetActive(false);
        player2Script = player2.GetComponent<Player>();
        player2Script.Initialize();
        player2Script.SetPlayerType(Player.PlayerType.PLAYER_2);

        mainCamera = Instantiate(loadedAssets["MainCamera"].Result);
        mainCameraScript = mainCamera.GetComponent<MainCamera>();
        mainCameraComponent = mainCamera.GetComponent<Camera>();
        mainCameraScript.Initialize();

        mainMenu = Instantiate(loadedAssets["MainMenu"].Result);
        mainMenu.SetActive(false);
        mainMenuScript = mainMenu.GetComponent<MainMenu>();
        mainMenuScript.Initialize();

        settingsMenu = Instantiate(loadedAssets["SettingsMenu"].Result);
        settingsMenu.SetActive(false);
        settingsMenuScript = settingsMenu.GetComponent<SettingsMenu>();
        settingsMenuScript.Initialize();

        gameModeMenu = Instantiate(loadedAssets["GameModeMenu"].Result);
        gameModeMenu.SetActive(false);
        gameModeMenuScript = gameModeMenu.GetComponent<GameModeMenu>(); //Is getting this script even has any value? i wont call anything from it. It has button code only!

        connectionMenu = Instantiate(loadedAssets["ConnectionMenu"].Result);
        connectionMenu.SetActive(false);
        connectionMenuScript = connectionMenu.GetComponent<ConnectionMenu>();
        connectionMenuScript.Initialize();

        customizationMenu = Instantiate(loadedAssets["CustomizationMenu"].Result);
        customizationMenu.SetActive(false);
        customizationMenuScript = customizationMenu.GetComponent<CustomizationMenu>();
        customizationMenuScript.Initialize();

        LevelSelectMenu = Instantiate(loadedAssets["LevelSelectMenu"].Result);
        LevelSelectMenu.SetActive(false);
        LevelSelectMenuScript = LevelSelectMenu.GetComponent<LevelSelectMenu>();
        LevelSelectMenuScript.Initialize();

        loadingScreen = Instantiate(loadedAssets["LoadingScreen"].Result);
        loadingScreen.SetActive(false);
        loadingScreenScript = loadingScreen.GetComponent<LoadingScreen>();
        loadingScreenScript.Initialize();

        transitionMenu = Instantiate(loadedAssets["TransitionMenu"].Result);
        transitionMenu.SetActive(false);
        transitionMenuScript = transitionMenu.GetComponent<TransitionMenu>();
        transitionMenuScript.Initialize();

        countdownMenu = Instantiate(loadedAssets["CountdownMenu"].Result);
        countdownMenu.SetActive(false);
        countdownMenuScript = countdownMenu.GetComponent<CountdownMenu>();
        countdownMenuScript.Initialize();


        Debug.Log("Finished Creating Entities!");
    }
    private void SetupEntities() {
        mainCameraScript.SetFollowTarget(player1); //Deprecated

        customizationMenuScript.SetRenderCameraTarget(mainCameraComponent);
        LevelSelectMenuScript.SetLevelsBundle(levelsBundle);

        //SetControlSchemes from loaded addressable control shcemes? both players use the same one now.

    }

    //FIX INIFINITE RELOADING OF ASSETS IN CASE OF ERROR! initialization loop
    public void SetGameState(GameState state) {
        switch (state) {
            case GameState.MAIN_MENU:
                transitionMenuScript.StartTransition(SetupMainMenuState); //This scares me!  I never call this with using SetGameState. I call SetupMainMenuState directly!
                break;
            case GameState.SETTINGS_MENU:
                transitionMenuScript.StartTransition(SetupSettingsMenuState);
                break;
            case GameState.GAMEMODE_MENU:
                transitionMenuScript.StartTransition(SetupGameModeMenuState);
                break;
            case GameState.CONNECTION_MENU:
                transitionMenuScript.StartTransition(SetupConnectionMenuState);
                break;
            case GameState.CUSTOMIZATION_MENU:
                transitionMenuScript.StartTransition(SetupCustomizationMenuState);
                break;
            case GameState.LEVEL_SELECT_MENU:
                transitionMenuScript.StartTransition(SetupLevelSelectMenuState);
                break;
            case GameState.LOADING_SCREEN:
                Debug.LogWarning("You cant use SetGameState to transition into a loading screen \n StartLoadingScreenProcess is used instead for internal use only!");
                break;
            case GameState.PAUSE_MENU:
                SetupPauseMenuState();
                break;
            case GameState.PLAYING:
                transitionMenuScript.StartTransition(SetupPlayState);//Should set the game in start state as if you just clicked play!
                break;
        }
    }
    private void SetupMainMenuState() {
        HideAllMenus();
        EnableMouseCursor();
        mainMenu.SetActive(true);
        currentGameState = GameState.MAIN_MENU;
    }
    private void SetupSettingsMenuState() {
        HideAllMenus();
        EnableMouseCursor();
        settingsMenu.SetActive(true);
        currentGameState = GameState.SETTINGS_MENU;
    }
    private void SetupGameModeMenuState() {
        HideAllMenus(); //hmm, with transition,,,
        EnableMouseCursor();
        gameModeMenu.SetActive(true);
        currentGameState = GameState.GAMEMODE_MENU;
    }
    private void SetupConnectionMenuState() {
        HideAllMenus();
        EnableMouseCursor();
        connectionMenu.SetActive(true);
        currentGameState = GameState.CONNECTION_MENU;
    }
    private void SetupCustomizationMenuState() {
        HideAllMenus();
        EnableMouseCursor();
        customizationMenu.SetActive(true);
        currentGameState = GameState.CUSTOMIZATION_MENU;
    }
    private void SetupLevelSelectMenuState() {
        HideAllMenus();
        EnableMouseCursor();
        LevelSelectMenu.SetActive(true);
        currentGameState = GameState.LEVEL_SELECT_MENU;
    }
    private void SetupLoadingScreenState() {
        HideAllMenus();
        DisableMouseCursor();
        loadingScreen.SetActive(true);
        currentGameState = GameState.LOADING_SCREEN;
    }
    private void SetupPauseMenuState() {
        //Hide HUDs
        EnableMouseCursor();
        //mainMenu.SetActive(true);
        currentGameState = GameState.PAUSE_MENU;
    }

    //Probably need to change name to refelect which state it resets - ADD A SECOND ONE? START STATE AND PLAYSSTATE

    //Now it seems like they do the same thing...
    private void SetupLevelStartState() {
        HideAllMenus(); //To turn off loading screen
        DisableMouseCursor();
        //Get data from level and setup start state for players by reseting them and positing them
        //Reset any match score between them
        //Disable their input
        //Start count down at the end
        countdownMenu.SetActive(true);
        countdownMenuScript.StartAnimation(StartMatch);
    }




    public void StartAsHost() {

        networkManagerScript.StartHost();
        Debug.Log("Started as Host!");
    }
    public void StartAsClient() {

        networkManagerScript.StartClient();
        Debug.Log("Started as Client!");
    }



    private void EnableMouseCursor() {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
    private void DisableMouseCursor() {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }


    //Make funcs for cursor!!!!!

    private void SetupPlayState() {
        //TEMP
        //Assert if gamemode is none! 


        currentGameState = GameState.PLAYING;
    }



    public void ConfirmCharacterSelection(Player.PlayerType type, PlayerCharacterData data) {
        if (type == Player.PlayerType.NONE)
            return;

        if (type == Player.PlayerType.PLAYER_1)
            player1Script.SetPlayerData(data);
        else if (type == Player.PlayerType.PLAYER_2)
            player2Script.SetPlayerData(data);
    }
    public void SetGameModeSelection(GameMode mode) {
        if (mode == GameMode.NONE) //?? this means i can never return to none.
            return;

        currentGameMode = mode;

        if (mode == GameMode.COOP) {
            player1Script.DisableNetworking();
            player2Script.DisableNetworking();
            networkManager.SetActive(false);
        }
        else if (mode == GameMode.LAN) {
            player1Script.EnableNetworking();
            player2Script.EnableNetworking();
            networkManager.SetActive(true);
        }

        //SetMode? for players (That func would manage EnableNetworking/DisableNetworking then!) and menus!

        //Do online or disable online stuff and set changes to menus!
    }
    public void StartLevel(uint level) {
        if (currentLoadedLevel) {
            Debug.LogError("Attempted to start a level while another level was loaded!");
            return;
        }

        currentLoadedLevelHandle = Addressables.LoadAssetAsync<GameObject>(levelsBundle.levels[level].asset);
        currentLoadedLevelHandle.Completed += LevelLoadedCallback;
        StartLoadingScreenProcess(LoadingScreenProcess.LOADING_LEVEL);
    }


    private void UnloadCurrentLevel() {
        if (currentLoadedLevelHandle.IsValid())
            Addressables.Release(currentLoadedLevelHandle);
        //Delete Instansiated go
        //Other stuff? change func name maybe to something more final! EndGame?
    }

    //Unnecessary but in case the loading screen is reused for different types of loading (levels, leaderboards, etc)
    private void StartLoadingScreenProcess(LoadingScreenProcess process) {
        if (process == LoadingScreenProcess.NONE)
            return;

        currentLoadingScreenProcess = process;
        SetupLoadingScreenState();
    }




    private void StartMatch() {
        //Not finished
        Debug.Log("Invocation worked!");
        player1.SetActive(true);
        player1.transform.position = currentLoadedLevelScript.GetPlayer1SpawnPoint();
        player1Script.EnableInput();
        player2.SetActive(true);
        player2.transform.position = currentLoadedLevelScript.GetPlayer2SpawnPoint();
        player2Script.EnableInput();
        //TRurn off countdown menu?
    }
    private void HideAllMenus() {
        //Add all menus here!
        mainMenu.SetActive(false);
        customizationMenu.SetActive(false);
        LevelSelectMenu.SetActive(false);
        settingsMenu.SetActive(false);
        gameModeMenu.SetActive(false);
        countdownMenu.SetActive(false);
        loadingScreen.SetActive(false);
    }




    public PlayerCharactersBundle GetPlayerCharactersBundle() {
        return playerCharactersBundle;
    }
    public GameSettings GetGameSettings() {
        return gameSettings;
    }




    private void LevelLoadedCallback(AsyncOperationHandle<GameObject> handle)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            Debug.Log("Loaded level " + handle.Result.ToString() + " successfully");

            currentLoadedLevel = Instantiate(handle.Result);
            currentLoadedLevelScript = currentLoadedLevel.GetComponent<Level>();
            currentLoadedLevelScript.Initialize();

            SetupLevelStartState();
            SetupPlayState();
        }
        else
        {
            Debug.LogError("Failed to load level");
            SetupMainMenuState();
        }

        currentLoadingScreenProcess = LoadingScreenProcess.NONE;
    }
    private void GameSettingsLoadingCallback(AsyncOperationHandle<GameSettings> handle) {
        if (handle.Status == AsyncOperationStatus.Succeeded) {
            gameSettings = handle.Result;
            Debug.Log("GameSettings Loaded Successfully!");
        }
        else {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Abort("Failed to load GameSettings! \n exiting..");
#endif
        }

        gameSettingsLoadingInProgress = false;
    }
    private void PlayerCharactersBundleLoadingCallback(AsyncOperationHandle<PlayerCharactersBundle> handle) {
        if (handle.Status == AsyncOperationStatus.Succeeded) {
            playerCharactersBundle = handle.Result;
            Debug.Log("PlayerCharactersBundle Loaded Successfully!");
        }
        else {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Abort("Failed to load PlayerCharactersBundle! \n exiting..");
#endif
        }

        playerCharactersBundleLoadingInProgress = false;
    }
    private void GameAssetsBundleLoadingCallback(AsyncOperationHandle<AssetsBundle> handle) {
        if (handle.Status == AsyncOperationStatus.Succeeded) {
            assetsBundle = handle.Result;
            Debug.Log("GameAssetsBundle Loaded Successfully!");
        }
        else {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Abort("Failed to load assets bundle! \n exiting..");
#endif
        }

        assetsBundleLoadingInProgress = false;
    }
    private void GameLevelsBundleLoadingCallback(AsyncOperationHandle<LevelsBundle> handle) {
        if (handle.Status == AsyncOperationStatus.Succeeded) {
            levelsBundle = handle.Result;
            Debug.Log("GameLevelsBundle Loaded Successfully!");
        }
        else {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Abort("Failed to load levels bundle! \n exiting..");
#endif
        }

        assetsBundleLoadingInProgress = false;
    }
    private void GameObjectLoadingCallback(AsyncOperationHandle<GameObject> handle) {
        if (handle.Status == AsyncOperationStatus.Succeeded) {
            Debug.Log("Successfully loaded " + handle.Result.ToString());
        }
        else {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Abort("Asset " + handle.ToString() + " failed to load!");
#endif
        }
    }
}
