using System.Collections.Generic;
using UnityEngine;
using static GameInstance;
using ILanderUtility;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using static UnityEngine.EventSystems.EventTrigger;
using UnityEngine.InputSystem;
using System;
using static UnityEngine.Rendering.VirtualTexturing.Debugging;

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
    private bool initialized = false;
    private bool assetsLoaded = false;

    private Vector3 player1SpawnPoint;
    private Vector3 player2SpawnPoint;

    private uint pickupsSpawnPointsCount = 0; //kINDA NOT needed

    private Transform[] pickupsSpawnPoints;
    public bool[] occupiedPickupsSpawnPoints;

    private Dictionary<string, AsyncOperationHandle<GameObject>> loadedPickupAssets;
    private Dictionary<Projectile.ProjectileType, AsyncOperationHandle<GameObject>> loadedProjectileAssets;

    private List<GameObject> pickupsPool;
    private Dictionary<Projectile.ProjectileType, ProjectilesPool<Projectile>> projectilePools;
    private Dictionary<Projectile.ProjectileType, uint> queuedProjectileElements;

    private float currentRespawnTimer = 0.0f;


    //Plan:
    //Load all pickups and keep track of which projectile types and how many instances each are requested.
    //Load all requested ones and in the call back, create a pool and fill it with Requested amount.

    //Plan 2:
    //Load all pickups and during each, read the projectile type
    //1. Does pool for type already exist? Just add element.
    //2. Pool doesnt exist? load asset with same projectile type from bundle. AND create pool for it.
    //3. On callback, get pool for it and initialize it with first value.


    public void Initialize() {
        if (initialized)
            return;



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
            if (IsSpawningPossible()) {
                UpdateRespawnTimer();
                UpdatePickupsSpawns();
            }
            else {
                currentRespawnTimer = pickupRespawnTimer; //Otherwise reset it?
            }
            foreach (var pool in projectilePools)
                pool.Value.Tick();
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
        foreach(var entry in projectilePools)
            entry.Value.ReleaseResources();
    }

    private void LoadAssets() {

        //This needs refactoring and further testing.
        //Also, i disabled some error messages. Enable them since they help pinpoint what went wrong in case of errors.

        currentLevelState = LevelState.LOADING_ASSETS;
        loadedPickupAssets = new Dictionary<string, AsyncOperationHandle<GameObject>>(pickupsBundle.pickups.Length);
        loadedProjectileAssets = new Dictionary<Projectile.ProjectileType, AsyncOperationHandle<GameObject>>();
        queuedProjectileElements = new Dictionary<Projectile.ProjectileType, uint>();

        //Use pool class?
        pickupsPool = new List<GameObject>(pickupsBundle.pickups.Length);
        projectilePools = new Dictionary<Projectile.ProjectileType, ProjectilesPool<Projectile>>();

        foreach (var pickupEntry in pickupsBundle.pickups) {
            if (pickupEntry.name.Length == 0) //Skip empty entries in the bundle.
                continue;

            //Load pickup
            StartPickupAssetLoadProcess(pickupEntry);
            if (pickupEntry.associatedProjectile == Projectile.ProjectileType.NONE)
                continue;

            //Update queued entities
            if (queuedProjectileElements.ContainsKey(pickupEntry.associatedProjectile))
                queuedProjectileElements[pickupEntry.associatedProjectile] += 2; //One for each player!
            else
                queuedProjectileElements.Add(pickupEntry.associatedProjectile, 2);

            //Start loading asset if it has no entry
            if (!loadedProjectileAssets.ContainsKey(pickupEntry.associatedProjectile)) {
                foreach (var projectileEntry in projectilesBundle.Entries) {
                    if (projectileEntry.type == pickupEntry.associatedProjectile) {
                        StartProjectileAssetLoadProcess(projectileEntry);
                        break;
                    }
                }

                //Debug.LogError("Unable to locate projectile asset associated with type " + pickupEntry.associatedProjectile.ToString());
            }




                //If pool exist then type is ready and available so just add element
            /*if (DoesPoolExist(pickupEntry.associatedProjectile)) {
                var gameObject = Instantiate(loadedProjectileAssets[pickupEntry.associatedProjectile].Result);
                var script = gameObject.GetComponent<Projectile>();
                projectilePools[pickupEntry.associatedProjectile].AddNewElement(script);
                Debug.Log("Pool already exist for element" + pickupEntry.associatedProjectile.ToString() + " so a new element was added!");
            }
            else {
                if (!loadedProjectileAssets.ContainsKey(pickupEntry.associatedProjectile)) {

                    ProjectileEntry targetEntry = new ProjectileEntry();
                    bool foundEntry = false;
                    foreach (var projectileEntry in projectilesBundle.Entries) {
                        if (projectileEntry.type == pickupEntry.associatedProjectile) {
                            foundEntry = true;
                            targetEntry = projectileEntry;
                            break;
                        }
                    }

                    if (foundEntry) {
                        var projectileHandle = Addressables.LoadAssetAsync<GameObject>(targetEntry.asset);
                        projectileHandle.Completed += ProjectileAssetLoadedCallback;
                        loadedProjectileAssets.Add(targetEntry.type, projectileHandle);
                        Debug.Log(gameObject.name + " started loading projectile asset " + targetEntry.type.ToString());
                        continue;
                    }
                    else
                        Debug.LogError("Unable to locate projectile asset associated with type " + pickupEntry.associatedProjectile.ToString());
                }
                else { //Its being loaded so add 1 to stack so it adds one more element to pool once its done!

                    Debug.Log("Pool doesnt exist for element" + pickupEntry.associatedProjectile.ToString() + " but its being loaded. Element was queued!");
                    if (bufferedProjectileElements.ContainsKey(pickupEntry.associatedProjectile))
                        bufferedProjectileElements[pickupEntry.associatedProjectile]++;
                    else
                        bufferedProjectileElements.Add(pickupEntry.associatedProjectile, 1);
                }

            }*/
        }
    }
    private void StartPickupAssetLoadProcess(PickupEntry entry) {
        if (entry.assetReference == null) {
            Debug.LogError("Entry " + entry.ToString() + " contains null reference to asset - StartPickupAssetLoadProcess()");
            return;
        }

        var handle = Addressables.LoadAssetAsync<GameObject>(entry.assetReference);
        handle.Completed += PickupAssetLoadedCallback;
        loadedPickupAssets.Add(entry.name, handle);
        Debug.Log(gameObject.name + " started loading pickup asset " + entry.name);
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

    private void SetupProjectilePools() {
        foreach (var queue in queuedProjectileElements) {
            ProjectilesPool<Projectile> projectilesPool = new ProjectilesPool<Projectile>();
            projectilePools.Add(queue.Key, projectilesPool);

            for (int i = 0; i < queue.Value; i++) {
                var gameObject = Instantiate(GetProjectileAsset(queue.Key));
                Projectile projectile = gameObject.GetComponent<Projectile>();
                projectile.SetActive(false);
                projectile.Initialize();

                if (i == 0)
                    projectilesPool.Initialize(projectile);
                else
                    projectilesPool.AddNewElement(projectile);
            }
        }
    }

    private GameObject GetProjectileAsset(Projectile.ProjectileType type) {
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

    private void CheckAssetsLoadingStatus() {
        bool checkResults = true;
        //HERE!
        //Also clean the dictionary of counts after everything is done

        foreach (var entry in loadedPickupAssets) {
            if (entry.Value.Status == AsyncOperationStatus.Failed) {
                loadedPickupAssets.Remove(entry.Key); //Clears failed to load asset entry.
                continue;
            }
            if (entry.Value.Status != AsyncOperationStatus.Succeeded)
                checkResults = false;
        }

        foreach(var entry in loadedProjectileAssets) {
            if (entry.Value.Status == AsyncOperationStatus.Failed) {
                loadedProjectileAssets.Remove(entry.Key); //Clears failed to load asset entry.
                continue;
            }
            if (entry.Value.Status != AsyncOperationStatus.Succeeded)
                checkResults = false;
        }

        if (checkResults) {
            Debug.Log("Level Finished Loading Assets!");
            RefreshAllPickupSpawns();
            SetupProjectilePools();
            //Clear queued elements dictionary? or it could be used as a book keeper for types and counts!
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
    private void ProjectileAssetLoadedCallback(AsyncOperationHandle<GameObject> handle) {
        if (handle.Status == AsyncOperationStatus.Succeeded) {
            Debug.Log("Successfully loaded " + handle.Result.ToString());

            ////Create First Element
            //var gameObject = Instantiate(handle.Result);
            //Projectile projectile = gameObject.GetComponent<Projectile>();
            //projectile.SetActive(false);
            //
            ////Create Pool
            //var type = projectile.GetProjectileType();
            //if (type == Projectile.ProjectileType.NONE) {
            //    Debug.Log("Projectile " + gameObject.name + " had the projectile type NONE - Make sure to set it to the correct value!");
            //    return;
            //}
            //ProjectilesPool<Projectile> projectilesPool = new ProjectilesPool<Projectile>();
            //projectilesPool.Initialize(projectile);
            //projectilePools.Add(type, projectilesPool);
            //
            ////Add Queued Elements
            //if (bufferedProjectileElements.ContainsKey(type)) {
            //    uint count = bufferedProjectileElements[type];
            //    if (count == 0)
            //        return;
            //
            //    for (uint i = 0; i < count; i++) {
            //        var element = Instantiate(handle.Result);
            //        Projectile script = element.GetComponent<Projectile>();
            //        script.SetActive(false);
            //        projectilePools[type].AddNewElement(script);
            //    }
            //    bufferedProjectileElements[type] = 0;
            //}
        }
        else
            Debug.LogWarning("Asset " + handle.ToString() + " failed to load!");

    }

    private bool DoesPoolExist(Projectile.ProjectileType type) {
        if (projectilePools.Count == 0)
            return false;

        if (projectilePools.ContainsKey(type))
            return true;
        return false;
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

    private void RefreshAllPickupSpawns() {
        if (pickupsPool.Count == 0)
            return;

        foreach (var pickup in pickupsPool) {
            var script = pickup.GetComponent<Pickup>();
            script.SetActive(false);
        }


        for (int i = 0; i < pickupsSpawnPoints.Length; i++) {
            var pickup = GetRandomUnactivePickup();
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

        var pickup = GetRandomUnactivePickup();
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
            if (!script.IsActive())
                return script;
        }

        return null;
    }
    private Pickup GetRandomUnactivePickup() {
        List<Pickup> unactivePickups = new List<Pickup>();
        foreach(var entry in pickupsPool) {
            Pickup script = entry.GetComponent<Pickup>();
            if (!script.IsActive())
                unactivePickups.Add(script);
        }

        if (unactivePickups.Count == 0)
            return null;

        int rand = UnityEngine.Random.Range(0, unactivePickups.Count);
        return unactivePickups[rand];
    }
    private bool IsSpawningPossible() {
        bool results = false;
        foreach (var entry in occupiedPickupsSpawnPoints) {
            if (!entry)
                results = true;
        }
        return results;
    }

    public void RegisterPickupDispawn(int spawnPointIndex) {
        occupiedPickupsSpawnPoints[spawnPointIndex] = false;
    }
}
