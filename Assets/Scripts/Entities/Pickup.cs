using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Pickup : MonoBehaviour
{
    [SerializeField] protected float Potency = 0.0f;
    protected bool active = false;
    protected Level levelScript;
    protected int spawnPointIndex = -1;

    public void SetActive(bool state) {
        active = state;
        gameObject.SetActive(state);
    }
    public bool GetActive() {
        return active;
    }

    public void SetSpawnPointIndex(int index) {
        spawnPointIndex = index;
    }
    public void SetLevelScript(Level script) {
        levelScript = script;
    }


    public abstract void Activate(Player user);
    protected virtual void OnPickup(Player script) {
        script.RegisterPickup(this);
        SetActive(false);
        gameObject.SetActive(false);
        levelScript.RegisterPickupDispawn(spawnPointIndex);
    }


    private void OnTriggerEnter2D(Collider2D collision) {
        var playerScript = collision.GetComponent<Player>();
        if (!playerScript)
            return;

        OnPickup(playerScript);
    }
}
