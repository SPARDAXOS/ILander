using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using UnityEngine.AddressableAssets;


[Serializable]
public struct AssetEntry
{
    public string name;
    public AssetReference reference;
}


[CreateAssetMenu(fileName = "GameAssets", menuName = "Data/GameAssets", order = 2)]
public class GameAssets : ScriptableObject
{
    public AssetEntry[] assets;




}
