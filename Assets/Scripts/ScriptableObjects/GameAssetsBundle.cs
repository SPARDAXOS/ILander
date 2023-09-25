using System;
using UnityEngine;
using UnityEngine.AddressableAssets;


[Serializable]
public struct AssetEntry
{
    public string name;
    public AssetReference reference;
}


[CreateAssetMenu(fileName = "GameAssets", menuName = "Data/GameAssets", order = 2)]
public class GameAssetsBundle : ScriptableObject
{
    public AssetEntry[] assets;




}
