using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using Unity.Netcode;
using Unity.Netcode.Components;
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
        RESULTS_MENU,
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
    public enum ConnectionState
    {
        NONE = 0,
        CLIENT,
        HOST
    }


    public ApplicationState currentApplicationStatus = ApplicationState.STOPPED;
    public GameState currentGameState = GameState.NONE;
    public GameMode currentGameMode = GameMode.NONE;
    public ConnectionState currentConnectionState = ConnectionState.NONE;
    private LoadingScreenProcess currentLoadingScreenProcess = LoadingScreenProcess.NONE;

    //The most pritle part of the loading process.
    private const string assetsBundleKey           = "MainAssetsBundle"; 
    private const string levelsBundleKey           = "MainLevelsBundle";
    private const string gameSettingsLabel         = "MainGameSettings";
    private const string playerCharactersBundleKey = "MainPlayerCharactersBundle";

    private static GameInstance instance;

    //Temp PUBLICS!


    //Dont like names,  game, characters
    private LevelsBundle levelsBundle = null;
    private AssetsBundle assetsBundle = null;
    private GameSettings gameSettings = null;
    private PlayerCharactersBundle playerCharactersBundle = null;
    private Dictionary<string, AsyncOperationHandle<GameObject>> loadedAssets = new Dictionary<string, AsyncOperationHandle<GameObject>>();


    //Terrible names
    private AsyncOperationHandle<GameObject> currentLoadedLevelHandle;
    private GameObject currentLoadedLevel = null;
    private Level currentLoadedLevelScript = null;


    private bool initializationInProgress = false;

    private bool assetsBundleIsLoading = false;
    private bool levelsBundleIsLoading = false;
    private bool gameSettingsIsLoading = false;
    private bool playerCharactersBundleIsLoading = false;
    private bool assetsLoadingInProgress = false;

    private bool assetsLoaded = false;
    private bool gameInitialized = false;

    public uint connectedClients = 0;
    public long clientID = -1;

    private GameObject player1;
    private GameObject player2;
    private GameObject mainCamera;
    private GameObject mainMenu;
    private GameObject settingsMenu;
    private GameObject gameModeMenu;
    private GameObject connectionMenu;
    private GameObject customizationMenu;
    private GameObject levelSelectMenu;
    private GameObject loadingScreen;
    private GameObject transitionMenu;
    private GameObject countdownMenu;
    private GameObject eventSystem;
    private GameObject networkManager;
    private GameObject rpcManager;
    private GameObject HUD;
    private GameObject pauseMenu;
    private GameObject resultsMenu;
    private GameObject matchDirector;

    public Player player1Script;
    public Player player2Script;
    public NetworkObject player1NetworkObject;
    public NetworkObject player2NetworkObject;
    private MainCamera mainCameraScript;
    private MainMenu mainMenuScript;
    private SettingsMenu settingsMenuScript;
    private GameModeMenu gameModeMenuScript;
    private ConnectionMenu connectionMenuScript;
    private CustomizationMenu customizationMenuScript;
    private LevelSelectMenu levelSelectMenuScript;
    private LoadingScreen loadingScreenScript;
    private TransitionMenu transitionMenuScript;
    private CountdownMenu countdownMenuScript;
    private NetworkManager networkManagerScript;
    public RpcManager rpcManagerScript;
    private HUD HUDScript;
    private PauseMenu pauseMenuScript;
    private ResultsMenu resultsMenuScript;
    private MatchDirector matchDirectorScript;

    private Camera mainCameraComponent;




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

        //DESTROY FIRST THEN RELEASE!


        foreach (var entry in loadedAssets)
            Addressables.Release(entry.Value);

        //Unload loaded level!
        //Unload Settings file!
        //Unload characters file!
    }
    void Update() {
        switch (currentApplicationStatus) {
            case ApplicationState.INITIALIZING:
                UpdateApplicationInitializingState();
            break;
            case ApplicationState.RUNNING:
                UpdateApplicationRunningState();
            break;
        }
    }
    private void FixedUpdate() {
        switch (currentGameState) {
            case GameState.PLAYING:
                UpdatePlayingFixedState();
                break;
        }
    }


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



    private void UpdateApplicationInitializingState() {
        if (gameInitialized) {
            currentApplicationStatus = ApplicationState.RUNNING;
            return;                                         
        }

        if (!AreEssentialBundlesLoaded())
            return;

        if (assetsLoadingInProgress)
            CheckAssetsLoadingStatus();
        else if (!assetsLoaded)
            LoadAssets();

        if (assetsLoaded) {
            CreateEntities();
            SetupEntities();
            SetGameState(GameState.MAIN_MENU);
            gameInitialized = true;
            currentApplicationStatus = ApplicationState.RUNNING;
            Debug.Log("Finished Initializing Game!");
        }
    }
    private void UpdateApplicationRunningState() {
        //If any actions need to be taken during any of the game states, they should be added here!

        //Wait wot. This is called every single frame even if im in the menu? no its just confusing!
        switch (currentGameState) {
            case GameState.LOADING_SCREEN:
                UpdateLoadingState();
                break;
            case GameState.CUSTOMIZATION_MENU:
                customizationMenuScript.Tick();
                break;
            case GameState.RESULTS_MENU:
                resultsMenuScript.Tick();
                break;
            case GameState.PLAYING:
                UpdatePlayingState();
                break;

        }
    }


    private void UpdatePlayingState() {

        //ONLINE HERE

        //Questionable
        if (countdownMenuScript.IsAnimationPlaying())
            countdownMenuScript.Tick();

        if (currentLoadedLevel)
            currentLoadedLevelScript.Tick();

        matchDirectorScript.Tick();

        //Crashes in online
        player1Script.Tick();
        player2Script.Tick();
        mainCameraScript.Tick();
    }
    private void UpdatePlayingFixedState() {

        //ONLINE HERE

        //Crashes in online
        player1Script.FixedTick();
        player2Script.FixedTick();
    }

    private void UpdateLoadingState() {
        float value = 0.0f;

        if (currentLoadingScreenProcess == LoadingScreenProcess.LOADING_LEVEL) //Its only to figure out which value for the bar to get!
            value = currentLoadedLevelHandle.PercentComplete;

        loadingScreenScript.SetLoadingBarValue(value);
    }


    public void Initialize() {
        if (gameInitialized) {
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
        LoadEssentialBundles();
    }
    private void LoadEssentialBundles() {
        LoadGameSettings();
        LoadPlayerCharactersBundle();
        LoadLevelsBundle();
        LoadAssetsBundle();
    }
    private void LoadGameSettings() {
        if (gameSettings) {
            Debug.Log("GameSetting is already loaded!");
            return;
        }
        Debug.Log("Started Loading GameSettings!");
        AssetLabelReference GameSettingsLabel = new AssetLabelReference {
            labelString = gameSettingsLabel
        };
        Addressables.LoadAssetAsync<GameSettings>(GameSettingsLabel).Completed += GameSettingsLoadingCallback;
        gameSettingsIsLoading = true;
    }
    private void LoadPlayerCharactersBundle() {
        if (playerCharactersBundle) {
            Debug.Log("PlayerCharactersBundle is already loaded!");
            return;
        }
        Debug.Log("Started Loading PlayerCharactersBundle!");
        AssetLabelReference playerCharactersBundleLabel = new AssetLabelReference {
            labelString = playerCharactersBundleKey
        };
        Addressables.LoadAssetAsync<PlayerCharactersBundle>(playerCharactersBundleLabel).Completed += PlayerCharactersBundleLoadingCallback;
        playerCharactersBundleIsLoading = true;
    }
    private void LoadLevelsBundle() {
        if (levelsBundle) {
            Debug.Log("LevelsBundle is already loaded!");
            return;
        }
        Debug.Log("Started Loading LevelsBundle!");
        AssetLabelReference LevelsBundleLabel = new AssetLabelReference {
            labelString = levelsBundleKey
        };
        Addressables.LoadAssetAsync<LevelsBundle>(LevelsBundleLabel).Completed += GameLevelsBundleLoadingCallback;
        levelsBundleIsLoading = true;
    }
    private void LoadAssetsBundle() {
        if (assetsBundle) {
            Debug.Log("AssetsBundle is already loaded!");
            return;
        }
        Debug.Log("Started Loading AssetsBundle!");
        AssetLabelReference AssetsLabel = new AssetLabelReference {
            labelString = assetsBundleKey
        };
        Addressables.LoadAssetAsync<AssetsBundle>(AssetsLabel).Completed += GameAssetsBundleLoadingCallback;
        assetsBundleIsLoading = true;
    }
    private void LoadAssets() {
        Debug.Log("Started Loading Assets!");
        assetsLoadingInProgress = true;
        foreach (var entry in assetsBundle.assets) {
            if (entry.name.Length == 0) //Skip empty entries in the bundle.
                continue;

            var handle = Addressables.LoadAssetAsync<GameObject>(entry.reference);
            handle.Completed += GameObjectLoadingCallback;
            loadedAssets.Add(entry.name, handle);
        }
    }
    private void CheckAssetsLoadingStatus() {
        bool checkResults = true;

        foreach (var entry in loadedAssets) {
            if (entry.Value.Status != AsyncOperationStatus.Succeeded)
                checkResults = false;
        }
        if (checkResults) {
            Debug.Log("Finished Loading Assets!");
            assetsLoadingInProgress = false;
            assetsLoaded = true;
        }
    }
    private bool AreEssentialBundlesLoaded() {
        if (!assetsBundle) {
            if (assetsBundleIsLoading)
                Debug.Log("Waiting on GameAssetsBundle to load!");
            else
                Debug.LogError("Unable to load assets. \n GameAssetsBundle is missing!");
            return false;
        }
        if (!levelsBundle) {
            if (levelsBundleIsLoading)
                Debug.Log("Waiting on GameLevelsBundle to load!");
            else
                Debug.LogError("Unable to load assets. \n GameLevelsBundle is missing!");
            return false;
        }
        if (!gameSettings) {
            if (gameSettingsIsLoading)
                Debug.Log("Waiting on GameSettings to load!");
            else
                Debug.LogError("Unable to load assets. \n GameSettings is missing!");
            return false;
        }
        if (!playerCharactersBundle) {
            if (playerCharactersBundleIsLoading)
                Debug.Log("Waiting on PlayerCharactersBundle to load!");
            else
                Debug.LogError("Unable to load assets. \n PlayerCharactersBundle is missing!");
            return false;
        }

        return true;
    }
    private void CreateEntities() {
        Debug.Log("Started Creating Entities!");

        try {
            eventSystem = Instantiate(loadedAssets["EventSystem"].Result);

            networkManager = Instantiate(loadedAssets["NetworkManager"].Result);
            networkManagerScript = networkManager.GetComponent<Unity.Netcode.NetworkManager>();
            networkManagerScript.OnClientConnectedCallback += ClientConnectedCallback;
            networkManagerScript.ConnectionApprovalCallback += ClientApprovalCallback;
            networkManagerScript.OnClientDisconnectCallback += ClientDisconnectedCallback;
            networkManager.SetActive(false);

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

            levelSelectMenu = Instantiate(loadedAssets["LevelSelectMenu"].Result);
            levelSelectMenu.SetActive(false);
            levelSelectMenuScript = levelSelectMenu.GetComponent<LevelSelectMenu>();
            levelSelectMenuScript.Initialize();

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

            HUD = Instantiate(loadedAssets["HUD"].Result);
            HUD.SetActive(false);
            HUDScript = HUD.GetComponent<HUD>();
            HUDScript.Initialize();

            pauseMenu = Instantiate(loadedAssets["PauseMenu"].Result);
            pauseMenu.SetActive(false);
            pauseMenuScript = pauseMenu.GetComponent<PauseMenu>();

            resultsMenu = Instantiate(loadedAssets["ResultsMenu"].Result);
            resultsMenu.SetActive(false);
            resultsMenuScript = resultsMenu.GetComponent<ResultsMenu>();
            resultsMenuScript.Initialize();

            matchDirector = Instantiate(loadedAssets["MatchDirector"].Result);
            matchDirector.SetActive(false);
            matchDirectorScript = matchDirector.GetComponent<MatchDirector>();
            matchDirectorScript.Initialize();

            Debug.Log("Finished Creating Entities!");
        }
        catch (Exception e) {
            Debug.LogException(e);
            Abort("Failed to Create Entities.");
        }
    }
    private void SetupEntities() {
        //Setup any special dependencies before game start.
        customizationMenuScript.SetRenderCameraTarget(mainCameraComponent);
        resultsMenuScript.SetRenderCameraTarget(mainCameraComponent);
        //Consider ditching this if possible
    }
    private void CreatePlayers() {
        if (player1 || player2) {
            Debug.LogWarning("Attempted to reinstanciate players!");
            return;
        }

        player1 = Instantiate(loadedAssets["Player"].Result);
        player1.name = "Player1";
        player1.SetActive(false);
        player1NetworkObject = player1.GetComponent<NetworkObject>(); //NOT needed - remove it and add it in runtime!
        player1Script = player1.GetComponent<Player>();
        player1Script.Initialize();
        player1Script.SetPlayerType(Player.PlayerType.PLAYER_1);
        player1Script.SetHUDReference(HUDScript);

        player2 = Instantiate(loadedAssets["Player"].Result);
        player2.name = "Player2";
        player2.SetActive(false);
        player2NetworkObject = player2.GetComponent<NetworkObject>(); //NOT needed
        player2Script = player2.GetComponent<Player>();
        player2Script.Initialize();
        player2Script.SetPlayerType(Player.PlayerType.PLAYER_2);
        player2Script.SetHUDReference(HUDScript);
    }


    //Level Loading-
    public void StartLevel(uint level) {
        if (currentLoadedLevel) {
            Debug.LogError("Attempted to start a level while another level was loaded! \n Unload loaded level first.");
            return;
        }

        currentLoadedLevelHandle = Addressables.LoadAssetAsync<GameObject>(levelsBundle.levels[level].asset);
        currentLoadedLevelHandle.Completed += LevelLoadedCallback;
        StartLoadingScreenProcess(LoadingScreenProcess.LOADING_LEVEL);
    }
    private void UnloadCurrentLevel() {
        currentLoadedLevelScript.ReleaseResources();
        Destroy(currentLoadedLevel);
        if (currentLoadedLevelHandle.IsValid()) //???? wot
            Addressables.Release(currentLoadedLevelHandle);

        currentLoadedLevel = null;
        currentLoadedLevelScript = null;
    }
    private void StartLoadingScreenProcess(LoadingScreenProcess process) {
        if (process == LoadingScreenProcess.NONE)
            return;

        currentLoadingScreenProcess = process;
        SetupLoadingScreenState();
    }
    private void CompleteLoadingScreenProcess(LoadingScreenProcess process) {
        if (currentLoadingScreenProcess != process)
            return;

        currentLoadingScreenProcess = LoadingScreenProcess.NONE;
        loadingScreen.SetActive(false);
    }
    //


    //Networking-
    public void StartAsHost() {
        currentConnectionState = ConnectionState.HOST;
        networkManagerScript.StartHost();
    }
    public void StartAsClient() {
        currentConnectionState = ConnectionState.CLIENT;
        networkManagerScript.StartClient();
    }
    private void StopNetworking() {
        networkManagerScript.Shutdown();
        networkManager.SetActive(false);
        clientID = -1;
        currentConnectionState = ConnectionState.NONE;
        currentGameMode = GameMode.NONE;

        connectedClients = 0;

        player1 = null;
        player2 = null;
        player1Script = null;
        player2Script = null;
        player1NetworkObject = null;
        player2NetworkObject = null;
        rpcManager = null;
        rpcManagerScript = null;

        connectionMenuScript.SetConnectionMenuMode(ConnectionMenu.ConnectionMenuMode.NORMAL);
        customizationMenuScript.SetCustomizationMenuMode(CustomizationMenu.CustomizationMenuMode.NORMAL);
        levelSelectMenuScript.SetLevelSelectMenuMode(LevelSelectMenu.LevelSelectMenuMode.NORMAL);
    }
    private bool AddClient(ulong id) {
        if (connectedClients == 2) {
            Debug.LogWarning("Unable to add client \n Maximum clients limit reached!");
            //TODO: THIS WAS TRIGGERED! Check the connectedClients reset!
            return false;
        }

        if (networkManagerScript.ConnectedClients.Count == 0) {
            player1 = Instantiate(loadedAssets["Player"].Result);
            player1.name = "Player1";
            //player1.SetActive(false);
            player1Script = player1.GetComponent<Player>();
            player1Script.Initialize();
            player1Script.SetPlayerType(Player.PlayerType.PLAYER_1);
            player1Script.SetHUDReference(HUDScript);
            player1NetworkObject = player1.GetComponent<NetworkObject>();
            player1NetworkObject.SpawnWithOwnership(id);

            //Adds RpcManager as well
            rpcManager = Instantiate(loadedAssets["RpcManager"].Result);
            rpcManagerScript = rpcManager.GetComponent<RpcManager>(); //Need this when im not host!
            rpcManagerScript.Initialize();
            rpcManager.GetComponent<NetworkObject>().Spawn();
        } 
        else if (networkManagerScript.ConnectedClients.Count == 1) {
            player2 = Instantiate(loadedAssets["Player"].Result);
            player2.name = "Player2";
            //player2.SetActive(false); //This breaks it - causes mismatching
            player2Script = player2.GetComponent<Player>();
            player2Script.Initialize();
            player2Script.SetPlayerType(Player.PlayerType.PLAYER_2);
            player2Script.SetHUDReference(HUDScript);
            player2NetworkObject = player2.GetComponent<NetworkObject>();
            player2NetworkObject.SpawnWithOwnership(id);
        }

        connectedClients++;
        return true;
    }
    public ulong GetClientID() {
        return (ulong)clientID;
    }


    private void ClientDisconnectedCallback(ulong id) {
        Debug.Log("Client has disconnected! Returning to main menu");

        StopNetworking();
        UnloadCurrentLevel();
        SetGameState(GameState.MAIN_MENU);
    }
    private void ClientApprovalCallback(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response) {
        if (!networkManagerScript.IsHost)
            return;

        Debug.Log("Received connection request from Client " + request.ClientNetworkId);
        if (AddClient(request.ClientNetworkId)) {
            response.CreatePlayerObject = false;
            response.Approved = true;
        } else {
            response.Reason = "Maximum players limit reached!";
            response.Approved = false;
        }
    }
    private void ClientConnectedCallback(ulong id) {
        Debug.Log("Client Connected!");
        if (clientID == -1)
            clientID = (long)id;

        if (networkManagerScript.IsServer) {
            if (networkManagerScript.ConnectedClients.Count == 2) {
                rpcManagerScript.RelayRpcManagerReferenceClientRpc(rpcManager);
                ClientRpcParams clientRpcParams = new ClientRpcParams();
                clientRpcParams.Send = new ClientRpcSendParams { TargetClientIds = new ulong[] { player2NetworkObject.OwnerClientId } };
                rpcManagerScript.RelayPlayerReferenceClientRpc(player1, Player.PlayerType.PLAYER_1, clientRpcParams);
                rpcManagerScript.RelayPlayerReferenceClientRpc(player2, Player.PlayerType.PLAYER_2, clientRpcParams);
                //HIDE SPRITES?


                rpcManagerScript.ProccedToCustomizationMenuClientRpc();
                Debug.Log("All players connected.");
            }
        }
    }


    public void SetReceivedRpcManagerRef(NetworkObjectReference reference) {
        rpcManager = reference;
        rpcManagerScript = rpcManager.GetComponent<RpcManager>();
        rpcManagerScript.Initialize();
    }
    public void SetReceivedRpcPlayerRef(NetworkObjectReference reference, Player.PlayerType type) {
        if (type == Player.PlayerType.NONE) {
            Debug.LogWarning("Received player reference for type none!");
            return;
        }


        if (type == Player.PlayerType.PLAYER_1) {
            player1 = reference;
            player1.name = "Player1";
            player1NetworkObject = player1.GetComponent<NetworkObject>();
            player1Script = player1.GetComponent<Player>();
            player1Script.Initialize();
            player1Script.SetPlayerType(Player.PlayerType.PLAYER_1);
            player1Script.SetHUDReference(HUDScript);
            //HIDE SPRITE?
        } else if (type == Player.PlayerType.PLAYER_2) {
            player2 = reference;
            player2.name = "Player2";
            player2NetworkObject = player2.GetComponent<NetworkObject>();
            player2Script = player2.GetComponent<Player>();
            player2Script.Initialize();
            player2Script.SetPlayerType(Player.PlayerType.PLAYER_2);
            player2Script.SetHUDReference(HUDScript);
            //HIDE SPRITE?
        }
    }
    //



    public void SetGameModeSelection(GameMode mode) {
        if (mode == GameMode.NONE)
            return;

        currentGameMode = mode;
        if (mode == GameMode.COOP) {
            CreatePlayers();
            networkManager.SetActive(false);
            customizationMenuScript.SetCustomizationMenuMode(CustomizationMenu.CustomizationMenuMode.NORMAL);
            levelSelectMenuScript.SetLevelSelectMenuMode(LevelSelectMenu.LevelSelectMenuMode.NORMAL);
        } else if (mode == GameMode.LAN) {
            networkManager.SetActive(true);
            customizationMenuScript.SetCustomizationMenuMode(CustomizationMenu.CustomizationMenuMode.ONLINE);
            levelSelectMenuScript.SetLevelSelectMenuMode(LevelSelectMenu.LevelSelectMenuMode.ONLINE);
        }
    }
    public void SetCharacterSelection(Player.PlayerType type, PlayerCharacterData data, Color color) {
        if (type == Player.PlayerType.NONE)
            return;

        if (type == Player.PlayerType.PLAYER_1) {
            player1Script.SetPlayerData(data);
            player1Script.SetPlayerColor(color);
        }
        else if (type == Player.PlayerType.PLAYER_2) {
            player2Script.SetPlayerData(data);
            player2Script.SetPlayerColor(color);
        }
    }



    //FIX INIFINITE RELOADING OF ASSETS IN CASE OF ERROR! initialization loop
    public void SetGameState(GameState state) {
        switch (state) {
            case GameState.MAIN_MENU: {
                    if (matchDirectorScript.HasMatchStarted()) {
                        Debug.LogWarning("You cant use SetGameState to quit a match. \n Use QuitMatch() instead!");
                        return;
                    }
                    else
                        transitionMenuScript.StartTransition(SetupMainMenuState);
                }
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
                Debug.LogWarning("You cant use SetGameState to transition into a pause menu \n Use PauseGame/UnpauseGame instead!");
                break;
            case GameState.RESULTS_MENU:
                transitionMenuScript.StartTransition(SetupResultsMenuState);
                break;
            case GameState.PLAYING:
                transitionMenuScript.StartTransition(SetupPlayState);
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
        connectionMenuScript.SetConnectionMenuMode(ConnectionMenu.ConnectionMenuMode.NORMAL);
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
        levelSelectMenu.SetActive(true);
        currentGameState = GameState.LEVEL_SELECT_MENU;
        if (currentGameMode == GameMode.LAN && networkManagerScript.IsHost)
            RandomizeLevelSelectionChoice();
    }
    private void SetupLoadingScreenState() {
        HideAllMenus();
        DisableMouseCursor();
        loadingScreen.SetActive(true);
        currentGameState = GameState.LOADING_SCREEN;
    }
    private void SetupResultsMenuState() {
        //No idea about online here yet!
        //Director should call this when it detects a win!
        HideAllMenus();
        EnableMouseCursor();
        resultsMenuScript.StartReturnTimer(12.0f); //Move to some value. Maybe in the menu itself! prefab serializedfield
        resultsMenuScript.SetWinner(matchDirectorScript.GetWinner());
        resultsMenuScript.SetPlayerPortrait(Player.PlayerType.PLAYER_1, player1Script.GetPlayerData().portraitSprite);
        resultsMenuScript.SetPlayerPortrait(Player.PlayerType.PLAYER_2, player2Script.GetPlayerData().portraitSprite);

        //NOTE: On return to main menu, host kicks client! Delete players!

        resultsMenu.SetActive(true);
        currentGameState = GameState.RESULTS_MENU;
    }




    public void PauseGame() {
        if (countdownMenuScript.IsAnimationPlaying())
            return;

        HideAllMenus();
        EnableMouseCursor();
        currentGameState = GameState.PAUSE_MENU;
        HUD.gameObject.SetActive(false);
        pauseMenu.SetActive(true);
        //Careful online disconnection
        player1Script.DisableInput();
        player2Script.DisableInput();

        if (currentGameMode == GameMode.COOP)
            Time.timeScale = 0.0f;
    }
    public void UnpauseGame() {
        HideAllMenus();
        DisableMouseCursor();
        currentGameState = GameState.PLAYING;
        HUD.gameObject.SetActive(true);
        pauseMenu.SetActive(false);
        //Careful online disconnection
        player1Script.EnableInput();
        player2Script.EnableInput();

        if (currentGameMode == GameMode.COOP)
            Time.timeScale = 1.0f;
    }

    //API for quiting matches instead of setgamestate to mainmenu - Put check in setgamestate in case match was running?

    //Called by match director to dictate the state of the game
    //StartMatch is the very start of a match
    //Something for round? 
    //EndMatch is when match is finished and we going to results
    //QuitMatch is when the match is interrupted by disconnection or quiting through pause menu

    //This is the start
    private void SetupPlayState() {
        HideAllMenus();
        DisableMouseCursor();
        //Call start match? 
        //It calls start match on director which resets his data, and starts properly
        //Maybe call transition first using countdown and that calls startmatch

        //Note: Spawn points are decided at match start!
        if (currentGameMode == GameMode.COOP) {
            player1Script.SetSpawnPoint(currentLoadedLevelScript.GetPlayer1SpawnPoint());
            player2Script.SetSpawnPoint(currentLoadedLevelScript.GetPlayer2SpawnPoint());
        }
        else if (currentGameMode == GameMode.LAN) {
            if (networkManagerScript.IsHost) {
                //SendRPC
            }
            else if (networkManagerScript.IsClient) {
                //Nothing?
            }
        }

        matchDirector.SetActive(true); //Should set to false when match over


        SetupRoundStartState();
        countdownMenuScript.StartAnimation(StartMatch);
        currentGameState = GameState.PLAYING;
    }

    //These two are a lot simpler to look at now!!! 
    private void SetupRoundStartState() {
        //This func needs a online version!

        //IMPORTANT NOTE: I could reuse those all over the place honsetly and do the online version once!


        player1Script.DisableInput(); //Kinda redundant but at least it disables the monitoring of the input by the input system
        player2Script.DisableInput();

        //This part needs online version probably with disabling sprites instead. But physics tho? Disable rigidbody too then? seems to be what online solutions say!
        player1.SetActive(false);
        player2.SetActive(false);

        player1Script.SetupStartState(); //This also sets sprite visibility internally! should be used to reset after death!
        player2Script.SetupStartState();

        HUD.SetActive(false); //?? semeed messing from the grouo dowon there

        //Disable round timer either here or before this is called! im just worried since this one is used all over the place later!

        //ResetLevelSpawners and ResetPlayers (Pickups, health, speed, direction, etc)
    }
    public void StartNewRound() {
        SetupRoundStartState();
        countdownMenuScript.StartAnimation(StartRound);
    }
    public void StartRound() {
        //Need online version!

        matchDirectorScript.SetRoundTimerState(true); //Needed here!

        player1Script.EnableInput();
        player2Script.EnableInput();

        player1.SetActive(true);
        player2.SetActive(true);

        HUD.SetActive(true);
    }

    //StartMatch is called by countdown callback. It calls director startmatch
    //EndMatch is called by director when match has concluded
    //QuitMatch is called by game instance when disconnected or quit through pause menu. It calls director to quit game too.
    private void StartMatch() {

        Debug.Log("Match started!");
        matchDirectorScript.StartMatch();
        StartRound();
    }
    public void EndMatch() {

        matchDirector.SetActive(false); // I THINK?

        player1Script.DisableInput();
        player2Script.DisableInput();

        //Online stuff!
        player1.SetActive(false);
        player2.SetActive(false);

        HUD.SetActive(false);

        //FINAL NOTES ON THIS MATCH STUFF
        //It works but there are minor stuff that needs fixing like timer starting on countdown start and it being visible during start countdown!
        //BUG: Player 1 got points when player 1 was the one who died! And the score was correct! at the results menu! Something is reversed!

        UnloadCurrentLevel(); //this is here! and in quit!
        SetGameState(GameState.RESULTS_MENU);
        //ToResults menu!
        //Stops all match related code
    }
    public void QuitMatch() {

        matchDirectorScript.QuitMatch(); //Mandatory
        matchDirector.SetActive(false); // I THINK?

        //General
        //SetStateToMainMenu
        //Reset GameMode
        //matchStarted = false; ON DISCONNECTION TOO!
        //Unload Level!

        //COOP
        //Delete both players
        //Reset GameMode

        //Online
        //Stop Networking
        //

        //To main menu!
    }


    public void RegisterPlayerDeath(Player.PlayerType type) {
        if (type == Player.PlayerType.NONE)
            return;


        //IMPORTANT NOTE: Its possible to trigger multiple death registries somehow! this could end the match immediately! FIX THIS!

        matchDirectorScript.ScorePoint(type);
    }




    private void RandomizeLevelSelectionChoice() { 
        int rand = UnityEngine.Random.Range(0, networkManagerScript.ConnectedClients.Count);

        if (rand == 0)
            levelSelectMenuScript.ActivateStartButton();
        else if (rand == 1) {
            ClientRpcParams clientRpcParams = new ClientRpcParams();
            clientRpcParams.Send = new ClientRpcSendParams { TargetClientIds = new ulong[] { player2NetworkObject.OwnerClientId } };
            rpcManagerScript.RelayLevelSelectorRoleClientRpc(clientRpcParams);
        }
    }
    private void HideAllMenus() {
        //Add all menus here!
        mainMenu.SetActive(false);
        customizationMenu.SetActive(false);
        levelSelectMenu.SetActive(false);
        settingsMenu.SetActive(false);
        gameModeMenu.SetActive(false);
        countdownMenu.SetActive(false);
        loadingScreen.SetActive(false);
        connectionMenu.SetActive(false);
        pauseMenu.SetActive(false);
    }
    private void EnableMouseCursor()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
    private void DisableMouseCursor()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }


    public GameSettings GetGameSettings() {
        return gameSettings;
    }
    public RpcManager GetRpcManagerScript() {
        return rpcManagerScript;
    }
    public CustomizationMenu GetCustomizationMenuScript() {
        return customizationMenuScript;
    }
    public LevelSelectMenu GetLevelSelectMenuScript() {  
        return levelSelectMenuScript; 
    }
    public Player GetPlayer1Script() {
        return player1Script;
    }
    public Player GetPlayer2Script() {
        return player2Script;  
    }


    public PlayerCharactersBundle GetPlayerCharactersBundle() {
        return playerCharactersBundle;
    }
    public LevelsBundle GetLevelsBundle() {
        return levelsBundle;
    }


    private void LevelLoadedCallback(AsyncOperationHandle<GameObject> handle) {
        if (handle.Status == AsyncOperationStatus.Succeeded) {
            Debug.Log("Loaded level " + handle.Result.ToString() + " successfully");
            currentLoadedLevel = Instantiate(handle.Result);
            currentLoadedLevelScript = currentLoadedLevel.GetComponent<Level>();
            currentLoadedLevelScript.Initialize();
            CompleteLoadingScreenProcess(LoadingScreenProcess.LOADING_LEVEL);
            SetupPlayState(); //Direct call to avoid transition.
        }
        else {
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

        gameSettingsIsLoading = false;
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

        playerCharactersBundleIsLoading = false;
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

        assetsBundleIsLoading = false;
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

        assetsBundleIsLoading = false;
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
