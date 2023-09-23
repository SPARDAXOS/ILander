using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine;
using System.Resources;
using static UnityEngine.EventSystems.EventTrigger;

public class GameInstance : MonoBehaviour
{
    private enum GameStatus
    {
        STOPPED = 0,
        RUNNING,
        PAUSED,
        MAIN_MENU
    }

    //Temp public
    private const string GameAssetsBundleKey = "GameAssetsBundle"; //The most pritle part of the loading process.
    private static GameInstance instance;
    public GameAssets resources;



    private GameStatus currentStatus = GameStatus.STOPPED;





    private bool resourcesLoaded = false;



    private GameObject player;


    private Player playerScript;


    void Start()
    {
        //OnStart
    }
    void Update()
    {
        switch (currentStatus)
        {
            case GameStatus.STOPPED:
                {

                }break;
            case GameStatus.RUNNING:
                {

                }
                break;
            case GameStatus.PAUSED:
                {

                }
                break;
            case GameStatus.MAIN_MENU:
                {

                }
                break;
        }
        //tick
    }
    private void Awake()
    {
        if (!instance)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            return;
        }

        Debug.LogWarning("Instance of 'GameInstance' already exists!");
        Destroy(gameObject);
    }


    //??? check vid
    public static GameInstance GetInstance()
    {
        return instance;
    }



    public void Initialize()
    {
        //Some check to make sure not to load twice
        //if (!resourcesLoaded)
        //LoadResources();


        AssetLabelReference GameAssetsLabel = new AssetLabelReference
        {
            labelString = GameAssetsBundleKey
        };
        Addressables.LoadAssetAsync<ScriptableObject>(GameAssetsLabel).Completed += ScriptableObjectLoadingCompleted;

        Addressables.LoadAssetsAsync<GameObject>("default", null).Completed += GameInstance_Completed; ;

    }

    private void GameInstance_Completed(AsyncOperationHandle<IList<GameObject>> obj)
    {
        foreach(var entry in obj.Result)
            Debug.Log("Hellop! " + entry.ToString());
    }

    private void OnDestroy()
    {
        
    }



    private void LoadResources()
    {
        foreach (var entry in resources.assets)
        {
            Debug.Log("Started loading asset " + entry.ToString());

            var handle = Addressables.LoadAssetAsync<GameObject>(entry);
            handle.Completed += GameObjectLoadingCompleted;
        }

        //Do loading check and when all bools check out! do this!
        resourcesLoaded = true;
    }
    private void ScriptableObjectLoadingCompleted(AsyncOperationHandle<ScriptableObject> obj)
    {
        Debug.Log("Hellop! " + obj.Result.ToString()); //NOTE: Gets called twice for some reason...

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
    }



    private void GameObjectLoadingCompleted(AsyncOperationHandle<GameObject> handle)
    {

        //Instanciate gameobject out of asset?

        if (handle.Status == AsyncOperationStatus.Succeeded) {
            var go = Instantiate(handle.Result);
            if (go.CompareTag("Player"))
            {
                player = go;
                playerScript = player.GetComponent<Player>();
            }
        }
        else
            Debug.LogError("Asset " + handle.ToString() + " failed to load!");
    }
}
