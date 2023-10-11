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
        LOADING_ASSETS,
        ACTIVE
    }

    //SerializedField Pickups respawn time?
    [SerializeField] private PickupsBundle pickupsBundle;
    [SerializeField] private ProjectilesBundle projectilesBundle;
    [SerializeField] private float pickupRespawnTimer = 1.0f;
    [SerializeField] private float pickupRespawnRetryTimer = 4.0f;

    private LevelState currentLevelState = LevelState.NONE;
    private GameMode currentGameMode = GameMode.NONE;
    private bool initialized = false;
    private bool assetsLoaded = false;

    private Vector3 player1SpawnPoint;
    private Vector3 player2SpawnPoint;

    private Transform[] pickupsSpawnPoints;
    public bool[] occupiedPickupsSpawnPoints;

    private Dictionary<Pickup.PickupType, AsyncOperationHandle<GameObject>> loadedPickupAssets;
    private Dictionary<Projectile.ProjectileType, AsyncOperationHandle<GameObject>> loadedProjectileAssets;

    public List<GameObject> pickupsPool;
    private Dictionary<Pickup.PickupType, uint> pickupsLog;

    private Dictionary<Projectile.ProjectileType, ProjectilesPool<Projectile>> projectilePools;
    private Dictionary<Projectile.ProjectileType, uint> projectilesLog;

    private float currentRespawnTimer = 0.0f;


    public void Initialize(GameMode mode) {
        if (initialized)
            return;

        currentGameMode = mode;
        SetupReferences();
        if (pickupsBundle)
            LoadAssets();
        else
            currentLevelState = LevelState.ACTIVE;

        currentRespawnTimer = pickupRespawnTimer;
        initialized = true;
    }
    public void Tick() {
        if (!initialized) {
            Debug.LogError("Attempted to tick uninitialized entity " + gameObject.name);
            return;
        }

        if (currentLevelState == LevelState.LOADING_ASSETS)
            CheckAssetsLoadingStatus();
        else if (currentLevelState == LevelState.ACTIVE && assetsLoaded) {
            if (currentGameMode == GameMode.COOP || GetGameInstance().IsHost()) {
                if (IsSpawningPossible()) {
                    UpdateRespawnTimer();
                    UpdatePickupsSpawns();
                }
                else
                    currentRespawnTimer = pickupRespawnTimer;
            }

            UpdateProjectiles();
        }
    }
    private void SetupReferences() {

        Transform player1SpawnPositionTransform = transform.Find("Player1SpawnPoint");
        if (!Utility.Validate(player1SpawnPositionTransform, "No Player1SpawnPoint was found in " + gameObject.name, Utility.ValidationLevel.WARNING, false))
            player1SpawnPoint = Vector3.zero;
        else
            player1SpawnPoint = player1SpawnPositionTransform.position;

        Transform player2SpawnPositionTransform = transform.Find("Player2SpawnPoint");
        if (!Utility.Validate(player2SpawnPositionTransform, "No Player2SpawnPoint was found in " + gameObject.name, Utility.ValidationLevel.WARNING, false))
            player2SpawnPoint = Vector3.zero;
        else
            player2SpawnPoint = player2SpawnPositionTransform.position;


        Transform PickupsSpawnPointsTransform = transform.Find("PickupSpawnPoints");
        if (!Utility.Validate(PickupsSpawnPointsTransform, "No pickups spawn points were found in " + gameObject.name, Utility.ValidationLevel.WARNING, false))
            return;

        uint pickupsSpawnPointsCount = (uint)PickupsSpawnPointsTransform.childCount;
        pickupsSpawnPoints = new Transform[pickupsSpawnPointsCount];
        occupiedPickupsSpawnPoints = new bool[pickupsSpawnPointsCount];
        for (uint i = 0; i < PickupsSpawnPointsTransform.childCount; i++)
            pickupsSpawnPoints[i] = PickupsSpawnPointsTransform.GetChild((int)i).transform;
    }
    public void ReleaseResources() {
        if (!assetsLoaded)
            return;

        foreach (var entry in pickupsPool)
            Destroy(entry.gameObject);
        foreach (var entry in loadedPickupAssets)
            Addressables.Release(entry.Value);
        foreach(var entry in projectilePools)
            entry.Value.ReleaseResources();
    }



    public void DeactivateAllPickups() {
        foreach (var pickup in pickupsPool) {
            var script = pickup.GetComponent<Pickup>();
            script.SetActive(false);
        }
    }
    public void RefreshAllPickupSpawns() {
        if (pickupsPool.Count == 0)
            return;

        var instance = GetGameInstance();
        var rpcManager = instance.GetRpcManagerScript();

        if (instance.IsHost())
            rpcManager.DeactivatePickupSpawnsServerRpc(instance.GetClientID());

        DeactivateAllPickups();

        for (int i = 0; i < pickupsSpawnPoints.Length; i++) {
            var pickup = GetRandomUnactivePickup();
            if (!pickup)
                break;

            var script = pickup.GetComponent<Pickup>();
            script.SetActive(true);
            script.SetSpawnPointIndex(i);
            occupiedPickupsSpawnPoints[i] = true;
            pickup.transform.position = pickupsSpawnPoints[i].position;

            if (instance.IsHost())
                rpcManager.UpdatePickupSpawnsServerRpc(instance.GetClientID(), script.GetPickupID(), script.GetSpawnPointIndex());
        }
    }
    private void UpdateRespawnTimer() {
        if (currentRespawnTimer > 0.0f) {
            currentRespawnTimer -= Time.deltaTime;
            if (currentRespawnTimer <= 0.0f)
                currentRespawnTimer = 0.0f;
        }
    }
    private void UpdatePickupsSpawns() {
        if (currentRespawnTimer > 0.0f)
            return;

        int spawnPointIndex = GetRandomUnoccupiedSpawnPoint();
        var pickup = GetRandomUnactivePickup();
        if (spawnPointIndex == -1 || !pickup) { //spawnPoint wont be -1 since IsSpawningPossible is called before this but i kept it for consistency's sake.
            currentRespawnTimer = pickupRespawnRetryTimer;
            return;
        }

        pickup.SetActive(true);
        pickup.SetSpawnPointIndex(spawnPointIndex);
        occupiedPickupsSpawnPoints[spawnPointIndex] = true;
        pickup.transform.position = pickupsSpawnPoints[spawnPointIndex].position;

        var instance = GetGameInstance();
        var rpcManager = instance.GetRpcManagerScript();
        if (instance.IsHost())
            rpcManager.UpdatePickupSpawnsServerRpc(instance.GetClientID(), pickup.GetPickupID(), spawnPointIndex);

        currentRespawnTimer = pickupRespawnTimer;
    }
    private void UpdateProjectiles() {
        foreach (var pool in projectilePools)
            pool.Value.Tick();
    }
    public void RegisterPickupDispawn(int spawnPointIndex) {
        occupiedPickupsSpawnPoints[spawnPointIndex] = false;
    }


    private void LoadAssets() {
        currentLevelState = LevelState.LOADING_ASSETS;

        loadedPickupAssets = new Dictionary<Pickup.PickupType, AsyncOperationHandle<GameObject>>(pickupsBundle.entries.Length);
        loadedProjectileAssets = new Dictionary<Projectile.ProjectileType, AsyncOperationHandle<GameObject>>();

        projectilesLog = new Dictionary<Projectile.ProjectileType, uint>();
        pickupsLog = new Dictionary<Pickup.PickupType, uint>();

        pickupsPool = new List<GameObject>(pickupsBundle.entries.Length);
        projectilePools = new Dictionary<Projectile.ProjectileType, ProjectilesPool<Projectile>>();

        foreach (var pickupEntry in pickupsBundle.entries) {
            if (pickupEntry.type == Pickup.PickupType.NONE) //Skip invalid entries in the bundle.
                continue;

            ProcessPickupData(pickupEntry);
            if (pickupEntry.associatedProjectile == Projectile.ProjectileType.NONE)
                continue;

            ProcessAssociatedProjectileData(pickupEntry);
        }
    }
    private void ProcessPickupData(PickupEntry entry) {
        if (pickupsLog.ContainsKey(entry.type))
            pickupsLog[entry.type]++;
        else
            pickupsLog.Add(entry.type, 1);

        //Start loading asset if it has no entry - Hasnt been loaded before! Makes sure to load assets once!
        if (!loadedPickupAssets.ContainsKey(entry.type))
            StartPickupAssetLoadProcess(entry);
    }
    private void ProcessAssociatedProjectileData(PickupEntry entry) {
        if (projectilesLog.ContainsKey(entry.associatedProjectile))
            projectilesLog[entry.associatedProjectile] += 2; //One for each player!
        else
            projectilesLog.Add(entry.associatedProjectile, 2); //One for each player!

        //Start loading asset if it has no entry
        if (!loadedProjectileAssets.ContainsKey(entry.associatedProjectile)) {

            //Looks for assetEntry in bundle
            foreach (var projectileEntry in projectilesBundle.Entries) {
                if (projectileEntry.type == entry.associatedProjectile) {
                    StartProjectileAssetLoadProcess(projectileEntry);
                    return; //I changed this for this func here!
                }
            }
            Debug.LogError("Unable to locate projectile asset associated with type " + entry.associatedProjectile.ToString());
        }
    }

    private void StartPickupAssetLoadProcess(PickupEntry entry) {
        if (entry.assetReference == null) {
            Debug.LogError("Entry " + entry.ToString() + " contains null reference to asset - StartPickupAssetLoadProcess()");
            return;
        }

        var handle = Addressables.LoadAssetAsync<GameObject>(entry.assetReference);
        handle.Completed += PickupAssetLoadedCallback;
        loadedPickupAssets.Add(entry.type, handle);
        Debug.Log(gameObject.name + " started loading pickup asset of type " + entry.type);
    }
    private void StartProjectileAssetLoadProcess(ProjectileEntry entry) {
        if (entry.assetReference == null) {
            Debug.LogError("Entry " + entry.ToString() + " contains null reference to asset - StartProjectileAssetLoadProcess()");
            return;
        }

        var handle = Addressables.LoadAssetAsync<GameObject>(entry.assetReference);
        handle.Completed += ProjectileAssetLoadedCallback;
        loadedProjectileAssets.Add(entry.type, handle);
        Debug.Log(gameObject.name + " started loading projectile asset " + entry.type.ToString());
    }
    private void CheckAssetsLoadingStatus() {
        bool checkResults = true;

        foreach (var entry in loadedPickupAssets) {
            if (entry.Value.Status == AsyncOperationStatus.Failed) {
                loadedPickupAssets.Remove(entry.Key); //Clears failed to load asset entry.
                pickupsLog.Remove(entry.Key);
                continue;
            }
            if (entry.Value.Status != AsyncOperationStatus.Succeeded)
                checkResults = false;
        }
        foreach(var entry in loadedProjectileAssets) {
            if (entry.Value.Status == AsyncOperationStatus.Failed) {
                loadedProjectileAssets.Remove(entry.Key); //Clears failed to load asset entry.
                projectilesLog.Remove(entry.Key);
                continue;
            }
            if (entry.Value.Status != AsyncOperationStatus.Succeeded)
                checkResults = false;
        }

        if (checkResults) {
            Debug.Log("Level Finished Loading Assets!");
            SetupPickupsPool();
            SetupProjectilePools();
            currentLevelState = LevelState.ACTIVE;
            assetsLoaded = true;
        }
    }

    private void SetupPickupsPool() {
        int IDCounter = 0;
        foreach(var type in pickupsLog) {
            for (int i = 0; i < type.Value; i++) {
                var gameObject = Instantiate(GetLoadedPickupAsset(type.Key));
                Pickup pickup = gameObject.GetComponent<Pickup>();
                pickup.Initialize();
                pickup.SetActive(false);
                pickup.SetLevelScript(this);
                pickup.SetPickupID(IDCounter);
                pickupsPool.Add(gameObject);
                IDCounter++;
            }
        }
    }
    private void SetupProjectilePools() {
        foreach (var type in projectilesLog) {
            ProjectilesPool<Projectile> projectilesPool = new ProjectilesPool<Projectile>();
            projectilePools.Add(type.Key, projectilesPool);

            for (int i = 0; i < type.Value; i++) {
                var gameObject = Instantiate(GetLoadedProjectileAsset(type.Key));
                Projectile projectile = gameObject.GetComponent<Projectile>();
                projectile.Initialize();
                projectile.SetActive(false);

                if (i == 0)
                    projectilesPool.Initialize(projectile);
                else
                    projectilesPool.AddNewElement(projectile);
            }
        }
    }


    private GameObject GetLoadedPickupAsset(Pickup.PickupType type) {
        if (type == Pickup.PickupType.NONE) {
            Debug.LogWarning("Called GetPickupAsset() with type NONE");
            return null;
        }
        foreach (var asset in loadedPickupAssets) {
            if (asset.Key == type)
                return asset.Value.Result;
        }

        Debug.LogWarning("Falied to find requested pickup asset at GetPickupAsset()");
        return null;
    }
    private GameObject GetLoadedProjectileAsset(Projectile.ProjectileType type) {
        if (type == Projectile.ProjectileType.NONE) {
            Debug.LogWarning("Called GetProjectileAsset() with type NONE");
            return null;
        }
        foreach (var asset in loadedProjectileAssets) {
            if (asset.Key == type)
                return asset.Value.Result;
        }

        Debug.LogWarning("Falied to find requested projectile asset at GetProjectileAsset()");
        return null;
    }

    public Vector3 GetPlayer1SpawnPoint() {
        return player1SpawnPoint;
    }
    public Vector3 GetPlayer2SpawnPoint() {  
        return player2SpawnPoint;
    }

    private Pickup GetPickupByID(int ID) {
        foreach (var entry in pickupsPool) {
            var script = entry.GetComponent<Pickup>();
            if (script.GetPickupID() == ID)
                return script;
        }

        return null;
    }
    private Pickup GetRandomUnactivePickup() {
        List<Pickup> unactivePickups = new List<Pickup>();
        foreach (var entry in pickupsPool) {
            Pickup script = entry.GetComponent<Pickup>();
            if (!script.IsActive())
                unactivePickups.Add(script);
        }

        if (unactivePickups.Count == 0)
            return null;

        int rand = UnityEngine.Random.Range(0, unactivePickups.Count);
        return unactivePickups[rand];
    }
    private int GetRandomUnoccupiedSpawnPoint() {
        List<int> unactiveSpawnPoints = new List<int>();
        for (int i = 0; i < occupiedPickupsSpawnPoints.Length; i++) {
            if (!occupiedPickupsSpawnPoints[i])
                unactiveSpawnPoints.Add(i);
        }

        if (unactiveSpawnPoints.Count == 0)
            return -1;

        int rand = UnityEngine.Random.Range(0, unactiveSpawnPoints.Count);
        return unactiveSpawnPoints[rand];
    }

    private bool IsSpawningPossible() {
        bool results = false;
        foreach (var entry in occupiedPickupsSpawnPoints) {
            if (!entry)
                results = true;
        }
        return results;
    }



    //Left those for information purposes.
    private void PickupAssetLoadedCallback(AsyncOperationHandle<GameObject> handle) {
        if (handle.Status == AsyncOperationStatus.Succeeded)
            Debug.Log("Successfully loaded " + handle.Result.ToString());
        else
            Debug.LogWarning("Asset " + handle.ToString() + " failed to load!");
    }
    private void ProjectileAssetLoadedCallback(AsyncOperationHandle<GameObject> handle) {
        if (handle.Status == AsyncOperationStatus.Succeeded)
            Debug.Log("Successfully loaded " + handle.Result.ToString());
        else
            Debug.LogWarning("Asset " + handle.ToString() + " failed to load!");
    }


    public bool SpawnProjectile(Player owner, Projectile.ProjectileType type) {
        if (type == Projectile.ProjectileType.NONE) {
            Debug.LogWarning("Received NONE as type in SpawnProjectile");
            return false;
        }
        if (!projectilePools.ContainsKey(type)) {
            Debug.LogError("Unable to find projectiles pool associated with type " + type.ToString());
            return false;
        }

        return projectilePools[type].SpawnProjectile(owner);
    }
    public bool ReceiveProjectileSpawnRequest(Player.PlayerType owner, Projectile.ProjectileType type) {
        if (owner == Player.PlayerType.NONE)
            return false;

        //See if you can add the muzzle flash event from rpc call to this!

        //NOTE: Call it from the player instead? is that even possible?

        if (owner == Player.PlayerType.PLAYER_1)
            return SpawnProjectile(GetGameInstance().GetPlayer1Script(), type);
        else if (owner == Player.PlayerType.PLAYER_2)
            return SpawnProjectile(GetGameInstance().GetPlayer2Script(), type);

        return false;
    }
    public void ReceivePickupSpawnRequestRpc(int ID, int spawnIndex) {
        var targetPickup = GetPickupByID(ID);
        if (!targetPickup) {
            Debug.LogError("Failed to find pickup with ID " + ID);
            return;
        }

        targetPickup.SetActive(true);
        targetPickup.SetSpawnPointIndex(spawnIndex);
        occupiedPickupsSpawnPoints[spawnIndex] = true;
        targetPickup.transform.position = pickupsSpawnPoints[spawnIndex].position;
    }
}
