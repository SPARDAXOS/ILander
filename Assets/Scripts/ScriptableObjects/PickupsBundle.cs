using System;
using UnityEngine;
using UnityEngine.AddressableAssets;



//Break this into 2 - Data and the entry for loading
[Serializable]
public struct PickupEntryData {

    public string name;
    public AssetReference asset;
}


[CreateAssetMenu(fileName = "PickupsBundle", menuName = "Data/PickupsBundle", order = 7)]
public class PickupsBundle : ScriptableObject
{
    public PickupEntryData[] pickups;




}
