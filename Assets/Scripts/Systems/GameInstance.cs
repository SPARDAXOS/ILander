using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class GameInstance : MonoBehaviour {
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


    private ApplicationState currentApplicationStatus = ApplicationState.STOPPED;
    private GameState currentGameState = GameState.NONE;
    private GameMode currentGameMode = GameMode.NONE;
    private LoadingScreenProcess currentLoadingScreenProcess = LoadingScreenProcess.NONE;

    //Labels in addressables.
    private const string assetsBundleKey           = "MainAssetsBundle"; 
    private const string levelsBundleKey           = "MainLevelsBundle";
    private const string gameSettingsLabel         = "MainGameSettings";
    private const string playerCharactersBundleKey = "MainPlayerCharactersBundle";

    private static GameInstance instance;

    private AsyncOperationHandle<LevelsBundle> levelsBundleHandle;
    private AsyncOperationHandle<AssetsBundle> assetsBundleHandle;
    private AsyncOperationHandle<GameSettings> gameSettingsHandle;
    private AsyncOperationHandle<PlayerCharactersBundle> playerCharactersBundleHandle;
    private AsyncOperationHandle<GameObject> currentLoadedLevelHandle;
    private Dictionary<string, AsyncOperationHandle<GameObject>> loadedAssets = new Dictionary<string, AsyncOperationHandle<GameObject>>();

    private LevelsBundle levelsBundle = null;
    private AssetsBundle assetsBundle = null;
    private GameSettings gameSettings = null;
    private PlayerCharactersBundle playerCharactersBundle = null;

    private GameObject currentLoadedLevel = null;
    private Level currentLoadedLevelScript = null;

    private bool assetsBundleIsLoading = false;
    private bool levelsBundleIsLoading = false;
    private bool gameSettingsIsLoading = false;
    private bool playerCharactersBundleIsLoading = false;

    private bool initializationInProgress = false;
    private bool assetsLoadingInProgress = false;

    private bool assetsLoaded = false;
    private bool gameInitialized = false;
    private bool isPaused = false;

    private uint connectedClients = 0;
    private long clientID = -1;

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
    private GameObject soundManager;

    private Player player1Script;
    private Player player2Script;
    private NetworkObject player1NetworkObject;
    private NetworkObject player2NetworkObject;
    private MainMenu mainMenuScript; //For consistency's sake
    private SettingsMenu settingsMenuScript;
    private GameModeMenu gameModeMenuScript;
    private ConnectionMenu connectionMenuScript;
    private CustomizationMenu customizationMenuScript;
    private LevelSelectMenu levelSelectMenuScript;
    private LoadingScreen loadingScreenScript;
    private TransitionMenu transitionMenuScript;
    private CountdownMenu countdownMenuScript;
    private NetworkManager networkManagerScript;
    private RpcManager rpcManagerScript;
    private HUD HUDScript;
    private PauseMenu pauseMenuScript;
    private ResultsMenu resultsMenuScript;
    private MatchDirector matchDirectorScript;
    private SoundManager soundManagerScript;

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
        ValidateAndDestroy(player1);
        ValidateAndDestroy(player2);
        ValidateAndDestroy(mainCamera);
        ValidateAndDestroy(mainMenu);
        ValidateAndDestroy(settingsMenu);
        ValidateAndDestroy(gameModeMenu);
        ValidateAndDestroy(connectionMenu);
        ValidateAndDestroy(customizationMenu);
        ValidateAndDestroy(levelSelectMenu);
        ValidateAndDestroy(loadingScreen);
        ValidateAndDestroy(transitionMenu);
        ValidateAndDestroy(countdownMenu);
        ValidateAndDestroy(eventSystem);
        ValidateAndDestroy(networkManager);
        ValidateAndDestroy(rpcManager);
        ValidateAndDestroy(HUD);
        ValidateAndDestroy(pauseMenu);
        ValidateAndDestroy(resultsMenu);
        ValidateAndDestroy(matchDirector);
        ValidateAndDestroy(soundManager);

        foreach (var entry in loadedAssets)
            Addressables.Release(entry.Value);

        if (currentLoadedLevel)
            UnloadCurrentLevel();

        Addressables.Release(levelsBundleHandle);
        Addressables.Release(gameSettingsHandle);
        Addressables.Release(playerCharactersBundleHandle);
        Addressables.Release(assetsBundle);
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
    public void Abort(string errorMessage) {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
        Debug.LogError(errorMessage);
#else
        Application.Quit();
#endif
    }

    private void ValidateAndDestroy(GameObject gameObject) {
        if (gameObject)
            Destroy(gameObject);
    }
    public static GameInstance GetGameInstance() {
        return instance;
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
            SetGameState(GameState.MAIN_MENU);
            gameInitialized = true;
            currentApplicationStatus = ApplicationState.RUNNING;
            Debug.Log("Finished Initializing Game!");
        }
    }
    private void UpdateApplicationRunningState() {
        //If any entitis need to be ticked during any of the game states, they should be added here!
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
        matchDirectorScript.Tick();
        
        if (countdownMenuScript.IsAnimationPlaying())
            countdownMenuScript.Tick();

        if (currentLoadedLevel)
            currentLoadedLevelScript.Tick();

        if (player1Script)
            player1Script.Tick();
        if (player2Script)
            player2Script.Tick();
    }
    private void UpdatePlayingFixedState() {
        if (player1Script)
            player1Script.FixedTick();
        if (player2Script)
            player2Script.FixedTick();
    }
    private void UpdateLoadingState() {
        float value = 0.0f;
        if (currentLoadingScreenProcess == LoadingScreenProcess.LOADING_LEVEL)
            value = currentLoadedLevelHandle.PercentComplete;

        loadingScreenScript.SetLoadingBarValue(value);
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
        gameSettingsHandle = Addressables.LoadAssetAsync<GameSettings>(GameSettingsLabel);
        gameSettingsHandle.Completed += GameSettingsLoadingCallback;
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
        playerCharactersBundleHandle = Addressables.LoadAssetAsync<PlayerCharactersBundle>(playerCharactersBundleLabel);
        playerCharactersBundleHandle.Completed += PlayerCharactersBundleLoadingCallback;
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

        levelsBundleHandle = Addressables.LoadAssetAsync<LevelsBundle>(LevelsBundleLabel);
        levelsBundleHandle.Completed += GameLevelsBundleLoadingCallback;
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
        assetsBundleHandle = Addressables.LoadAssetAsync<AssetsBundle>(AssetsLabel);
        assetsBundleHandle.Completed += GameAssetsBundleLoadingCallback;
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

        //Otherwise Memory leak at the first exception thrown.
        try {
            eventSystem = Instantiate(loadedAssets["EventSystem"].Result);

            networkManager = Instantiate(loadedAssets["NetworkManager"].Result);
            networkManagerScript = networkManager.GetComponent<Unity.Netcode.NetworkManager>();
            networkManagerScript.OnClientConnectedCallback += ClientConnectedCallback;
            networkManagerScript.ConnectionApprovalCallback += ClientApprovalCallback;
            networkManagerScript.OnClientDisconnectCallback += ClientDisconnectedCallback;
            networkManager.SetActive(false);

            soundManager = Instantiate(loadedAssets["SoundManager"].Result);
            soundManagerScript = soundManager.GetComponent<SoundManager>();
            soundManagerScript.Initialize();

            mainCamera = Instantiate(loadedAssets["MainCamera"].Result);
            mainCameraComponent = mainCamera.GetComponent<Camera>();

            mainMenu = Instantiate(loadedAssets["MainMenu"].Result);
            mainMenu.SetActive(false);
            mainMenuScript = mainMenu.GetComponent<MainMenu>();

            settingsMenu = Instantiate(loadedAssets["SettingsMenu"].Result);
            settingsMenu.SetActive(false);
            settingsMenuScript = settingsMenu.GetComponent<SettingsMenu>();
            settingsMenuScript.Initialize();

            gameModeMenu = Instantiate(loadedAssets["GameModeMenu"].Result);
            gameModeMenu.SetActive(false);
            gameModeMenuScript = gameModeMenu.GetComponent<GameModeMenu>(); //For consistencies sake

            connectionMenu = Instantiate(loadedAssets["ConnectionMenu"].Result);
            connectionMenu.SetActive(false);
            connectionMenuScript = connectionMenu.GetComponent<ConnectionMenu>();
            connectionMenuScript.Initialize();

            customizationMenu = Instantiate(loadedAssets["CustomizationMenu"].Result);
            customizationMenu.SetActive(false);
            customizationMenuScript = customizationMenu.GetComponent<CustomizationMenu>();
            customizationMenuScript.Initialize();
            customizationMenuScript.SetRenderCameraTarget(mainCameraComponent);

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
            resultsMenuScript.SetRenderCameraTarget(mainCameraComponent);

            matchDirector = Instantiate(loadedAssets["MatchDirector"].Result);
            matchDirector.SetActive(false);
            matchDirectorScript = matchDirector.GetComponent<MatchDirector>();
            matchDirectorScript.Initialize();
        }
        catch (Exception e) {
            Debug.LogException(e);
            Abort("Failed to Create Entities.");
        }

        Debug.Log("Finished Creating Entities!");
    }
    private void CreatePlayers() {
        if (player1 || player2) {
            Debug.LogWarning("Attempted to reinstanciate players!");
            return;
        }

        player1 = Instantiate(loadedAssets["Player"].Result);
        player1.name = "Player1";
        player1.SetActive(false);
        player1NetworkObject = player1.GetComponent<NetworkObject>();
        player1Script = player1.GetComponent<Player>();
        player1Script.Initialize();
        player1Script.SetPlayerType(Player.PlayerType.PLAYER_1);
        player1Script.SetHUDReference(HUDScript);

        player2 = Instantiate(loadedAssets["Player"].Result);
        player2.name = "Player2";
        player2.SetActive(false);
        player2NetworkObject = player2.GetComponent<NetworkObject>();
        player2Script = player2.GetComponent<Player>();
        player2Script.Initialize();
        player2Script.SetPlayerType(Player.PlayerType.PLAYER_2);
        player2Script.SetHUDReference(HUDScript);
    }

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
        if (!currentLoadedLevel) {
            Debug.LogWarning("Attempted to unload a level but currrentLoadedLevel was set to null!");
            return;
        }

        currentLoadedLevelScript.ReleaseResources();
        Destroy(currentLoadedLevel);
        if (currentLoadedLevelHandle.IsValid())
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

    public void StartAsHost() {
        networkManagerScript.StartHost();
    }
    public void StartAsClient() {
        networkManagerScript.StartClient();
    }
    private void StopNetworking() {
        networkManagerScript.Shutdown();
        networkManager.SetActive(false);

        clientID = -1;
        connectedClients = 0;

        player1 = null;
        player2 = null;
        player1Script = null;
        player2Script = null;
        player1NetworkObject = null;
        player2NetworkObject = null;
        rpcManager = null;
        rpcManagerScript = null;
    }
    private bool AddClient(ulong id) {
        if (connectedClients == 2) {
            Debug.LogWarning("Unable to add client \n Maximum clients limit reached!");
            return false;
        }

        if (networkManagerScript.ConnectedClients.Count == 0) {
            player1 = Instantiate(loadedAssets["Player"].Result);
            player1.name = "Player1";
            player1Script = player1.GetComponent<Player>();
            player1Script.Initialize();
            player1Script.SetPlayerType(Player.PlayerType.PLAYER_1);
            player1Script.SetHUDReference(HUDScript);
            player1Script.DeactivateNetworkedEntity();
            player1NetworkObject = player1.GetComponent<NetworkObject>();
            player1NetworkObject.SpawnWithOwnership(id);

            //Adds RpcManager as well
            rpcManager = Instantiate(loadedAssets["RpcManager"].Result);
            rpcManagerScript = rpcManager.GetComponent<RpcManager>();
            rpcManager.GetComponent<NetworkObject>().Spawn();
        } 
        else if (networkManagerScript.ConnectedClients.Count == 1) {
            player2 = Instantiate(loadedAssets["Player"].Result);
            player2.name = "Player2";
            player2Script = player2.GetComponent<Player>();
            player2Script.Initialize();
            player2Script.SetPlayerType(Player.PlayerType.PLAYER_2);
            player2Script.SetHUDReference(HUDScript);
            player2Script.DeactivateNetworkedEntity();
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
        if (matchDirectorScript.HasMatchStarted()) {
            Debug.Log("Client has disconnected! Returning to main menu");
            QuitMatch();
        }
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
                rpcManagerScript.ProccedToCustomizationMenuClientRpc();
                Debug.Log("All players connected. \n Proceeding to CustomizationMenu.");
            }
        }
    }

    public bool IsHost() {
        return networkManagerScript.IsHost;
    }
    public bool IsClient() {
        return networkManagerScript.IsClient;
    }

    public void SetReceivedRpcManagerRef(NetworkObjectReference reference) {
        rpcManager = reference;
        rpcManagerScript = rpcManager.GetComponent<RpcManager>();
    }
    public void SetReceivedPlayerRefRpc(NetworkObjectReference reference, Player.PlayerType type) {
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
            player1Script.DeactivateNetworkedEntity();
        } 
        else if (type == Player.PlayerType.PLAYER_2) {
            player2 = reference;
            player2.name = "Player2";
            player2NetworkObject = player2.GetComponent<NetworkObject>();
            player2Script = player2.GetComponent<Player>();
            player2Script.Initialize();
            player2Script.SetPlayerType(Player.PlayerType.PLAYER_2);
            player2Script.SetHUDReference(HUDScript);
            player2Script.DeactivateNetworkedEntity();
        }
    }
    public void SetReceivedPlayerSpawnPointRpc(Vector3 position) {
        player1Script.SetSpawnPoint(position);
    }

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

        //The visuals on te customization menu is inverted on the client to show client as player 1 in their own game.
        if (currentGameMode == GameMode.COOP || networkManagerScript.IsHost) {
            if (type == Player.PlayerType.PLAYER_1) {
                player1Script.SetPlayerData(data);
                player1Script.SetPlayerColor(color);
            }
            else if (type == Player.PlayerType.PLAYER_2) {
                player2Script.SetPlayerData(data);
                player2Script.SetPlayerColor(color);
            }
        }
        else if (networkManagerScript.IsClient) {
            if (type == Player.PlayerType.PLAYER_1) {
                player2Script.SetPlayerData(data);
                player2Script.SetPlayerColor(color);
            }
            else if (type == Player.PlayerType.PLAYER_2) {
                player1Script.SetPlayerData(data);
                player1Script.SetPlayerColor(color);
            }
        }
    }
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
        HideAllMenus();
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
        customizationMenuScript.SetupStartState();
        customizationMenu.SetActive(true);
        currentGameState = GameState.CUSTOMIZATION_MENU;
    }
    private void SetupLevelSelectMenuState() {
        HideAllMenus();
        EnableMouseCursor();
        levelSelectMenuScript.SetupStartState();
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
        HideAllMenus();
        EnableMouseCursor();
        resultsMenuScript.StartReturnTimer();
        resultsMenuScript.SetWinner(matchDirectorScript.GetWinner());
        resultsMenuScript.SetPlayerPortrait(Player.PlayerType.PLAYER_1, player1Script.GetPlayerData().portraitSprite);
        resultsMenuScript.SetPlayerPortrait(Player.PlayerType.PLAYER_2, player2Script.GetPlayerData().portraitSprite);

        resultsMenu.SetActive(true);
        currentGameState = GameState.RESULTS_MENU;
    }

    public void PauseGame() {
        if (countdownMenuScript.IsAnimationPlaying() || currentGameState != GameState.PLAYING)
            return;

        EnableMouseCursor();
        HUD.gameObject.SetActive(false);
        pauseMenu.SetActive(true);

        if (currentGameMode == GameMode.COOP) {
            player1Script.DisableInput();
            player2Script.DisableInput();
            matchDirectorScript.SetRoundTimerState(false);
            Time.timeScale = 0.0f;
        }
        else if (currentGameMode == GameMode.LAN) {
            if (IsHost())
                player1Script.DisableInput();
            else if (IsClient())
                player2Script.DisableInput();
        }

        isPaused = true;
    }
    public void UnpauseGame() {
        DisableMouseCursor();
        HUD.gameObject.SetActive(true);
        pauseMenu.SetActive(false);

        if (currentGameMode == GameMode.COOP) {
            player1Script.EnableInput();
            player2Script.EnableInput();
            matchDirectorScript.SetRoundTimerState(true);
            Time.timeScale = 1.0f;
        }
        else if (currentGameMode == GameMode.LAN) {
            if (IsHost())
                player1Script.EnableInput();
            else if (IsClient())
                player2Script.EnableInput();
        }

        isPaused = false;
        currentGameState = GameState.PLAYING;
    }

    private void SetupPlayState() {
        HideAllMenus();
        DisableMouseCursor();

        if (currentGameMode == GameMode.COOP) {
            player1Script.SetActiveControlScheme(Player.PlayerType.PLAYER_1);
            player2Script.SetActiveControlScheme(Player.PlayerType.PLAYER_2);
            player1Script.SetSpawnPoint(currentLoadedLevelScript.GetPlayer1SpawnPoint());
            player2Script.SetSpawnPoint(currentLoadedLevelScript.GetPlayer2SpawnPoint());
        }
        else if (currentGameMode == GameMode.LAN) {
            if (networkManagerScript.IsHost) {
                player1Script.SetActiveControlScheme(Player.PlayerType.PLAYER_1);
                player2Script.SetActiveControlScheme(Player.PlayerType.PLAYER_2);
                RandomizePlayerSpawnPoints();
            }
            else if (networkManagerScript.IsClient) {
                player1Script.SetActiveControlScheme(Player.PlayerType.PLAYER_2);
                player2Script.SetActiveControlScheme(Player.PlayerType.PLAYER_1);
            }
        }

        matchDirector.SetActive(true);
        SetupRoundStartState();
        countdownMenuScript.StartAnimation(StartMatch);
        currentGameState = GameState.PLAYING;
    }
    private void SetupRoundStartState() {
        if (currentGameMode == GameMode.COOP) {
            player1.SetActive(false);
            player2.SetActive(false);
        }
        else if (currentGameMode == GameMode.LAN) {
            player1Script.DeactivateNetworkedEntity();
            player2Script.DeactivateNetworkedEntity();
            if (IsHost()) {
                player1.transform.position = player1Script.GetSpawnPoint();
                player2.transform.position = player2Script.GetSpawnPoint();
            }
        }

        player1Script.DisableInput();
        player2Script.DisableInput();
        player1Script.SetupStartState();
        player2Script.SetupStartState();

        HUD.SetActive(false);
        matchDirectorScript.SetRoundTimerVisibility(false);
        matchDirectorScript.SetRoundTimerState(false);
    }
    public void StartNewRound() {
        if (isPaused)
            UnpauseGame();
        SetupRoundStartState();
        countdownMenuScript.StartAnimation(StartRound);
    }
    public void StartRound() {
        if (IsHost())
            currentLoadedLevelScript.RefreshAllPickupSpawns();

        if (currentGameMode == GameMode.COOP) {
            player1.SetActive(true);
            player2.SetActive(true);
            player1Script.EnableInput();
            player2Script.EnableInput();
        }
        else if (currentGameMode == GameMode.LAN) {
            player1Script.ActivateNetworkedEntity();
            player2Script.ActivateNetworkedEntity();

            if (IsHost())
                player1Script.EnableInput();
            else if (IsClient())
                player2Script.EnableInput();
        }

        matchDirectorScript.SetRoundTimerState(true);
        matchDirectorScript.SetRoundTimerVisibility(true);
        HUD.SetActive(true);
    }


    //StartMatch() is called by countdown callback. It calls MatchDirector StartMatch().
    //EndMatch() is called by MatchDirector when match has concluded.
    //QuitMatch() is called by GameInstance when disconnected or quit through pause menu. It calls MatchDirector QuitMatch() too.
    private void StartMatch() {
        matchDirectorScript.StartMatch();
        StartRound();
    }
    public void EndMatch() {
        if (isPaused)
            UnpauseGame();

        if (currentGameMode == GameMode.COOP) {
            player1.SetActive(false);
            player2.SetActive(false);
            player1Script.DisableInput();
            player2Script.DisableInput();
        }
        else if (currentGameMode == GameMode.LAN) {
            player1Script.DeactivateNetworkedEntity();
            player2Script.DeactivateNetworkedEntity();
            if (IsHost())
                player1Script.DisableInput();
            else if (IsClient())
                player2Script.DisableInput();
        }

        matchDirector.SetActive(false);
        HUD.SetActive(false);
        
        UnloadCurrentLevel();
        SetGameState(GameState.RESULTS_MENU);
    }
    public void QuitMatch() {
        if (isPaused)
            UnpauseGame();

        matchDirectorScript.QuitMatch();
        matchDirector.SetActive(false);
        HUD.SetActive(false);

        UnloadCurrentLevel(); 
        RestartGameState();
    }
    public void RestartGameState() {
        if (currentGameMode == GameMode.COOP) {
            Destroy(player1);
            Destroy(player2);

            player1 = null;
            player2 = null;
            player1Script = null;
            player2Script = null;
            player1NetworkObject = null;
            player2NetworkObject = null;
        } 
        else if (currentGameMode == GameMode.LAN)
            StopNetworking();

        connectionMenuScript.SetConnectionMenuMode(ConnectionMenu.ConnectionMenuMode.NORMAL);
        customizationMenuScript.SetCustomizationMenuMode(CustomizationMenu.CustomizationMenuMode.NORMAL);
        levelSelectMenuScript.SetLevelSelectMenuMode(LevelSelectMenu.LevelSelectMenuMode.NORMAL);

        currentGameMode = GameMode.NONE;
        SetGameState(GameState.MAIN_MENU);
    }

    public void RegisterPlayerDeath(Player.PlayerType type) {
        if (type == Player.PlayerType.NONE)
            return;

        if (type == Player.PlayerType.PLAYER_1)
            matchDirectorScript.ScorePoint(Player.PlayerType.PLAYER_2);
        if (type == Player.PlayerType.PLAYER_2)
            matchDirectorScript.ScorePoint(Player.PlayerType.PLAYER_1);
    }
    private void RandomizePlayerSpawnPoints() {
        int rand = UnityEngine.Random.Range(0, 2);
        ClientRpcParams clientRpcParams = new ClientRpcParams();
        clientRpcParams.Send = new ClientRpcSendParams { TargetClientIds = new ulong[] { player2NetworkObject.OwnerClientId } };

        Vector3 player1SpawnPoint = currentLoadedLevelScript.GetPlayer1SpawnPoint();
        Vector3 player2SpawnPoint = currentLoadedLevelScript.GetPlayer2SpawnPoint();

        if (rand == 0) {
            player1Script.SetSpawnPoint(player1SpawnPoint);
            player2Script.SetSpawnPoint(player2SpawnPoint);
            rpcManagerScript.RelayPlayerSpawnPositionClientRpc(player2SpawnPoint, clientRpcParams);
        }
        else if (rand == 1) {
            player1Script.SetSpawnPoint(player2SpawnPoint);
            player2Script.SetSpawnPoint(player1SpawnPoint);
            rpcManagerScript.RelayPlayerSpawnPositionClientRpc(player1SpawnPoint, clientRpcParams);
        }
    }
    private void RandomizeLevelSelectionChoice() { 
        int rand = UnityEngine.Random.Range(0, 2);

        if (rand == 0)
            levelSelectMenuScript.ActivateStartButton();
        else if (rand == 1) {
            ClientRpcParams clientRpcParams = new ClientRpcParams();
            clientRpcParams.Send = new ClientRpcSendParams { TargetClientIds = new ulong[] { player2NetworkObject.OwnerClientId } };
            rpcManagerScript.RelayLevelSelectorRoleClientRpc(clientRpcParams);
        }
    }
    private void HideAllMenus() {

        mainMenu.SetActive(false);
        customizationMenu.SetActive(false);
        levelSelectMenu.SetActive(false);
        settingsMenu.SetActive(false);
        gameModeMenu.SetActive(false);
        countdownMenu.SetActive(false);
        loadingScreen.SetActive(false);
        connectionMenu.SetActive(false);
        pauseMenu.SetActive(false);
        resultsMenu.SetActive(false);
    }
    private void EnableMouseCursor() {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
    private void DisableMouseCursor() {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public GameMode GetCurrentGameMode() {
        return currentGameMode;
    }
    public GameSettings GetGameSettings() {
        return gameSettings;
    }
    public RpcManager GetRpcManagerScript() {
        return rpcManagerScript;
    }
    public MatchDirector GetMatchDirector() {
        return matchDirectorScript;
    }
    public NetworkManager GetNetworkManagerScript() {
        return networkManagerScript;
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
    public Level GetCurrentLevelScript() {
        return currentLoadedLevelScript;
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
            currentLoadedLevelScript.Initialize(currentGameMode);
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
