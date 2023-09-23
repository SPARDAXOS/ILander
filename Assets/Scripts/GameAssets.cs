using System.Collections;
using System.Collections.Generic;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using UnityEngine.AddressableAssets;


[CreateAssetMenu(fileName = "GameAssets", menuName = "Data/GameAssets", order = 2)]
public class GameAssets : ScriptableObject
{
    public AssetReference[] assets;
}
