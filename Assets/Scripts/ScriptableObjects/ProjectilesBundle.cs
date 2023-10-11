using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

[Serializable]
public struct ProjectileEntry {
    public Projectile.ProjectileType type;
    public AssetReference assetReference;
}


[CreateAssetMenu(fileName = "ProjectilesBundle", menuName = "Data/ProjectilesBundle", order = 8)]
public class ProjectilesBundle : ScriptableObject {
    public ProjectileEntry[] Entries;



}
