using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;




[Serializable]
public struct QualityPresetData {
    public SettingsMenu.AntiAliasingOptions antiAliasing;
    public SettingsMenu.TextureQualityOptions textureQuality;
    public SettingsMenu.AnisotropicFilteringOptions anisotropicFiltering;
}

[CreateAssetMenu(fileName = "GameSettings", menuName = "Data/GameSettings", order = 1)]
public class GameSettings : ScriptableObject {

    public SettingsMenu.QualityPreset defualtPreset;
    public QualityPresetData lowPreset;
    public QualityPresetData mediumPreset;
    public QualityPresetData highPreset;
    public QualityPresetData ultraPreset;
}
