using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[CreateAssetMenu(fileName = "PlayerStats", menuName = "Data/PlayerStats", order = 4)]
public class PlayerStats : ScriptableObject
{
    [Header("Thruster")]
    public float thrusterStrengthLimit = 1.0f;
    [Range(0.001f, 5.0f)] public float thrusterAccelerationRate = 1.0f;
    [Range(0.001f, 5.0f)] public float thrusterDecelerationRate = 1.0f;
    [Range(0.001f, 5.0f)] public float thrusterBreaksRate = 1.0f;

    [Space(10.0f)]
    [Header("Health")]
    public float healthCap = 1.0f;

    [Space(10.0f)]
    [Header("Movement")]
    public float turnRate = 10.0f;


}
