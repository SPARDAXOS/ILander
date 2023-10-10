using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[CreateAssetMenu(fileName = "PlayerStats", menuName = "Data/PlayerStats", order = 4)]
public class PlayerStats : ScriptableObject
{
    [Header("Thruster")]
    public float thrusterStrengthLimit = 1.0f;
    [Range(1.0f, 1000.0f)] public float thrusterAccelerationRate = 1.0f;
    [Range(1.0f, 1000.0f)] public float thrusterDecelerationRate = 1.0f;
    [Range(1.0f, 1000.0f)] public float thrusterBreaksRate = 1.0f;

    [Space(10.0f)]
    [Header("Health/Fuel")]
    public float healthCap = 1.0f;
    public float fuelCap = 1.0f;
    public float boostCost = 0.2f;

    [Space(10.0f)]
    [Header("Movement")]
    [Range(1.0f, 1000.0f)] public float maxVelocity = 100.0f;
    [Range(1.0f, 1000.0f)] public float accelerationRate = 100.0f;
    [Range(1.0f, 1000.0f)] public float boostStrength = 400.0f;
    [Range(1.0f, 1000.0f)] public float turnRate = 150.0f;
    [Range(0.0f, 10.0f)] public float gravityScale = 0.2f;
    [Range(0.0f, 10.0f)] public float movingDragRate = 1.6f;
    [Range(0.0f, 10.0f)] public float stoppedDragRate = 0.8f;



    [Range(0.0001f, 1.0f)]
    public float steeringRate = 0.001f;


}
