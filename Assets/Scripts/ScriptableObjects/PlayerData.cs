using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[CreateAssetMenu(fileName = "PlayerData", menuName = "Data/PlayerData", order = 4)]
public class PlayerData : ScriptableObject
{
    [Header("Thruster")]
    public float thrusterLimit = 1.0f;
    [Range(0.001f, 1.0f)] public float thrusterAccelerationRate = 1.0f;
    [Range(0.001f, 1.0f)] public float thrusterDecelerationRate = 1.0f;

    [Space(10.0f)]
    [Header("Health")]
    public float healthCap = 1.0f;

    [Space(10.0f)]
    [Header("Movement")]
    public float turnRate = 10.0f;


}
