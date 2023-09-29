using System;
using System.Collections;
using System.Collections.Generic;
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

///Inconsistency since levels and assets are loading by instance while characters is by customization screen
//  Move gamesettigns and this characters thing both to be loaded by game instance!