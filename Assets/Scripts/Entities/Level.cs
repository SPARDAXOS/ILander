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

    private Vector3 spawnPoint1;
    private Vector3 spawnPoint2;

    public void Initialize() {
        if (initialized)
            return;

        SetupReferences();
        initialized = true;
    }

    private void SetupReferences() {

        if (!Utility.Validate(player1SpawnPoint, "No Player1SpawnPoint has been set for level " + gameObject.name, false))
            spawnPoint1 = Vector3.zero;
        else
            spawnPoint1 = player1SpawnPoint.position;
        if (!Utility.Validate(player2SpawnPoint, "No Player2SpawnPoint has been set for level " + gameObject.name, false))
            spawnPoint2 = Vector3.zero;
        else
            spawnPoint2 = player2SpawnPoint.position;





    }

    public Vector3 GetPlayer1SpawnPoint() {
        return spawnPoint1;
    }
    public Vector3 GetPlayer2SpawnPoint() {  
        return spawnPoint2;
    }

}
