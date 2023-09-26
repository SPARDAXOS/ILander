using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine;
using System.Resources;
using static UnityEngine.EventSystems.EventTrigger;
using static UnityEngine.Rendering.VirtualTexturing.Debugging;
using Newtonsoft.Json.Bson;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEditor;

public class GameInstance : MonoBehaviour
{


    //NOTE: Break this into ApplicationStatus and GameStatus

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
        CUSTOMIZATION_MENU,
        PAUSE_MENU,
        PLAYING
    }
    public ApplicationState currentApplicationStatus = ApplicationState.STOPPED;
    public GameState currentGameState = GameState.NONE;


    //Temp public
    private const string gameAssetsBundleKey = "GameAssetsBundle"; //The most pritle part of the loading process.
    private const SettingsMenu.QualityPreset startingQualityPreset = SettingsMenu.QualityPreset.ULTRA; //Put somewhere else? 
    private static GameInstance instance;



    public GameAssetsBundle gameAssetsBundle;
    public Dictionary<string, AsyncOperationHandle<GameObject>> loadedAssets = new Dictionary<string, AsyncOperationHandle<GameObject>>();

    //Sus
    private bool initializationInProgress = false;
    private bool assetsBundleLoadingInProgress = false;
    private bool assetsLoadingInProgress = false;

    private bool assetsLoaded = false;
    private bool gameInitialized = false;



    private GameObject player;
    private GameObject mainCamera;
    private GameObject mainMenu;
    private GameObject settingsMenu;
    private GameObject customizationMenu;

    private Player playerScript;
    private MainCamera mainCameraScript;
    private MainMenu mainMenuScript;
    private SettingsMenu settingsMenuScript;
    private CustomizationMenu customizationMenuScript;

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
        if (!gameAssetsBundle) {
            if (assetsBundleLoadingInProgress)
                Debug.Log("Waiting on GameAssetsBundle to load!");
            else
                Debug.LogError("Unable to load assets. \n GameAssetsBundle is missing!");
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
            case GameState.PLAYING:
                UpdatePlayingState();
                break;
        }
    }



    private void UpdatePlayingState() {

        playerScript.Tick();
        mainCameraScript.Tick();
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
        if (gameAssetsBundle) {
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
    }



    private void LoadGameAssetsBundle()
    {
        if (gameAssetsBundle) {
            Debug.Log("Game assets bundle is already loaded!");
            return;
        }
        Debug.Log("Started Loading GameAssetsBundle!");
        AssetLabelReference GameAssetsLabel = new AssetLabelReference
        {
            labelString = gameAssetsBundleKey
        };
        Addressables.LoadAssetAsync<GameAssetsBundle>(GameAssetsLabel).Completed += GameAssetsBundleLoadingCallback;
        assetsBundleLoadingInProgress = true;
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
    private void CreateEntities()
    {
        Debug.Log("Started Creating Entities!");

        player = Instantiate(loadedAssets["Player"].Result);
        player.SetActive(false);
        playerScript = player.GetComponent<Player>();
        playerScript.Initialize();

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

        customizationMenu = Instantiate(loadedAssets["CustomizationMenu"].Result);
        customizationMenu.SetActive(false);
        customizationMenuScript = customizationMenu.GetComponent<CustomizationMenu>();
        customizationMenuScript.Initialize();



        Debug.Log("Finished Creating Entities!");
    }
    private void SetupEntities() {
        mainCameraScript.SetFollowTarget(player); //Deprecated

        customizationMenuScript.SetRenderCameraTarget(mainCameraComponent);
        settingsMenuScript.SetQualityPreset(startingQualityPreset);
    }


    public void SetGameState(GameState state) {
        switch (state) {
            case GameState.MAIN_MENU:
                SetupMainMenuState();
                break;
            case GameState.SETTINGS_MENU:
                SetupSettingsMenuState();
                break;
            case GameState.CUSTOMIZATION_MENU:
                SetupCustomizationMenuState();
                break;
            case GameState.PAUSE_MENU:
                SetupPauseMenuState();
                break;
            case GameState.PLAYING:
                SetupStartState(); //Should set the game in start state as if you just clicked play!
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
    private void SetupCustomizationMenuState() {
        HideAllMenus();
        customizationMenu.SetActive(true);
        Cursor.visible = true;
        currentGameState = GameState.CUSTOMIZATION_MENU;
    }
    private void SetupPauseMenuState() {
        //mainMenu.SetActive(true);
        //Cursor.visible = true;
        //currentGameState = GameState.PAUSE_MENU;
    }

    //Probably need to change name to refelect which state it resets
    private void SetupStartState() {

    }


    private void HideAllMenus()
    {
        //Add all menus here!
        mainMenu.SetActive(false);
        customizationMenu.SetActive(false);
        settingsMenu.SetActive(false);
    }


    //NOTE: Maybe move to helper class
    public static void Clamp(ref float target, float min, float max)
    {
        if (target > max)
            target = max;
        if (target < min)
            target = min;
    }
    public static void Validate(object target, string errorMessage)
    {
        if (target == null)
            GameInstance.GetInstance().Abort(errorMessage);
    }


    private void GameAssetsBundleLoadingCallback(AsyncOperationHandle<GameAssetsBundle> handle)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            gameAssetsBundle = handle.Result;
            Debug.Log("GameAssetsBundle Loaded Successfully!");
        }
        else {
            Debug.LogError("Failed to load assets bundle! \n exiting..");

            //NOTE:
            //If it fails to load and it is intergral then 
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
        }

        assetsBundleLoadingInProgress = false;
        /*
        //NOTES:
        //This method works.
        //The only thing to keep track of is the GameAssetsBundle key which i should make into a member to make it more important and make sure it is trackable!

        //Although, whats the point of the GameAssetsBundle? i could just load everything in the thing....
        //I think its a better solution than to rely on labeling assets like crazy and making sure they all carry the label i want
        //The bundle method makes sure that if something is wrong then its in the specific bundle. Removing labels makes it more safer for errors such as
        //not marking something with the right label and such

        //Plans
        //Make some sort of key/value pair in a dictionary so i can associate asset handles with assets by keys
        //Either i get the key + AssetReference pair from the SO then i build my dictionary with key + Handle here
        //Or i have 2 lists at GameAssets where i subscribe the key/handle pair or something.
        //First option seems better.
        //My method of loading basically allows for grouping assets without relying too heavily on label and instead grouping using SOs.
        //Cars SO contains only car refs and such...


        //Final Notes:
        //Get AssetEntry from GameAssets
        //Load Asset and use its key to somehow keep in a Dictionary here along with its handle!
        //How to associate both is the problem left
        */
    }
    private void GameObjectLoadingCallback(AsyncOperationHandle<GameObject> handle)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded) {
            Debug.Log("Successfully loaded " + handle.Result.ToString());
        }
        else {
            Debug.LogError("Asset " + handle.ToString() + " failed to load!");

            //NOTE:
            //If it fails to load and it is intergral then 
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
        }
    }
}
