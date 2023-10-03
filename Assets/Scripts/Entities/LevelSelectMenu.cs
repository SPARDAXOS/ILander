using Initialization;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GameInstance;
using ILanderUtility;

public class LevelSelectMenu : MonoBehaviour
{
    public enum LevelSelectMenuMode
    {
        NORMAL,
        ONLINE
    }




    private LevelSelectMenuMode currentMenuMode = LevelSelectMenuMode.NORMAL;

    private bool initialized = false;
    private LevelsBundle levelsBundle;

    private int currentLevelIndex = 0;

    private Image levelPreview;
    private TextMeshProUGUI levelName;

    public void Initialize() {
        if (initialized)
            return;

        levelsBundle = GetInstance().GetLevelsBundle();
        SetupReferences();
        ApplyCurrentMenuMode();
        initialized = true;
    }

    private void SetupReferences() {

        Transform previewWindowTransform = transform.Find("PreviewWindow");
        Utility.Validate(previewWindowTransform, "Failed to find reference to PreviewWindow - LevelSelectMenu");

        Transform levelPreviewTransform = previewWindowTransform.Find("LevelPreview");
        Utility.Validate(levelPreviewTransform, "Failed to find reference to LevelPreview - LevelSelectMenu");

        levelPreview = levelPreviewTransform.GetComponent<Image>();
        Utility.Validate(levelPreview, "Failed to find component Image on LevelPreview - LevelSelectMenu");

        Transform levelSelectorTransform = previewWindowTransform.Find("LevelSelector");
        Utility.Validate(levelSelectorTransform, "Failed to find reference to LevelSelector - LevelSelectMenu");

        levelName = levelSelectorTransform.GetComponent<TextMeshProUGUI>();
        Utility.Validate(levelPreview, "Failed to find component Text on LevelSelector - LevelSelectMenu");
    }


    public void SetLevelSelectMenuMode(LevelSelectMenuMode mode)
    {
        currentMenuMode = mode;
        ApplyCurrentMenuMode();
    }



    private void ApplyCurrentMenuMode()
    {
        //UpdateLevelPreview(); do this here somewhere!
        if (currentMenuMode == LevelSelectMenuMode.NORMAL)
        {

        }
        else if (currentMenuMode == LevelSelectMenuMode.ONLINE)
        {

        }
    }

    private void UpdateLevelPreview()
    {
        if (!levelsBundle)
        {
            Debug.LogError("LevelsBundle has not been set for LevelSelectMenu");
            return;
        }

        levelPreview.sprite = levelsBundle.levels[currentLevelIndex].preview;
        levelName.text = levelsBundle.levels[currentLevelIndex].name;
    }

    public void SwitchLevelLeft() {
        currentLevelIndex--;
        if (currentLevelIndex < 0)
            currentLevelIndex = levelsBundle.levels.Length - 1;

        UpdateLevelPreview();
    }
    public void SwitchLevelRight() {
        currentLevelIndex++;
        if (currentLevelIndex > levelsBundle.levels.Length - 1)
            currentLevelIndex = 0;

        UpdateLevelPreview();
    }
    public void StartButton() {
        var instance = GameInstance.GetInstance();
        instance.StartLevel((uint)currentLevelIndex);
    }
}

