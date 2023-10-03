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
    public AssetsBundle assetsBundle = null;
    public GameSettings gameSettings = null;
    public PlayerCharactersBundle playerCharactersBundle = null;
    public Dictionary<string, AsyncOperationHandle<GameObject>> loadedAssets = new Dictionary<string, AsyncOperationHandle<GameObject>>();


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

            networkManager = Instantiate(loadedAssets["NetworkManagder"].Result);
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

        player2 = Instantiate(loadedAssets["Player"].Result);
        player2.name = "Player2";
        player2.SetActive(false);
        player2NetworkObject = player2.GetComponent<NetworkObject>(); //NOT needed
        player2Script = player2.GetComponent<Player>();
        player2Script.Initialize();
        player2Script.SetPlayerType(Player.PlayerType.PLAYER_2);
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

        Destroy(currentLoadedLevel);
        if (currentLoadedLevelHandle.IsValid())
            Addressables.Release(currentLoadedLevelHandle);
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

        if (currentConnectionState == ConnectionState.HOST)
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
            return false;
        }

        if (networkManagerScript.ConnectedClients.Count == 0) {
            player1 = Instantiate(loadedAssets["Player"].Result);
            player1.name = "Player1";
            //player1.SetActive(false);
            player1Script = player1.GetComponent<Player>();
            player1Script.Initialize();
            player1Script.SetPlayerType(Player.PlayerType.PLAYER_1);
            player1NetworkObject = player1.GetComponent<NetworkObject>();
            player1NetworkObject.SpawnWithOwnership(id);

            //Adds RpcManager as well
            rpcManager = Instantiate(loadedAssets["RpcManager"].Result);
            rpcManagerScript = rpcManager.GetComponent<RpcManager>(); //Need this when im not host!
            rpcManagerScript.Initialize();
            rpcManager.GetComponent<NetworkObject>().Spawn();
        } else if (networkManagerScript.ConnectedClients.Count == 1) {
            player2 = Instantiate(loadedAssets["Player"].Result);
            player2.name = "Player2";
            //player2.SetActive(false); //This breaks it - causes mismatching
            player2Script = player2.GetComponent<Player>();
            player2Script.Initialize();
            player2Script.SetPlayerType(Player.PlayerType.PLAYER_2);
            player2NetworkObject = player2.GetComponent<NetworkObject>();
            player2NetworkObject.SpawnWithOwnership(id);
        }

        connectedClients++;
        return true;
    }
    public long GetClientID() {
        return clientID;
    }


    private void ClientDisconnectedCallback(ulong id) {
        Debug.Log("Client has disconnected! Returning to main menu");

        StopNetworking();
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
        } else if (type == Player.PlayerType.PLAYER_2) {
            player2 = reference;
            player2.name = "Player2";
            player2NetworkObject = player2.GetComponent<NetworkObject>();
            player2Script = player2.GetComponent<Player>();
            player2Script.Initialize();
            player2Script.SetPlayerType(Player.PlayerType.PLAYER_2);
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
    public void SetCharacterSelection(Player.PlayerType type, PlayerCharacterData data) {
        if (type == Player.PlayerType.NONE)
            return;

        if (type == Player.PlayerType.PLAYER_1)
            player1Script.SetPlayerData(data);
        else if (type == Player.PlayerType.PLAYER_2)
            player2Script.SetPlayerData(data);
    }



    //FIX INIFINITE RELOADING OF ASSETS IN CASE OF ERROR! initialization loop
    public void SetGameState(GameState state) {
        switch (state) {
            case GameState.MAIN_MENU:
                transitionMenuScript.StartTransition(SetupMainMenuState);
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


    private void SetupRoundStartState() {
        player1Script.DisableInput(); //Kinda redundant but at least it disables the monitoring of the input by the input system
        player2Script.DisableInput();
        player1.SetActive(false);
        player2.SetActive(false);
        player1.transform.position = currentLoadedLevelScript.GetPlayer1SpawnPoint();
        player2.transform.position = currentLoadedLevelScript.GetPlayer2SpawnPoint();
        //ResetLevelSpawners and ResetPlayers (Pickups, health, speed, direction, etc)
    }
    private void SetupPlayState() {
        HideAllMenus();
        DisableMouseCursor();
        SetupRoundStartState();
        countdownMenuScript.StartAnimation(StartMatch);
        currentGameState = GameState.PLAYING;
    }
    private void StartMatch() {
        Debug.Log("Match started!");
        player1Script.EnableInput();
        player2Script.EnableInput();
        player1.SetActive(true);
        player2.SetActive(true);
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
    public RpcManager GetRpcManagerScript()
    {
        return rpcManagerScript;
    }
    public CustomizationMenu GetCustomizationMenuScript()
    {
        return customizationMenuScript;
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
