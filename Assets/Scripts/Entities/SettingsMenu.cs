using Microsoft.Win32.SafeHandles;
using Mono.Cecil;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using static UnityEngine.EventSystems.EventTrigger;

public class SettingsMenu : MonoBehaviour
{
    public enum QualityPreset {
        LOW = 0,
        MEDIUM = 1,
        HIGH = 2,
        ULTRA = 3,
        CUSTOM = 4,
        NONE = 5 //Primarily to avoid GameInstance.startingQualityLevel from being equal to currentQualityPreset
    }

    private Color selectedPresetColor = new Color(0.06f, 0.6f, 0.77f, 1.0f);

    private bool initialized = false;
    public QualityPreset currentQualityPreset = QualityPreset.NONE;


    private Color defaultPresetNormalColor;
    private Color defaultPresetSelectedColor;

    private Resolution[] supportedResolutions = null;


    private TMP_Dropdown resolutionDropdown           = null;
    private TMP_Dropdown windowModeDropdown           = null;
    private TMP_Dropdown vsyncDropdown                = null;
    private TMP_Dropdown fpsLimitDropdown             = null;
    private TMP_Dropdown antiAliasingDropdown         = null;
    private TMP_Dropdown textureQualityDropdown       = null;
    private TMP_Dropdown anisotropicFilteringDropdown = null;


    private Button lowPresetButton    = null;
    private Button mediumPresetButton = null;
    private Button highPresetButton   = null;
    private Button ultraPresetButton  = null;
    private Button customPresetButton = null;


    public void Initialize() {
        if (initialized)
            return;

        SetupReferences();
        SetupResolutions();
        SetupWindowModes();
        UpdateAllData();
        initialized = true;
    }
    private void SetupReferences() {

        //Presets
        Transform presetsTransform = transform.Find("Presets");
        GameInstance.Validate(presetsTransform, "Failed to find reference to Presets - SettingsMenu");

        Transform lowPresetTransform    = presetsTransform.Find("LowButton");
        Transform mediumPresetTransform = presetsTransform.Find("MediumButton");
        Transform highPresetTransform   = presetsTransform.Find("HighButton");
        Transform ultraPresetTransform  = presetsTransform.Find("UltraButton");
        Transform customPresetTransform = presetsTransform.Find("CustomButton");

        GameInstance.Validate(lowPresetTransform, "Failed to find reference to LowButton - SettingsMenu");
        GameInstance.Validate(mediumPresetTransform, "Failed to find reference to MediumButton - SettingsMenu");
        GameInstance.Validate(highPresetTransform, "Failed to find reference to HighButton - SettingsMenu");
        GameInstance.Validate(ultraPresetTransform, "Failed to find reference to UltraButton - SettingsMenu");
        GameInstance.Validate(customPresetTransform, "Failed to find reference to CustomButton - SettingsMenu");

        lowPresetButton    = lowPresetTransform.GetComponent<Button>();
        mediumPresetButton = mediumPresetTransform.GetComponent<Button>();
        highPresetButton   = highPresetTransform.GetComponent<Button>();
        ultraPresetButton  = ultraPresetTransform.GetComponent<Button>();
        customPresetButton = customPresetTransform.GetComponent<Button>();

        GameInstance.Validate(lowPresetButton, "Failed to find component Button in lowPresetTransform - SettingsMenu");
        GameInstance.Validate(mediumPresetButton, "Failed to find component Button in mediumPresetButton - SettingsMenu");
        GameInstance.Validate(highPresetButton, "Failed to find component Button in highPresetButton - SettingsMenu");
        GameInstance.Validate(ultraPresetButton, "Failed to find component Button in ultraPresetButton - SettingsMenu");
        GameInstance.Validate(customPresetButton, "Failed to find component Button in customPresetButton - SettingsMenu");

        defaultPresetNormalColor = customPresetButton.colors.normalColor;
        defaultPresetSelectedColor = customPresetButton.colors.selectedColor;


        //Resolution
        Transform resolutionDropDownTransform = transform.Find("ResolutionDropdown");
        GameInstance.Validate(resolutionDropDownTransform, "Failed to find reference to ResolutionDropdown - SettingsMenu");
        resolutionDropdown = resolutionDropDownTransform.GetComponent<TMP_Dropdown>();
        GameInstance.Validate(resolutionDropdown, "Failed to find component TMP_Dropdown for resolutionDropdown - SettingsMenu");

        //WindowMode
        Transform windowModeDropDownTransform = transform.Find("WindowModeDropdown");
        GameInstance.Validate(windowModeDropDownTransform, "Failed to find reference to WindowModeDropdown - SettingsMenu");
        windowModeDropdown = windowModeDropDownTransform.GetComponent<TMP_Dropdown>();
        GameInstance.Validate(windowModeDropdown, "Failed to find component TMP_Dropdown for windowModeDropdown - SettingsMenu");

        //Vsync
        Transform vsyncDropDownTransform = transform.Find("VsyncDropdown");
        GameInstance.Validate(vsyncDropDownTransform, "Failed to find reference to VsyncDropdown - SettingsMenu");
        vsyncDropdown = vsyncDropDownTransform.GetComponent<TMP_Dropdown>();
        GameInstance.Validate(vsyncDropdown, "Failed to find component TMP_Dropdown for VsyncDropdown - SettingsMenu");

        //FpsLimit
        Transform fpsLimitDropDownTransform = transform.Find("FpsLimitDropdown");
        GameInstance.Validate(fpsLimitDropDownTransform, "Failed to find reference to FpsLimitDropdown - SettingsMenu");
        fpsLimitDropdown = fpsLimitDropDownTransform.GetComponent<TMP_Dropdown>();
        GameInstance.Validate(fpsLimitDropdown, "Failed to find component TMP_Dropdown for fpsLimitDropdown - SettingsMenu");

        //Anti Aliasing
        Transform antiAliasingDropDownTransform = transform.Find("AntiAliasingDropdown");
        GameInstance.Validate(antiAliasingDropDownTransform, "Failed to find reference to AntiAliasingDropdown - SettingsMenu");
        antiAliasingDropdown = antiAliasingDropDownTransform.GetComponent<TMP_Dropdown>();
        GameInstance.Validate(antiAliasingDropdown, "Failed to find component TMP_Dropdown for antiAliasingDropdown - SettingsMenu");

        //Texture Quality
        Transform textureQualityDropDownTransform = transform.Find("TextureQualityDropdown");
        GameInstance.Validate(textureQualityDropDownTransform, "Failed to find reference to TextureQualityDropdown - SettingsMenu");
        textureQualityDropdown = textureQualityDropDownTransform.GetComponent<TMP_Dropdown>();
        GameInstance.Validate(textureQualityDropdown, "Failed to find component TMP_Dropdown for textureQualityDropdown - SettingsMenu");

        //AnisotropicFiltering
        Transform AFilteringDropDownTransform = transform.Find("AnisotropicFilteringDropdown");
        GameInstance.Validate(AFilteringDropDownTransform, "Failed to find reference to AnisotropicFilteringDropdown - SettingsMenu");
        anisotropicFilteringDropdown = AFilteringDropDownTransform.GetComponent<TMP_Dropdown>();
        GameInstance.Validate(anisotropicFilteringDropdown, "Failed to find component TMP_Dropdown for anisotropicFilteringDropdown - SettingsMenu");


    }
    private void SetupResolutions() {
        supportedResolutions = Screen.resolutions;
        List<string> dropdownOptions = new List<string>();
        foreach(var entry in supportedResolutions)
            dropdownOptions.Add(entry.width + "X" + entry.height + "  " + entry.refreshRate + "Hz");

        resolutionDropdown.ClearOptions();
        resolutionDropdown.AddOptions(dropdownOptions);
    }
    private void SetupWindowModes() {
        List<string> modes = new List<string>();
#if UNITY_STANDALONE_WIN
        modes.Add("Exclusive Fullscreen");
        windowModeDropdown.AddOptions(modes);
#elif UNITY_STANDALONE_OSX
        modes.Add("Maximized Window");
        windowModeDropdown.AddOptions(modes);
#endif
    }

    public void SetQualityPreset(QualityPreset level) {

        if (level == currentQualityPreset || level == QualityPreset.NONE)
            return;

        ResetAllPresetButtonsColors();

        if ((int)level == 0) {
            currentQualityPreset = QualityPreset.LOW;
            SetPresetSelected(ref lowPresetButton);
        }
        else if ((int)level == 1) {
            currentQualityPreset = QualityPreset.MEDIUM;
            SetPresetSelected(ref mediumPresetButton);
        }
        else if ((int)level == 2) {
            currentQualityPreset = QualityPreset.HIGH;
            SetPresetSelected(ref highPresetButton);
        }
        else if ((int)level == 3) {
            currentQualityPreset = QualityPreset.ULTRA;
            SetPresetSelected(ref ultraPresetButton);
        }
        else if ((int)level == 4) {
            currentQualityPreset = QualityPreset.CUSTOM;
            SetPresetSelected(ref customPresetButton);
            return;
        }

        QualitySettings.SetQualityLevel((int)level, true);
        UpdateAllData();
    }


    //IMPORTANT NOTES:
    //-Presets by unity are busted. if i change anything in them by setting vsync, antialiasing etc. it will become permenant.
    //-Turn GameSettings into QualityPreset where i define the preset myself.



    private void CheckPresetChanges() {
        if (currentQualityPreset != QualityPreset.CUSTOM) {
            currentQualityPreset = QualityPreset.CUSTOM;
            ResetAllPresetButtonsColors();
            SetPresetSelected(ref customPresetButton);
        }
    }
    private void ResetAllPresetButtonsColors() {
        ColorBlock colorBlock = customPresetButton.colors;
        colorBlock.normalColor = defaultPresetNormalColor;
        colorBlock.selectedColor = defaultPresetSelectedColor;

        customPresetButton.colors = colorBlock;
        lowPresetButton.colors = colorBlock;
        mediumPresetButton.colors = colorBlock;
        highPresetButton.colors = colorBlock;
        ultraPresetButton.colors = colorBlock;
    }


    public void UpdateVsync() {
        int value = vsyncDropdown.value;
        if (value > 4 || value < 0) {
            Debug.LogError("Value above 4 or less than 0 was sent to UpdateVsync - Allowed values are 0, 1, 2, 3, 4");
            QualitySettings.vSyncCount = 1;
            return;
        }

        CheckPresetChanges();
        QualitySettings.vSyncCount = value;
    }
    public void UpdateFPSLimit() {
        int value = fpsLimitDropdown.value;
        if (value > 3 || value < 0) {
            Debug.LogError("Value above 3 or less than 0 was sent to UpdateFPSLimit - Allowed values are 0, 1, 2, 3");
            Application.targetFrameRate = 30;
            return;
        }

        if (value == 0)
            value = -1; //Default in Unity to use platforms default target frame rate.
        else if (value == 1)
            value = 30;
        else if (value == 2)
            value = 60;
        else if (value == 3)
            value = 144;

        Application.targetFrameRate = value;
    }
    public void UpdateAntiAliasing() {
        int value = antiAliasingDropdown.value;
        //To func?
        if (value > 3 || value < 0) {
            Debug.LogError("Value above 3 or less than 0 was sent to UpdateAntiAliasing - Allowed values are 0, 1, 2, 3");
            QualitySettings.antiAliasing = 1;
            return;
        }

        if (value == 0)
            value = 0; //No MSAA
        else if (value == 1)
            value = 2;
        else if (value == 2)
            value = 4;
        else if (value == 3)
            value = 8;

        CheckPresetChanges();
        QualitySettings.antiAliasing = value;
    }
    public void UpdateTextureQuality() {
        //0 - fullres, 1 - Half res, 2 - Quarter res, 3 - eigth res
        int value = textureQualityDropdown.value;
        if (value > 3 || value < 0) {
            Debug.LogError("Value above 3 or less than 0 was sent to UpdateTextureQuality - Allowed values are 0, 1, 2, 3");
            QualitySettings.masterTextureLimit = 3;
            return;
        }
        CheckPresetChanges();
        QualitySettings.masterTextureLimit = value;
    }
    public void UpdateAnisotropicFiltering() {
        //ForceEnable, Enable, Disable
        int value = anisotropicFilteringDropdown.value;
        if (value > 2 || value < 0) {
            Debug.LogError("Value above 2 or less than 0 was sent to UpdateAnisotropicFiltering - Allowed values are 0, 1, 2");
            QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
            return;
        }

        var result = AnisotropicFiltering.Disable;
        if (value == 1)
            result = AnisotropicFiltering.Enable;
        else if (value == 2)
            result = AnisotropicFiltering.ForceEnable;

        CheckPresetChanges();
        QualitySettings.anisotropicFiltering = result;
    }
    public void UpdateQualityLevel(int level) {
        //Custom(4), Ultra(3), High(2), Medium(1), Low(0)
        if (level > 4 || level < 0) {
            Debug.LogError("Value above 4 or less than 0 was sent to UpdateQualityLevel - Allowed values are 0, 1, 2, 3, 4");
            QualitySettings.SetQualityLevel(0, true);

            SetPresetSelected(ref lowPresetButton);
            currentQualityPreset = QualityPreset.LOW;
            return;
        }
        SetQualityPreset((QualityPreset)level);
    }

    public void UpdateResolution() {
        int value = resolutionDropdown.value;
        Assert.IsFalse(value > supportedResolutions.Length - 1 || value < 0);
        var results = supportedResolutions[value];
        Screen.SetResolution(results.width, results.height, Screen.fullScreen);
        CheckPresetChanges();
    }
    public void UpdateWindowMode() {
        int value = windowModeDropdown.value;
        if (value > 3 || value < 0) {
            Debug.LogError("Value above 3 or less than 0 was sent to SetWindowMode - Allowed values are 0, 1, 2(win only), 3(mac only)");
            return;
        }

        var results = FullScreenMode.Windowed;
        if (value == 1)
            results = FullScreenMode.FullScreenWindow;

#if UNITY_STANDALONE_WIN
        if (value == 2)
            results = FullScreenMode.ExclusiveFullScreen;
#elif UNITY_STANDALONE_OSX
        if (value == 2)
            results = FullScreenMode.MaximizedWindow;
#endif

        Screen.fullScreenMode = results;
    }



    private void SetPresetSelected(ref Button presetButton) {
        ColorBlock colorBlock = presetButton.colors;
        colorBlock.normalColor = selectedPresetColor;
        colorBlock.selectedColor = selectedPresetColor;
        presetButton.colors = colorBlock;
    }

    private void RemoveAllCallbacks() {
        //Any listeners from editor are persistent and unaffected by this.
        resolutionDropdown.onValueChanged.RemoveAllListeners();
        resolutionDropdown.onValueChanged.RemoveAllListeners();
        windowModeDropdown.onValueChanged.RemoveAllListeners();
        vsyncDropdown.onValueChanged.RemoveAllListeners();
        fpsLimitDropdown.onValueChanged.RemoveAllListeners();
        antiAliasingDropdown.onValueChanged.RemoveAllListeners();
        textureQualityDropdown.onValueChanged.RemoveAllListeners();
        anisotropicFilteringDropdown.onValueChanged.RemoveAllListeners();
    }
    private void SetupAllCallbacks() {
        resolutionDropdown.onValueChanged.AddListener(delegate { UpdateResolution(); });
        windowModeDropdown.onValueChanged.AddListener(delegate { UpdateWindowMode(); });
        vsyncDropdown.onValueChanged.AddListener(delegate { UpdateVsync(); });
        fpsLimitDropdown.onValueChanged.AddListener(delegate { UpdateFPSLimit(); });
        antiAliasingDropdown.onValueChanged.AddListener(delegate {  UpdateAntiAliasing(); });
        textureQualityDropdown.onValueChanged.AddListener(delegate { UpdateTextureQuality(); });
        anisotropicFilteringDropdown.onValueChanged?.AddListener(delegate { UpdateAnisotropicFiltering(); });
    }

    private void UpdateAllData() {

        //To avoid calls to callbacks that would count as manual adjustment of settings. (Sets preset to Custom)
        RemoveAllCallbacks();

        //Resolution
        var currentResolution = Screen.currentResolution;
        for (uint i = 0; i < supportedResolutions.Length; i++) {
            var entry = supportedResolutions[i];
            if (entry.width == currentResolution.width && entry.height == currentResolution.height && entry.refreshRate == currentResolution.refreshRate)
                resolutionDropdown.value = (int)i;
        }

        //Window Mode
        var currentWindowMode = Screen.fullScreenMode;
        if (currentWindowMode == FullScreenMode.Windowed)
            windowModeDropdown.value = 0;
        else if (currentWindowMode == FullScreenMode.FullScreenWindow)
            windowModeDropdown.value = 1;
#if UNITY_STANDALONE_WIN
        else if (currentWindowMode == FullScreenMode.ExclusiveFullScreen)
            windowModeDropdown.value = 2;
#elif UNITY_STANDALONE_OSX
        else if (currentWindowMode == FullScreenMode.MaximizedWindow)
            windowModeDropdown.value = 2;
#endif

        //Vsync
        vsyncDropdown.value = QualitySettings.vSyncCount; //The options indicies mirror vSyncCount allowed values.

        //FPS Limit
        var fpsLimit = Application.targetFrameRate;
        if (fpsLimit == -1)
            fpsLimitDropdown.value = 0;
        else if (fpsLimit == 30)
            fpsLimitDropdown.value = 1;
        else if (fpsLimit == 60)
            fpsLimitDropdown.value = 2;
        else if (fpsLimit == 144)
            fpsLimitDropdown.value = 3;

        //Anti Aliasing
        var antiAliasing = QualitySettings.antiAliasing;
        if (antiAliasing == 0)
            antiAliasingDropdown.value = 0;
        else if (antiAliasing == 2)
            antiAliasingDropdown.value = 1;
        else if (antiAliasing == 4)
            antiAliasingDropdown.value = 2;
        else if (antiAliasing == 8)
            antiAliasingDropdown.value = 3;

        //Texture Quality
        textureQualityDropdown.value = QualitySettings.masterTextureLimit; //The options indicies mirror masterTextureLimit allowed values.

        //Anisotropic Filtering
        var anisotropicFiltering = QualitySettings.anisotropicFiltering;
        if (anisotropicFiltering == AnisotropicFiltering.Disable)
            anisotropicFilteringDropdown.value = 0;
        else if (anisotropicFiltering == AnisotropicFiltering.Enable)
            anisotropicFilteringDropdown.value = 1;
        else if (anisotropicFiltering == AnisotropicFiltering.ForceEnable)
            anisotropicFilteringDropdown.value = 2;

        SetupAllCallbacks();
    }

    public void ReturnButton() {
        GameInstance.GetInstance().SetGameState(GameInstance.GameState.MAIN_MENU);
    }
}
