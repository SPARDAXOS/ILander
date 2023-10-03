using ILanderUtility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GameInstance;

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

    private GameObject startButtonGameObject = null;
    private GameObject hostChoiceGameObject = null;
    private GameObject levelSelectorGameObject = null;

    private Image levelPreview;
    private TextMeshProUGUI levelName;

    public void Initialize() {
        if (initialized)
            return;

        levelsBundle = GetInstance().GetLevelsBundle();
        SetupReferences();
        UpdateLevelPreview();
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
        levelSelectorGameObject = levelSelectorTransform.gameObject;

        levelName = levelSelectorTransform.GetComponent<TextMeshProUGUI>();
        Utility.Validate(levelPreview, "Failed to find component Text on LevelSelector - LevelSelectMenu");

        startButtonGameObject = previewWindowTransform.Find("StartButton").gameObject;
        Utility.Validate(startButtonGameObject, "Failed to find reference to StartButton - LevelSelectMenu");

        hostChoiceGameObject = transform.Find("HostChoice").gameObject;
        Utility.Validate(hostChoiceGameObject, "Failed to find reference to HostChoice - LevelSelectMenu");
        
    }


    public void SetLevelSelectMenuMode(LevelSelectMenuMode mode) {
        currentMenuMode = mode;
        ApplyCurrentMenuMode();
    }


    public void ActivateStartButton() {
        startButtonGameObject.SetActive(true);
        hostChoiceGameObject.SetActive(false);
    }
    private void ApplyCurrentMenuMode() {
        if (currentMenuMode == LevelSelectMenuMode.NORMAL) {
            startButtonGameObject.SetActive(true);
            levelSelectorGameObject.SetActive(true);
            hostChoiceGameObject.SetActive(false);
        }
        else if (currentMenuMode == LevelSelectMenuMode.ONLINE) {
            startButtonGameObject.SetActive(false);
            levelSelectorGameObject.SetActive(false);
            hostChoiceGameObject.SetActive(true);
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

        if (currentMenuMode == LevelSelectMenuMode.ONLINE)
            GetInstance().GetRpcManagerScript().UpdateSelectedLevelIndexServerRpc((ulong)GetInstance().GetClientID(), currentLevelIndex);


        //TODO: Reset state or make gameinstance do it!
    }
}

