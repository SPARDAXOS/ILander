using System;
using UnityEngine;
using UnityEngine.AddressableAssets;




[Serializable]
public struct PickupEntry {

    public string name;
    public AssetReference assetReference;
    public Projectile.ProjectileType associatedProjectile;
}


[CreateAssetMenu(fileName = "PickupsBundle", menuName = "Data/PickupsBundle", order = 7)]
public class PickupsBundle : ScriptableObject
{
    public PickupEntry[] pickups;



}
