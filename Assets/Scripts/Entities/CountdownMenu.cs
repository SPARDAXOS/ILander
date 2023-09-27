using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CountdownMenu : MonoBehaviour
{
    public delegate void AnimationFinished();
    AnimationFinished callback;

    private bool initialized = false;
    private bool animationPlaying = false;


    private Animation animationComponent;


    public void Initialize() {
        if (initialized)
            return;

        animationComponent = GetComponent<Animation>();
        initialized = true;
    }

    public void Tick() {
        //ADD THIS TO THE REST!
        if (!initialized) {
            Debug.LogError("Attempted to tick uninitialized entity - CountdownMenu");
            return;
        }

        if (!animationPlaying)
            return;

        if (!animationComponent.isPlaying) {
            callback.Invoke();
            animationPlaying = false;
        }
    }

    //Sounds?

    public void PlayCountdownSound(int index) {

        Debug.Log("SOUND - " + index);

    }

    public bool IsAnimationPlaying() {
        return animationPlaying;
    }
    public void StartAnimation(AnimationFinished action) {
        if (animationPlaying)
            return;

        callback = action;
        animationComponent.Play();
        animationPlaying = true;
    }
}
