using UnityEngine;



[CreateAssetMenu(fileName = "PlayerStats", menuName = "Data/PlayerStats", order = 4)]
public class PlayerStats : ScriptableObject {
    [Space(10.0f)]
    [Header("Health/Fuel")]
    public float healthCap = 1.0f;
    public float fuelCap = 1.0f;
    public float boostCost = 0.2f;


    [Space(10.0f)]
    [Header("Movement")]
    [Range(1.0f, 1000.0f)] public float impulseStrength = 350.0f;
    [SerializeField] public Vector2 maxVelocity = new Vector2(100.0f, 100.0f);
    [Range(1.0f, 1000.0f)] public float accelerationRate = 100.0f;
    [Range(1.0f, 1000.0f)] public float boostStrength = 400.0f;
    [Range(1.0f, 1000.0f)] public float turnRate = 250.0f;
    [Range(0.0f, 10.0f)] public float gravityScale = 0.2f;
    [Range(0.0f, 10.0f)] public float movingDragRate = 1.6f;
    [Range(0.0f, 10.0f)] public float stoppedDragRate = 0.8f;
}
