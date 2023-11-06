using System;
using UnityEngine;
using ILanderUtility;

public class ExplosionSpawner : MonoBehaviour {
    private bool initialized = false;
    private Action ExplosionCallback;
    private Animator animatorComp;


    public void Initialize() {
        if (initialized)
            return;


        SetupReferences();
        initialized = true;
    }
    private void SetupReferences() {
        animatorComp = GetComponent<Animator>();
        Utility.Validate(animatorComp, "Failed to get reference to Animator component - ExplosionSpawner", Utility.ValidationLevel.ERROR, true);


    }

    public bool PlayAnimation(string name, Action callback) {
        if (!initialized) {
            Debug.LogWarning("Attempted to call PlayAnimation at ExplosionSpawner without initializing it first!");
            return false;
        }

        if (!animatorComp.GetCurrentAnimatorStateInfo(0).IsName("Empty"))
            return false;

        animatorComp.Play(name, -1);
        ExplosionCallback = callback;
        return true;
    }
    public void ExplosionEvent() {
        if (ExplosionCallback != null)
            ExplosionCallback.Invoke();
    }
}
