using UnityEngine;

public class CountdownMenu : MonoBehaviour {

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
        if (!initialized) {
            Debug.LogError("Attempted to tick uninitialized entity - CountdownMenu");
            return;
        }

        if (!animationPlaying)
            return;

        if (!animationComponent.isPlaying) {
            callback.Invoke();
            animationPlaying = false;
            gameObject.SetActive(false);
        }
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
        gameObject.SetActive(true);
    }
}
