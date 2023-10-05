using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

[Serializable]
public struct PickupData {

    Sprite HUDIcon;
    AssetReference asset;
    float Potency;
}


[CreateAssetMenu(fileName = "PickupsBundle", menuName = "Data/PickupsBundle", order = 7)]
public class PickupsBundle : ScriptableObject
{
    public PickupData[] pickups;




}
