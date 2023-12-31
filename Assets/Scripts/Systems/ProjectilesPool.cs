using System.Collections.Generic;
using UnityEngine;

public class ProjectilesPool<T> where T : Projectile {
    private bool initialized = false;
    private List<T> pool;
    private Projectile.ProjectileType typeKey;


    public void Initialize(T firstElement, int size = 1) {
        if (initialized)
            return;

        pool = new List<T>(size);
        pool.Add(firstElement);
        typeKey = firstElement.GetProjectileType();
        initialized = true;
    }
    public void Tick() {
        if (!initialized) {
            Debug.LogError("Attempted to tick uninitialized pool [" + typeKey + "]");
            return;
        }

        UpdateActiveElements();
    }
    public void UpdateActiveElements() {
        foreach (var entry in pool) {
            if (entry.IsActive())
                entry.Tick();
        }
    }
    public void ReleaseResources() {
        foreach(var entry in pool) {
            if (entry)
                GameObject.Destroy(entry.gameObject);
        }
    }


    public bool AddNewElement(T element) {
        if (element.GetProjectileType() != typeKey) {
            Debug.LogWarning("Failed to add element to pool due to mismatching keys [" + typeKey + "]");
            return false;
        }

        pool.Add(element);
        return true;
    }
    public bool SpawnProjectile(Player owner) {
        var projectile = GetUnactiveProjectile();
        if (!projectile) {
            Debug.LogWarning("Unable to spawn projectile due to none being unactive - ProjectilesPool_" + typeKey.ToString());
            return false;
        }

        return projectile.Shoot(owner); ;
    }


    private T GetUnactiveProjectile() {
        foreach(var projectile in pool) {
            if (!projectile.IsActive())
                return projectile;
        }
        return null;
    }
    public bool IsOfType(Projectile.ProjectileType type) {
        if (typeKey == type)
            return true;
        return false;
    }
}
