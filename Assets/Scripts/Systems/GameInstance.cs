using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
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

    //Temp public
    private const string gameAssetsBundleKey = "GameAssetsBundle"; //The most pritle part of the loading process.
    private const string levelsBundleKey = "GameLevelsBundle"; 

    //private const SettingsMenu.QualityPreset startingQualityPreset = SettingsMenu.QualityPreset.ULTRA; //Put somewhere else?  Move to QualitySettings SO as defualt preset!
    private static GameInstance instance;


    private LevelsBundle gameLevelsBundle = null;
    public AssetsBundle gameAssetsBundle = null;
    public Dictionary<string, AsyncOperationHandle<GameObject>> loadedAssets = new Dictionary<string, AsyncOperationHandle<GameObject>>();

    //Terrible names
    private AsyncOperationHandle<GameObject> currentLoadedLevelHandle;
    private GameObject currentLoadedLevel = null;
    private Level currentLoadedLevelScript = null;

    //Sus
    private bool initializationInProgress = false;
    //Shortyen these names
    private bool gameAssetsBundleLoadingInProgress = false;
    private bool gameLevelsBundleLoadingInProgress = false;
    private bool assetsLoadingInProgress = false;

    private bool assetsLoaded = false;
    private bool gameInitialized = false;



    private GameObject player1;
    private GameObject player2;
    private GameObject mainCamera;
    private GameObject mainMenu;
    private GameObject settingsMenu;
    private GameObject gameModeMenu;
    private GameObject customizationMenu;
    private GameObject LevelSelectMenu;
    private GameObject loadingScreen;
    private GameObject transitionMenu;
    private GameObject countdownMenu;
    private GameObject eventSystem;

    private Player player1Script;
    private Player player2Script;
    private MainCamera mainCameraScript;
    private MainMenu mainMenuScript;
    private SettingsMenu settingsMenuScript;
    private GameModeMenu gameModeMenuScript;
    private CustomizationMenu customizationMenuScript;
    private LevelSelectMenu LevelSelectMenuScript;
    private LoadingScreen loadingScreenScript;
    private TransitionMenu transitionMenuScript;
    private CountdownMenu countdownMenuScript;

    private Camera mainCameraComponent;

    void Update() {
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
        //Load AssetsBundle
        //Load LevelsBundle
        //Load Assets
        //Create Entities
        //Setup Entities
        //Setup Main Menu State

        if (!gameAssetsBundle) {
            if (gameAssetsBundleLoadingInProgress)
                Debug.Log("Waiting on GameAssetsBundle to load!");
            else
                Debug.LogError("Unable to load assets. \n GameAssetsBundle is missing!");
            return;
        }
        if (!gameLevelsBundle) {
            if (gameLevelsBundleLoadingInProgress)
                Debug.Log("Waiting on GameLevelsBundle to load!");
            else
                Debug.LogError("Unable to load assets. \n GameLevelsBundle is missing!");
            return;
        }


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



    public void Initialize()
    {
        if (gameAssetsBundle && gameLevelsBundle) { //Assets too? maybe just use the init bool instead?
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
        LoadGameAssetsBundle();
        LoadLevelsBundle();
    }



    private void LoadGameAssetsBundle()
    {
        if (gameAssetsBundle) {
            Debug.Log("GameAssetsBundle is already loaded!");
            return;
        }
        Debug.Log("Started Loading GameAssetsBundle!");
        AssetLabelReference GameAssetsLabel = new AssetLabelReference
        {
            labelString = gameAssetsBundleKey
        };
        Addressables.LoadAssetAsync<AssetsBundle>(GameAssetsLabel).Completed += GameAssetsBundleLoadingCallback;
        gameAssetsBundleLoadingInProgress = true;
    }
    private void LoadLevelsBundle(){
        if (gameLevelsBundle)
        {
            Debug.Log("GameLevelsBundle is already loaded!");
            return;
        }
        Debug.Log("Started Loading GameLevelsBundle!");
        AssetLabelReference LevelsBundleLabel = new AssetLabelReference
        {
            labelString = levelsBundleKey
        };
        Addressables.LoadAssetAsync<LevelsBundle>(LevelsBundleLabel).Completed += GameLevelsBundleLoadingCallback;
        gameLevelsBundleLoadingInProgress = true;
    }
    private void LoadGameAssets()
    {
        Debug.Log("Started Loading Assets!");
        foreach (var entry in gameAssetsBundle.assets) {
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

        //Get this from prefab instead to keep it consistent and allow the user to edit it!
        eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<EventSystem>();
        eventSystem.AddComponent<InputSystemUIInputModule>();

        player1 = Instantiate(loadedAssets["Player"].Result);
        player1.SetActive(false);
        player1Script = player1.GetComponent<Player>();
        player1Script.Initialize();

        player2 = Instantiate(loadedAssets["Player"].Result);
        player2.SetActive(false);
        player2Script = player2.GetComponent<Player>();
        player2Script.Initialize();

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
        LevelSelectMenuScript.SetLevelsBundle(gameLevelsBundle);

        //SetControlSchemes from loaded addressable control shcemes? both players use the same one now.

    }

    //FIX INIFINITE RELOADING OF ASSETS IN CASE OF ERROR! initialization loop
    public void SetGameState(GameState state) {
        switch (state) {
            case GameState.MAIN_MENU:
                SetupMainMenuState();
                break;
            case GameState.SETTINGS_MENU:
                transitionMenuScript.StartTransition(SetupSettingsMenuState);
                break;
            case GameState.GAMEMODE_MENU:
                transitionMenuScript.StartTransition(SetupGameModeMenuState);
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
        mainMenu.SetActive(true);
        Cursor.visible = true;
        currentGameState = GameState.MAIN_MENU;
    }
    private void SetupSettingsMenuState() {
        HideAllMenus();
        settingsMenu.SetActive(true);
        Cursor.visible = true;
        currentGameState = GameState.SETTINGS_MENU;
    }
    private void SetupGameModeMenuState() {
        HideAllMenus(); //hmm, with transition,,,
        gameModeMenu.SetActive(true);
        Cursor.visible = true;
        currentGameState = GameState.GAMEMODE_MENU;
    }
    private void SetupCustomizationMenuState() {
        HideAllMenus();
        customizationMenu.SetActive(true);
        Cursor.visible = true;
        currentGameState = GameState.CUSTOMIZATION_MENU;
    }
    private void SetupLevelSelectMenuState() {
        HideAllMenus();
        LevelSelectMenu.SetActive(true);
        Cursor.visible = true;
        currentGameState = GameState.LEVEL_SELECT_MENU;
    }
    private void SetupLoadingScreenState() {
        HideAllMenus();
        loadingScreen.SetActive(true);
        Cursor.visible = false;
        currentGameState = GameState.LOADING_SCREEN;
    }
    private void SetupPauseMenuState() {
        //mainMenu.SetActive(true);
        Cursor.visible = true;
        currentGameState = GameState.PAUSE_MENU;
    }

    //Probably need to change name to refelect which state it resets - ADD A SECOND ONE? START STATE AND PLAYSSTATE

    //Now it seems like they do the same thing...
    private void SetupLevelStartState() {
        HideAllMenus(); //To turn off loading screen
        Cursor.visible = false;
        //Get data from level and setup start state for players by reseting them and positing them
        //Reset any match score between them
        //Disable their input
        //Start count down at the end
        countdownMenu.SetActive(true);
        countdownMenuScript.StartAnimation(StartMatch);
    }
    private void SetupPlayState() {
        //TEMP
        //Assert if gamemode is none! 


        currentGameState = GameState.PLAYING;
    }



    public void SetGameModeSelection(GameMode mode) {
        currentGameMode = mode;
    }
    public void StartLevel(uint level) {
        if (currentLoadedLevel) {
            Debug.LogError("Attempted to start a level while another level was loaded!");
            return;
        }

        currentLoadedLevelHandle = Addressables.LoadAssetAsync<GameObject>(gameLevelsBundle.levels[level].asset);
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
        player2.SetActive(true);
        player2.transform.position = currentLoadedLevelScript.GetPlayer2SpawnPoint();
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



    //NOTE: Maybe move to helper class
    public static void Clamp(ref float target, float min, float max) {
        if (target > max)
            target = max;
        if (target < min)
            target = min;
    }
    public static bool Validate(object target, string errorMessage, bool abortOnFail = false) {
        if (target == null) {
            if (abortOnFail)
                GetInstance().Abort(errorMessage);
            return false;
        }
        return true;
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
    private void GameAssetsBundleLoadingCallback(AsyncOperationHandle<AssetsBundle> handle) {
        if (handle.Status == AsyncOperationStatus.Succeeded) {
            gameAssetsBundle = handle.Result;
            Debug.Log("GameAssetsBundle Loaded Successfully!");
        }
        else {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Abort("Failed to load assets bundle! \n exiting..");
#endif
        }

        gameAssetsBundleLoadingInProgress = false;
    }
    private void GameLevelsBundleLoadingCallback(AsyncOperationHandle<LevelsBundle> handle) {
        if (handle.Status == AsyncOperationStatus.Succeeded) {
            gameLevelsBundle = handle.Result;
            Debug.Log("GameLevelsBundle Loaded Successfully!");
        }
        else {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Abort("Failed to load levels bundle! \n exiting..");
#endif
        }

        gameAssetsBundleLoadingInProgress = false;
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
