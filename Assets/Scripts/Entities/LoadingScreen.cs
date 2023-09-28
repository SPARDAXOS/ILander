using Initialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
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
        GameInstance.Validate(loadingBarFillTransform, "Failed to find reference to LoadingBarFill - LoadingScreen");

        loadingBarFill = loadingBarFillTransform.GetComponent<Image>();
        GameInstance.Validate(loadingBarFill, "Failed to find component Image on LoadingBarFill - LoadingScreen");
    }
    public void SetLoadingBarValue(float value) {
        loadingBarFill.fillAmount = value;
    }
}