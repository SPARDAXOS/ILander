using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCamera : MonoBehaviour
{

    private GameObject followTarget;


    public void Initialize() {

    }
    public void Tick() {

    }

    public void SetFollowTarget(GameObject target) {
        followTarget = target;
    }

    private void UpdatePosition() {

    }
}
