using UnityEngine;
using UnityEngine.UI;
using ILanderUtility;

public class LoadingScreen : MonoBehaviour {

    private bool initialized = false;
    private Image loadingBarFill = null;

    public void Initialize() {
        if (initialized)
            return;

        SetupReferences();
        initialized = true;
    }

    private void SetupReferences() {
        Transform loadingBarFillTransform = transform.Find("LoadingBarFill");
        Utility.Validate(loadingBarFillTransform, "Failed to find reference to LoadingBarFill - LoadingScreen", Utility.ValidationLevel.ERROR, true);

        loadingBarFill = loadingBarFillTransform.GetComponent<Image>();
        Utility.Validate(loadingBarFill, "Failed to find component Image on LoadingBarFill - LoadingScreen", Utility.ValidationLevel.ERROR, true);
    }
    public void SetLoadingBarValue(float value) {
        if (!initialized) {
            Debug.LogWarning("Loading screen cannot update since it has not been initialized!");
            return;
        }

        loadingBarFill.fillAmount = value;
    }
}
