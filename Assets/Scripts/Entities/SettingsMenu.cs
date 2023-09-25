using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{

    private bool initialized = false;


    private TMP_Dropdown vsyncDropdown = null;
    private TMP_Dropdown fpsLimitDropdown = null;
    private TMP_Dropdown antiAliasingDropdown = null;

    public void Initialize() {
        if (initialized)
            return;

        SetupReferences();
        //LoadSettings from SO
        initialized = true;
    }
    private void SetupReferences() {

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

    }

    //Does not take platform into consideration! ^^
    public void UpdateVsync() {
        int value = vsyncDropdown.value;
        if (value > 4) {
            Debug.LogError("Value above 4 was sent to SetVsync - Allowed values are 0, 1, 2, 3, 4");
            QualitySettings.vSyncCount = 1;
            return;
        }
        QualitySettings.vSyncCount = value;
    }
    public void UpdateFPSLimit() {
        int value = fpsLimitDropdown.value;
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
        if (value == 0)
            value = 0; //No MSAA
        else if (value == 1)
            value = 2;
        else if (value == 2)
            value = 4;
        else if (value == 3)
            value = 8;

        QualitySettings.antiAliasing = value;
        Debug.Log("Anti aliasing set to " + value);
    }

}
