using System.Collections.Generic;
using UnityEngine;
using static GameInstance;
using ILanderUtility;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class Level : MonoBehaviour
{
    private enum LevelState {
        NONE = 0,
        INITIALIZING,
        ACTIVE
    }

    //SerializedField Pickups respawn time?
    [SerializeField] private PickupsBundle pickupsBundle;
    [SerializeField] private float pickupRespawnTimer = 1.0f;
    [SerializeField] private float pickupRespawnRetryTimer = 4.0f;

    private LevelState currentLevelState = LevelState.NONE;
    private bool initialized = false;
    private bool assetsLoaded = false;

    private Vector3 player1SpawnPoint;
    private Vector3 player2SpawnPoint;

    private uint pickupsSpawnPointsCount = 0;

    private Transform[] pickupsSpawnPoints;
    public bool[] occupiedPickupsSpawnPoints;

    private Dictionary<string, AsyncOperationHandle<GameObject>> loadedPickupAssets;
    public List<GameObject> pickupsPool;

    private float currentRespawnTimer = 0.0f;

    public void Initialize() {
        if (initialized)
            return;

        SetupReferences();
        if (pickupsBundle)
            LoadPickupsAssets();
        else
            currentLevelState = LevelState.ACTIVE;

        currentRespawnTimer = pickupRespawnTimer;
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
        else if (currentLevelState == LevelState.ACTIVE && assetsLoaded) {
            UpdateRespawnTimer();
            UpdatePickupsSpawns();
        }
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
        pickupsPool = new List<GameObject>(pickupsBundle.pickups.Length);


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
                continue;
            }
            if (entry.Value.Status != AsyncOperationStatus.Succeeded)
                checkResults = false;
        }

        if (checkResults) {
            Debug.Log("Level Finished Loading Assets!");
            RefreshAllPickupSpawns();
            currentLevelState = LevelState.ACTIVE;
            assetsLoaded = true;
        }
    }
    private void PickupAssetLoadedCallback(AsyncOperationHandle<GameObject> handle) {
        if (handle.Status == AsyncOperationStatus.Succeeded) {
            Debug.Log("Successfully loaded " + handle.Result.ToString());

            var obj = Instantiate(handle.Result);
            Pickup script = obj.GetComponent<Pickup>();
            script.SetActive(false);
            script.SetLevelScript(this);
            pickupsPool.Add(obj);
        }
        else
            Debug.LogWarning("Asset " + handle.ToString() + " failed to load!");
    }



    private void UpdateRespawnTimer() {
        if (currentRespawnTimer > 0.0f) {
            currentRespawnTimer -= Time.deltaTime;
            if (currentRespawnTimer <= 0.0f)
                currentRespawnTimer = 0.0f;
        }
    }

    public Vector3 GetPlayer1SpawnPoint() {
        return player1SpawnPoint;
    }
    public Vector3 GetPlayer2SpawnPoint() {  
        return player2SpawnPoint;
    }
    


    private void RefreshAllPickupSpawns() {
        if (pickupsPool.Count == 0)
            return;

        foreach (var pickup in pickupsPool) {
            var script = pickup.GetComponent<Pickup>();
            script.SetActive(false);
        }


        for (int i = 0; i < pickupsSpawnPoints.Length; i++) {
            var pickup = GetUnactivePickup();
            if (!pickup)
                break;

            var script = pickup.GetComponent<Pickup>();
            script.SetActive(true);
            script.SetSpawnPointIndex(i);
            occupiedPickupsSpawnPoints[i] = true;
            pickup.transform.position = pickupsSpawnPoints[i].position;
        }
    }
    private void UpdatePickupsSpawns() {
        if (currentRespawnTimer > 0.0f)
            return;

        int spawnPointIndex = -1;
        for (int i = 0; i < occupiedPickupsSpawnPoints.Length; i++) {
            if (!occupiedPickupsSpawnPoints[i]) {
                spawnPointIndex = i;
                break;
            }
        }
        if (spawnPointIndex == -1) {
            currentRespawnTimer = pickupRespawnRetryTimer;
            return;
        }

        var pickup = GetUnactivePickup();
        if (!pickup) {
            currentRespawnTimer = pickupRespawnRetryTimer;
            return;
        }

        pickup.SetActive(true);
        pickup.transform.position = pickupsSpawnPoints[spawnPointIndex].position;
        occupiedPickupsSpawnPoints[spawnPointIndex] = true;
        currentRespawnTimer = pickupRespawnTimer;
    }
    private Pickup GetUnactivePickup() {
        foreach (var entry in pickupsPool) {
            var script = entry.GetComponent<Pickup>();
            if (!script.GetActive())
                return script;
        }

        return null;
    }

    public void RegisterPickupDispawn(int spawnPointIndex) {
        occupiedPickupsSpawnPoints[spawnPointIndex] = false;
    }
}
