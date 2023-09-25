using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public struct SkinEntry {
    public string name;
    public Sprite shipSprite;
    public Sprite portraitSprite;
}


[CreateAssetMenu(fileName = "PlayerSkins", menuName = "Data/PlayerSkins", order = 5)]
public class PlayerSkinsBundle : ScriptableObject {
    public SkinEntry[] skins;
}
