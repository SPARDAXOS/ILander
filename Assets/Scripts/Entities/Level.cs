using Initialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using ILanderUtility;

public class Level : MonoBehaviour
{
    //SerializedField Pickups respawn time?


    private bool initialized = false;

    private Vector3 player1SpawnPoint;
    private Vector3 player2SpawnPoint;

    private uint pickupsSpawnPointsCount = 0;

    private Transform[] pickupsSpawnPoints;
    private bool[] occupiedPickupsSpawnPoints;

    public List<GameObject> pickupsPool;

    public void Initialize() {
        if (initialized)
            return;

        SetupReferences();
        CreatePickupsPool();
        RefreshAllPickupSpawns();
        initialized = true;
    }
    public void Tick() {
        if (!initialized) {
            Debug.LogError("Attempted to tick uninitialized entity " + gameObject.name);
            return;
        }


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

    public Vector3 GetPlayer1SpawnPoint() {
        return player1SpawnPoint;
    }
    public Vector3 GetPlayer2SpawnPoint() {  
        return player2SpawnPoint;
    }
    

    private void CreatePickupsPool() {
        if (pickupsSpawnPointsCount == 0)
            return;

        pickupsPool = new List<GameObject>((int)pickupsSpawnPointsCount);
        for (uint i = 0; i < pickupsSpawnPointsCount; i++) {
            //Need Projectiles Data
            //Create All from one type!
            //Morph them into other types by changing their data on demand


        }
    }

    private void RefreshAllPickupSpawns() {
        //Spawns random stuff all over! Called at start!
    }
    private void UpdatePickupsSpawns() {

    }
}
