using Initialization;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelectMenu : MonoBehaviour
{
    private bool initialized = false;
    private LevelsBundle levelsBundle;

    private int currentLevelIndex = 0;

    private Image levelPreview;
    private TextMeshProUGUI levelName;

    public void Initialize() {
        if (initialized)
            return;

        SetupReferences();
        initialized = true;
    }
    public void SetLevelsBundle(LevelsBundle bundle) {
        levelsBundle = bundle;
        UpdateLevelPreview();
    }

    private void SetupReferences() {

        Transform previewWindowTransform = transform.Find("PreviewWindow");
        GameInstance.Validate(previewWindowTransform, "Failed to find reference to PreviewWindow - LevelSelectMenu");

        Transform levelPreviewTransform = previewWindowTransform.Find("LevelPreview");
        GameInstance.Validate(levelPreviewTransform, "Failed to find reference to LevelPreview - LevelSelectMenu");

        levelPreview = levelPreviewTransform.GetComponent<Image>();
        GameInstance.Validate(levelPreview, "Failed to find component Image on LevelPreview - LevelSelectMenu");

        Transform levelSelectorTransform = previewWindowTransform.Find("LevelSelector");
        GameInstance.Validate(levelSelectorTransform, "Failed to find reference to LevelSelector - LevelSelectMenu");

        levelName = levelSelectorTransform.GetComponent<TextMeshProUGUI>();
        GameInstance.Validate(levelPreview, "Failed to find component Text on LevelSelector - LevelSelectMenu");
    }
    private void UpdateLevelPreview() {
        if (!levelsBundle) {
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

