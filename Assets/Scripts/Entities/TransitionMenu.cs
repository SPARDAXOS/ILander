using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransitionMenu : MonoBehaviour
{
    [SerializeField] private AnimationClip[] transitions;

    public delegate void AnimationFinished();
    AnimationFinished callback;


    private bool initialized = false;
    private bool animationPlaying = false;


    private Canvas mainCanvas = null;
    private Animation animationComponent;


    public void Initialize() {
        if (initialized)
            return;

        animationComponent = GetComponent<Animation>();
        mainCanvas = GetComponent<Canvas>();
        initialized = true;
    }

    public void StartTransition(AnimationFinished action) {
        if (animationPlaying)
            return;

        gameObject.SetActive(true); //Better pattern than instance doing it. i think,.....
        callback = action;

        PlayRandomTransition();
    }

    private void PlayRandomTransition() {
        var rand = Random.Range(0, transitions.Length);
        if (!animationComponent.Play(transitions[rand].name))
            Debug.LogError("Failed to play animation" + transitions[rand].name + " \n Clip was not found in Animation Component");
        else
            animationPlaying = true;
    }


    public void InvokeCallback() {
        callback.Invoke();
    }
    public void DisableMenu() {
        gameObject.SetActive(false);
        animationPlaying = false;
    }
}
