using Initialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameInstance;
using ILanderUtility;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using static UnityEngine.Rendering.VirtualTexturing.Debugging;

public class Level : MonoBehaviour
{
    private enum LevelState {
        NONE = 0,
        INITIALIZING,
        ACTIVE
    }

    //SerializedField Pickups respawn time?
    [SerializeField] private PickupsBundle pickupsBundle;

    private LevelState currentLevelState = LevelState.NONE;
    private bool initialized = false;
    private bool assetsLoaded = false;

    private Vector3 player1SpawnPoint;
    private Vector3 player2SpawnPoint;

    private uint pickupsSpawnPointsCount = 0;

    private Transform[] pickupsSpawnPoints;
    private bool[] occupiedPickupsSpawnPoints;

    private Dictionary<string, AsyncOperationHandle<GameObject>> loadedPickupAssets;
    public List<string> assetsNames;
    public List<GameObject> pickupsPool;

    public void Initialize() {
        if (initialized)
            return;

        SetupReferences();
        if (pickupsBundle)
            LoadPickupsAssets();
        else
            currentLevelState = LevelState.ACTIVE;


        //Questionable- Maybe do these after they all load! but what if there is nothing to load!
        initialized = true;
    }
    public void Tick() {
        if (!initialized) {
            Debug.LogError("Attempted to tick uninitialized entity " + gameObject.name);
            return;
        }

        if (currentLevelState == LevelState.INITIALIZING)
            CheckAssetsLoadingStatus();
        else if (currentLevelState == LevelState.ACTIVE && assetsLoaded) //If there are any pickups? here or in it!
            UpdatePickupsSpawns();
    }
    private void SetupReferences() {

        Transform player1SpawnPositionTransform = transform.Find("Player1SpawnPoint");
        if (!Utility.Validate(player1SpawnPositionTransform, "No Player1SpawnPoint was found in level " + gameObject.name, false))
            player1SpawnPoint = Vector3.zero;
        else
            player1SpawnPoint = player1SpawnPositionTransform.position;

        Transform player2SpawnPositionTransform = transform.Find("Player2SpawnPoint");
        if (!Utility.Validate(player2SpawnPositionTransform, "No Player2SpawnPoint was found in level " + gameObject.name, false))
            player2SpawnPoint = Vector3.zero;
        else
            player2SpawnPoint = player2SpawnPositionTransform.position;


        //PickupPoints - Use amount to figure out Pickup Instansiations
        Transform PickupsSpawnPointsTransform = transform.Find("PickupSpawnPoints");
        pickupsSpawnPointsCount = (uint)PickupsSpawnPointsTransform.childCount;
        pickupsSpawnPoints = new Transform[pickupsSpawnPointsCount];
        occupiedPickupsSpawnPoints = new bool[pickupsSpawnPointsCount];
        for (uint i = 0; i < PickupsSpawnPointsTransform.childCount; i++)
            pickupsSpawnPoints[i] = PickupsSpawnPointsTransform.GetChild((int)i).transform;

    }
    public void ReleaseResources() {
        foreach (var entry in pickupsPool)
            Destroy(entry.gameObject);
        foreach (var entry in loadedPickupAssets)
            Addressables.Release(entry.Value);
    }

    private void LoadPickupsAssets() {
        currentLevelState = LevelState.INITIALIZING;
        loadedPickupAssets = new Dictionary<string, AsyncOperationHandle<GameObject>>(pickupsBundle.pickups.Length);
        assetsNames = new List<string>(pickupsBundle.pickups.Length);
        foreach (var entry in pickupsBundle.pickups) {
            if (entry.name.Length == 0) //Skip empty entries in the bundle.
                continue;

            var handle = Addressables.LoadAssetAsync<GameObject>(entry.asset);
            handle.Completed += PickupAssetLoadedCallback;
            loadedPickupAssets.Add(entry.name, handle);
            Debug.Log("Level " + gameObject.name + " started loading asset " + entry.name);
        }
    }
    private void CheckAssetsLoadingStatus() {
        bool checkResults = true;

        foreach (var entry in loadedPickupAssets) {
            if (entry.Value.Status == AsyncOperationStatus.Failed) {
                loadedPickupAssets.Remove(entry.Key); //Clears failed to load asset entry.
                assetsNames.Remove(entry.Key);
                continue;
            }
            if (entry.Value.Status != AsyncOperationStatus.Succeeded)
                checkResults = false;
        }

        if (checkResults) {
            Debug.Log("Level Finished Loading Assets!");
            CreatePickupsPool();
            RefreshAllPickupSpawns();
            currentLevelState = LevelState.ACTIVE;
            assetsLoaded = true;
        }
    }
    private void PickupAssetLoadedCallback(AsyncOperationHandle<GameObject> handle) {
        if (handle.Status == AsyncOperationStatus.Succeeded)
            Debug.Log("Successfully loaded " + handle.Result.ToString());
        else
            Debug.LogWarning("Asset " + handle.ToString() + " failed to load!");
            //loadedPickupAssets.Remove()
    }
    private void CreatePickupsPool() {
        if (pickupsSpawnPointsCount == 0)
            return;

        pickupsPool = new List<GameObject>((int)pickupsSpawnPointsCount);
        for (uint i = 0; i < pickupsSpawnPointsCount; i++) {
            GameObject Object = Instantiate(loadedPickupAssets[assetsNames[0]].Result);
            var script = Object.GetComponent<Pickup>();
            script.SetActive(false);
            script.SetPickupData();

            pickupsPool.Add(Object);
            //Need Projectiles Data
            //Create All from one type!
            //Morph them into other types by changing their data on demand


        }
    }


    //Callback
    //CheckStatus
    //Two states for level
    //Initializing
    //Active


    public Vector3 GetPlayer1SpawnPoint() {
        return player1SpawnPoint;
    }
    public Vector3 GetPlayer2SpawnPoint() {  
        return player2SpawnPoint;
    }
    


    private void RefreshAllPickupSpawns() {
        //Spawns random stuff all over! Called at start!
    }
    private void UpdatePickupsSpawns() {

    }
}
