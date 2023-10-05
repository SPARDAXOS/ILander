using System;
using UnityEngine;


[Serializable]
public struct PlayerCharacterData {
    public string name;
    public Sprite shipSprite;
    public Sprite portraitSprite;
    public PlayerStats statsData;
}


[CreateAssetMenu(fileName = "PlayerCharactersBundle", menuName = "Data/PlayerCharactersBundle", order = 5)]
public class PlayerCharactersBundle : ScriptableObject {
    public PlayerCharacterData[] playerCharacters;
}
