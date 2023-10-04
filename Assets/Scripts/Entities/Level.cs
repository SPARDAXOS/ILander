using Initialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using ILanderUtility;

public class Level : MonoBehaviour
{
    [SerializeField] private Transform player1SpawnPoint;
    [SerializeField] private Transform player2SpawnPoint;

    private bool initialized = false;

    private Vector3 playerSpawnPoint1;
    private Vector3 playerSpawnPoint2;

    private Transform[] pickupsSpawnPoints;
    private bool[] occupiedPickupsSpawnPoints;



    public void Initialize() {
        if (initialized)
            return;

        SetupReferences();
        initialized = true;
    }

    private void SetupReferences() {

        if (!Utility.Validate(player1SpawnPoint, "No Player1SpawnPoint has been set for level " + gameObject.name, false))
            playerSpawnPoint1 = Vector3.zero;
        else
            playerSpawnPoint1 = player1SpawnPoint.position;
        if (!Utility.Validate(player2SpawnPoint, "No Player2SpawnPoint has been set for level " + gameObject.name, false))
            playerSpawnPoint2 = Vector3.zero;
        else
            playerSpawnPoint2 = player2SpawnPoint.position;


        //Transform PickupsSpawnPointsTransform = transform.Find("PickupSpawnPoints");
        //pickupsSpawnPoints = new Transform[PickupsSpawnPointsTransform.childCount];
        //occupiedPickupsSpawnPoints = new bool[PickupsSpawnPointsTransform.childCount];
        //for (uint i = 0; i < PickupsSpawnPointsTransform.childCount; i++)
        //    pickupsSpawnPoints[i] = PickupsSpawnPointsTransform.GetChild((int)i).transform;

    }

    public Vector3 GetPlayer1SpawnPoint() {
        return playerSpawnPoint1;
    }
    public Vector3 GetPlayer2SpawnPoint() {  
        return playerSpawnPoint2;
    }

    //Use bool to check whether a pickup has spawned at point
    //



}
