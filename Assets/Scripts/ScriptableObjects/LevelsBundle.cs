using System;
using UnityEngine;
using UnityEngine.AddressableAssets;



[Serializable]
public struct LevelData {
    public string name;
    public Sprite preview;
    public AssetReference asset;
}


[CreateAssetMenu(fileName = "LevelsBundle", menuName = "Data/LevelsBundle", order = 6)]
public class LevelsBundle : ScriptableObject {

    public LevelData[] levels;
}
