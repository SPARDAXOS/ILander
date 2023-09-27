using System;
using UnityEngine;
using UnityEngine.AddressableAssets;


[Serializable]
public struct AssetEntry {
    public string name;
    public AssetReference reference;
}


[CreateAssetMenu(fileName = "AssetsBundle", menuName = "Data/AssetsBundle", order = 2)]
public class AssetsBundle : ScriptableObject
{
    public AssetEntry[] assets;




}
