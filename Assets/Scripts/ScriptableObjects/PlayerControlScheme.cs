using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "PlayerControlScheme", menuName = "Data/PlayerControlScheme", order = 3)]
public class PlayerControlScheme : ScriptableObject
{
    public InputAction boosterInput;
    public InputAction rotationInput;




}
