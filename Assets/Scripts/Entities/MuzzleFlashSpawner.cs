using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using ILanderUtility;

public class MuzzleFlashSpawner : MonoBehaviour
{
    private bool initialized = false;
    private Action muzzleFlashEventCallback;

    private Vector3 defaultScale = Vector3.one;

    private SpriteRenderer spriteRendererComp;
    private Animator animatorComp;

    public void Initialize() {
        if (initialized)
            return;

        defaultScale = transform.localScale;
        SetupReferences();
        initialized = true;
    }
    private void SetupReferences() {
        animatorComp = GetComponent<Animator>();
        Utility.Validate(animatorComp, "Failed to get reference to Animator component - MuzzleFlashSpawner", Utility.ValidationLevel.ERROR, true);

        spriteRendererComp = GetComponent<SpriteRenderer>();
        Utility.Validate(spriteRendererComp, "Failed to get reference to SpriteRenderer component - MuzzleFlashSpawner", Utility.ValidationLevel.ERROR, true);
    }


    public bool PlayAnimation(string name, Action callback, Vector3 customSize) {
        if (!initialized) {
            Debug.LogWarning("Attempted to call PlayAnimation at MuzzleFlashSpawner without initializing it first!");
            return false;
        }

        if (!animatorComp.GetCurrentAnimatorStateInfo(0).IsName("Empty"))
            return false;


        SetupScale(customSize);
        animatorComp.Play(name, -1);
        muzzleFlashEventCallback = callback;
        return true;
    }
    private void SetupScale(Vector3 scale) {
        Vector3 currentScale = defaultScale;
        Vector3 newScale = currentScale;
        newScale.x *= scale.x;
        newScale.y *= scale.y;
        newScale.z *= scale.z;
        transform.localScale = newScale;
    }
    public void MuzzleFlashEvent() {
        muzzleFlashEventCallback.Invoke();
    }
}
