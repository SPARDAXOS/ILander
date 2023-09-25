using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public struct SkinEntry
{
    public string name;
    public Sprite sprite;
}


[CreateAssetMenu(fileName = "PlayerSkins", menuName = "Data/PlayerSkins", order = 5)]
public class PlayerSkins : ScriptableObject {
    public SkinEntry[] skins;
}
