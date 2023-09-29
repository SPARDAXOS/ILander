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
using ILanderUtility;

public class SettingsMenu : MonoBehaviour {
    public enum QualityPreset {
        LOW = 0,
        MEDIUM = 1,
        HIGH = 2,
        ULTRA = 3,
        CUSTOM = 4
    }
    public enum AntiAliasingOptions {
        NO_MSAA = 0,
        X2 = 2,
        X4 = 4,
        X8 = 8
    }
    public enum TextureQualityOptions {
        ULTRA = 0,
        HIGH = 1,
        MEDIUM = 2,
        LOW = 3
    }
    public enum AnisotropicFilteringOptions {
        DISABLE = AnisotropicFiltering.Disable,
        ENABLE = AnisotropicFiltering.Enable,
        FORCE_ENABLE = AnisotropicFiltering.ForceEnable
    }



    [SerializeField] private Color selectedPresetColor = new Color(0.06f, 0.6f, 0.77f, 1.0f);

    private bool initialized = false;

    private QualityPreset currentQualityPreset = QualityPreset.LOW;
    private QualityPresetData currentQualityPresetData;
    private Resolution[] supportedResolutions = null;

    private Color defaultPresetNormalColor;
    private Color defaultPresetSelectedColor;

    private GameSettings gameSettings;

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


    //TODO: Reverse the order for the TextureQuality dropdown menu to keep it inline with lowest to highest going down!
    //Keep in mind that some things depend on the TextureQuality options having the same order as the options in QualitySettings! make sure to keep an eye out!

    //TODO: Unrelated to this class but i could reuse the player data SO to give each character their own values! Maybe add something in Customization window to
    //indicate this?

    //TODO: Setup the code to set player data from customization screen
    //SetPlayer1Data(data, sprite?), SetPlayer2Data(data, sprite?)



    public void Initialize() {
        if (initialized)
            return;

        SetupReferences();
        SetupResolutions();
        SetupWindowModes();
        SetupPresetsCallbacks();
        ApplyDefaultPreset();
        initialized = true;
    }
    private void SetupReferences() {

        gameSettings = GameInstance.GetInstance().GetGameSettings();
        Utility.Validate(gameSettings, "Failed to retrieve GameSettings from GameInstance - SettingsMenu", true);

        //Presets
        Transform presetsTransform = transform.Find("Presets");
        Utility.Validate(presetsTransform, "Failed to find reference to Presets - SettingsMenu", true);

        Transform lowPresetTransform    = presetsTransform.Find("LowButton");
        Transform mediumPresetTransform = presetsTransform.Find("MediumButton");
        Transform highPresetTransform   = presetsTransform.Find("HighButton");
        Transform ultraPresetTransform  = presetsTransform.Find("UltraButton");
        Transform customPresetTransform = presetsTransform.Find("CustomButton");

        Utility.Validate(lowPresetTransform, "Failed to find reference to LowButton - SettingsMenu", true);
        Utility.Validate(mediumPresetTransform, "Failed to find reference to MediumButton - SettingsMenu", true);
        Utility.Validate(highPresetTransform, "Failed to find reference to HighButton - SettingsMenu", true);
        Utility.Validate(ultraPresetTransform, "Failed to find reference to UltraButton - SettingsMenu", true);
        Utility.Validate(customPresetTransform, "Failed to find reference to CustomButton - SettingsMenu", true);

        lowPresetButton    = lowPresetTransform.GetComponent<Button>();
        mediumPresetButton = mediumPresetTransform.GetComponent<Button>();
        highPresetButton   = highPresetTransform.GetComponent<Button>();
        ultraPresetButton  = ultraPresetTransform.GetComponent<Button>();
        customPresetButton = customPresetTransform.GetComponent<Button>();

        Utility.Validate(lowPresetButton, "Failed to find component Button in lowPresetTransform - SettingsMenu", true);
        Utility.Validate(mediumPresetButton, "Failed to find component Button in mediumPresetButton - SettingsMenu", true);
        Utility.Validate(highPresetButton, "Failed to find component Button in highPresetButton - SettingsMenu", true);
        Utility.Validate(ultraPresetButton, "Failed to find component Button in ultraPresetButton - SettingsMenu", true);
        Utility.Validate(customPresetButton, "Failed to find component Button in customPresetButton - SettingsMenu", true);

        //If any of the buttons has unique values for these then they will be set to customPresetButton's values upon clearing!
        defaultPresetNormalColor = customPresetButton.colors.normalColor;
        defaultPresetSelectedColor = customPresetButton.colors.selectedColor;


        //Resolution
        Transform resolutionDropDownTransform = transform.Find("ResolutionDropdown");
        Utility.Validate(resolutionDropDownTransform, "Failed to find reference to ResolutionDropdown - SettingsMenu", true);
        resolutionDropdown = resolutionDropDownTransform.GetComponent<TMP_Dropdown>();
        Utility.Validate(resolutionDropdown, "Failed to find component TMP_Dropdown for resolutionDropdown - SettingsMenu", true);

        //WindowMode
        Transform windowModeDropDownTransform = transform.Find("WindowModeDropdown");
        Utility.Validate(windowModeDropDownTransform, "Failed to find reference to WindowModeDropdown - SettingsMenu", true);
        windowModeDropdown = windowModeDropDownTransform.GetComponent<TMP_Dropdown>();
        Utility.Validate(windowModeDropdown, "Failed to find component TMP_Dropdown for windowModeDropdown - SettingsMenu", true);

        //Vsync
        Transform vsyncDropDownTransform = transform.Find("VsyncDropdown");
        Utility.Validate(vsyncDropDownTransform, "Failed to find reference to VsyncDropdown - SettingsMenu", true);
        vsyncDropdown = vsyncDropDownTransform.GetComponent<TMP_Dropdown>();
        Utility.Validate(vsyncDropdown, "Failed to find component TMP_Dropdown for VsyncDropdown - SettingsMenu", true);

        //FpsLimit
        Transform fpsLimitDropDownTransform = transform.Find("FpsLimitDropdown");
        Utility.Validate(fpsLimitDropDownTransform, "Failed to find reference to FpsLimitDropdown - SettingsMenu", true);
        fpsLimitDropdown = fpsLimitDropDownTransform.GetComponent<TMP_Dropdown>();
        Utility.Validate(fpsLimitDropdown, "Failed to find component TMP_Dropdown for fpsLimitDropdown - SettingsMenu", true);

        //Anti Aliasing
        Transform antiAliasingDropDownTransform = transform.Find("AntiAliasingDropdown");
        Utility.Validate(antiAliasingDropDownTransform, "Failed to find reference to AntiAliasingDropdown - SettingsMenu", true);
        antiAliasingDropdown = antiAliasingDropDownTransform.GetComponent<TMP_Dropdown>();
        Utility.Validate(antiAliasingDropdown, "Failed to find component TMP_Dropdown for antiAliasingDropdown - SettingsMenu", true);

        //Texture Quality
        Transform textureQualityDropDownTransform = transform.Find("TextureQualityDropdown");
        Utility.Validate(textureQualityDropDownTransform, "Failed to find reference to TextureQualityDropdown - SettingsMenu", true);
        textureQualityDropdown = textureQualityDropDownTransform.GetComponent<TMP_Dropdown>();
        Utility.Validate(textureQualityDropdown, "Failed to find component TMP_Dropdown for textureQualityDropdown - SettingsMenu", true);

        //AnisotropicFiltering
        Transform AFilteringDropDownTransform = transform.Find("AnisotropicFilteringDropdown");
        Utility.Validate(AFilteringDropDownTransform, "Failed to find reference to AnisotropicFilteringDropdown - SettingsMenu", true);
        anisotropicFilteringDropdown = AFilteringDropDownTransform.GetComponent<TMP_Dropdown>();
        Utility.Validate(anisotropicFilteringDropdown, "Failed to find component TMP_Dropdown for anisotropicFilteringDropdown - SettingsMenu", true);


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


    private void SetQualityPreset(QualityPreset level) {
        if (level == currentQualityPreset && initialized) //&& initialized to avoid skipping this if default preset == defualt value for currentQualityPreset
            return;

        if ((int)level == 0) {
            currentQualityPreset = QualityPreset.LOW;
            currentQualityPresetData = gameSettings.lowPreset;
        }
        else if ((int)level == 1) {
            currentQualityPreset = QualityPreset.MEDIUM;
            currentQualityPresetData = gameSettings.mediumPreset;
        }
        else if ((int)level == 2) {
            currentQualityPreset = QualityPreset.HIGH;
            currentQualityPresetData = gameSettings.highPreset;
        }
        else if ((int)level == 3) {
            currentQualityPreset = QualityPreset.ULTRA;
            currentQualityPresetData = gameSettings.ultraPreset;
        }
        else if ((int)level == 4) {
            currentQualityPreset = QualityPreset.CUSTOM;
            ResetAllPresetButtonsColors();
            ApplySelectedPresetButtonColor();
            return;
        }

        ResetAllPresetButtonsColors();
        ApplySelectedPresetButtonColor();
        ApplyCurrentQualityPreset();
        UpdateGUI();
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
    private void ApplySelectedPresetButtonColor() {
        //Slight code repetition but it is necessary to not override any other options for Button.colors other than selectedColor and normalColor. 
        ColorBlock colorBlock;
        if (currentQualityPreset == QualityPreset.CUSTOM)
        {
            colorBlock = customPresetButton.colors;
            colorBlock.normalColor = selectedPresetColor;
            colorBlock.selectedColor = selectedPresetColor;
            customPresetButton.colors = colorBlock;
        }
        else if (currentQualityPreset == QualityPreset.LOW)
        {
            colorBlock = lowPresetButton.colors;
            colorBlock.normalColor = selectedPresetColor;
            colorBlock.selectedColor = selectedPresetColor;
            lowPresetButton.colors = colorBlock;
        }
        else if (currentQualityPreset == QualityPreset.MEDIUM)
        {
            colorBlock = mediumPresetButton.colors;
            colorBlock.normalColor = selectedPresetColor;
            colorBlock.selectedColor = selectedPresetColor;
            mediumPresetButton.colors = colorBlock;
        }
        else if (currentQualityPreset == QualityPreset.HIGH)
        {
            colorBlock = highPresetButton.colors;
            colorBlock.normalColor = selectedPresetColor;
            colorBlock.selectedColor = selectedPresetColor;
            highPresetButton.colors = colorBlock;
        }
        else if (currentQualityPreset == QualityPreset.ULTRA)
        {
            colorBlock = ultraPresetButton.colors;
            colorBlock.normalColor = selectedPresetColor;
            colorBlock.selectedColor = selectedPresetColor;
            ultraPresetButton.colors = colorBlock;
        }
    }
    private void ApplyCurrentQualityPreset() {
        if (currentQualityPreset == QualityPreset.CUSTOM)
            return;

        QualitySettings.antiAliasing = (int)currentQualityPresetData.antiAliasing;
        QualitySettings.anisotropicFiltering = (AnisotropicFiltering)currentQualityPresetData.anisotropicFiltering;
        QualitySettings.masterTextureLimit = (int)currentQualityPresetData.textureQuality;
    }
    private void UpdateGUI() {
        //To avoid calls to callbacks that would count as manual adjustment of settings. (Sets preset to Custom)
        RemoveAllSettingsCallbacks();

        //Resolution
        var currentResolution = Screen.currentResolution;
        for (uint i = 0; i < supportedResolutions.Length; i++)
        {
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
        vsyncDropdown.value = QualitySettings.vSyncCount; //The dropdown menu options indicies mirror vSyncCount allowed values.

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
        antiAliasingDropdown.value = (int)currentQualityPresetData.antiAliasing;

        //Texture Quality
        textureQualityDropdown.value = QualitySettings.masterTextureLimit; //The options indicies mirror masterTextureLimit allowed values.

        //Anisotropic Filtering
        anisotropicFilteringDropdown.value = (int)currentQualityPresetData.anisotropicFiltering;

        SetupSettingsCallbacks();
    }


    private bool ValidateUserInput(int min, uint max, int value, string operationName){
        if (value < min) {
            Debug.LogError("Invalid user input! \n Value less than " + min + " was sent to " + operationName);
            return false;
        }
        else if (value > max) {
            Debug.LogError("Invalid user input! \n Value more than " + max + " was sent to " + operationName);
            return false;
        }

        return true;
    }
    private void ApplyDefaultPreset() {
        //If set to custom then it will switch to low!
        if (gameSettings.defualtPreset == QualityPreset.CUSTOM)
            SetQualityPreset(QualityPreset.LOW);
        else
            SetQualityPreset(gameSettings.defualtPreset);
    }
    private void CheckPresetChanges() {
        if (currentQualityPreset != QualityPreset.CUSTOM) {
            currentQualityPreset = QualityPreset.CUSTOM;
            ResetAllPresetButtonsColors();
            ApplySelectedPresetButtonColor();
        }
    }


    public void UpdateVsync() {
        int value = vsyncDropdown.value;
        if (!ValidateUserInput(0, 4, value, "UpdateVsync"))
            return;

        CheckPresetChanges();
        QualitySettings.vSyncCount = value;
    }
    public void UpdateFPSLimit() {
        int value = fpsLimitDropdown.value;
        if (!ValidateUserInput(0, 3, value, "UpdateFPSLimit"))
            return;

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
        if (!ValidateUserInput(0, 3, value, "UpdateAntiAliasing"))
            return;

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
        if (!ValidateUserInput(0, 3, value, "UpdateTextureQuality"))
            return;

        CheckPresetChanges();
        QualitySettings.masterTextureLimit = value;
    }
    public void UpdateAnisotropicFiltering() {
        //ForceEnable, Enable, Disable
        int value = anisotropicFilteringDropdown.value;
        if (!ValidateUserInput(0, 2, value, "UpdateAnisotropicFiltering"))
            return;

        var result = AnisotropicFiltering.Disable;
        if (value == 1)
            result = AnisotropicFiltering.Enable;
        else if (value == 2)
            result = AnisotropicFiltering.ForceEnable;

        CheckPresetChanges();
        QualitySettings.anisotropicFiltering = result;
    }
    public void UpdateQualityPreset(int level) {
        //Custom(4), Ultra(3), High(2), Medium(1), Low(0)
        if (!ValidateUserInput(0, 4, level, "UpdateQualityPreset"))
            return;

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
        if (!ValidateUserInput(0, 3, value, "SetWindowMode"))
            return;

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


    private void RemoveAllSettingsCallbacks() {
        //Any listeners from editor are persistent and unaffected by this.
        //UpdateQualityPreset is not affected by this since it wouldnt make sense if it was. 
        resolutionDropdown.onValueChanged.RemoveAllListeners();
        resolutionDropdown.onValueChanged.RemoveAllListeners();
        windowModeDropdown.onValueChanged.RemoveAllListeners();
        vsyncDropdown.onValueChanged.RemoveAllListeners();
        fpsLimitDropdown.onValueChanged.RemoveAllListeners();
        antiAliasingDropdown.onValueChanged.RemoveAllListeners();
        textureQualityDropdown.onValueChanged.RemoveAllListeners();
        anisotropicFilteringDropdown.onValueChanged.RemoveAllListeners();
    }
    private void SetupSettingsCallbacks() {
        resolutionDropdown.onValueChanged.AddListener(delegate { UpdateResolution(); });
        windowModeDropdown.onValueChanged.AddListener(delegate { UpdateWindowMode(); });
        vsyncDropdown.onValueChanged.AddListener(delegate { UpdateVsync(); });
        fpsLimitDropdown.onValueChanged.AddListener(delegate { UpdateFPSLimit(); });
        antiAliasingDropdown.onValueChanged.AddListener(delegate {  UpdateAntiAliasing(); });
        textureQualityDropdown.onValueChanged.AddListener(delegate { UpdateTextureQuality(); });
        anisotropicFilteringDropdown.onValueChanged?.AddListener(delegate { UpdateAnisotropicFiltering(); });
    }
    private void SetupPresetsCallbacks() {
        lowPresetButton.onClick.AddListener(delegate { UpdateQualityPreset(0); });
        mediumPresetButton.onClick.AddListener(delegate { UpdateQualityPreset(1); });
        highPresetButton.onClick.AddListener(delegate { UpdateQualityPreset(2); });
        ultraPresetButton.onClick.AddListener(delegate { UpdateQualityPreset(3); });
        customPresetButton.onClick.AddListener(delegate { UpdateQualityPreset(4); });
    }


    public void ReturnButton() {
        GameInstance.GetInstance().SetGameState(GameInstance.GameState.MAIN_MENU);
    }
}
