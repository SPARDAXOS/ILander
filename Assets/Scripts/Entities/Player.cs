using Initialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using ILanderUtility;
using Unity.Netcode;
using Unity.Multiplayer;
using static GameInstance;
using System;

public class Player : NetworkBehaviour
{
    public enum PlayerType
    {
        NONE,
        PLAYER_1,
        PLAYER_2
    }


    [SerializeField] private PlayerControlScheme player1ControlScheme;
    [SerializeField] private PlayerControlScheme player2ControlScheme;
    [SerializeField] private Color frozenColor = Color.cyan;
    [SerializeField] private Color hitEffectColor = Color.red;
    [SerializeField] private float hitEffectDuration = 1.0f;

    public bool initialized = false;
    private PlayerCharacterData playerCharacterData; //Add Check to validate if player has data before doing any updates!
    private PlayerType currentPlayerType = PlayerType.NONE;


    private PlayerControlScheme activeControlScheme;
    private HUD HUDScript;

    public float currentThrusterStrength = 0.0f;


    private Color defaultColor = Color.white;


    public float currentHealth = 0.0f;
    public float currentFuel = 0.0f;
    public Pickup equippedPickup = null;

    private bool isMoving = false;
    private bool isRotating = false;
    private bool isFrozen = false;
    private bool isDead = false;

    private bool hitEffectOn = false;

    private float freezeTimer = 0.0f;
    private float hitEffectTimer = 0.0f;

    private MuzzleFlashSpawner muzzleFlashSpawnerScript;
    private ExplosionSpawner explosionSpawnerScript;
    private SpriteRenderer spriteRendererComp;
    private BoxCollider2D boxCollider2DComp;
    private Rigidbody2D rigidbodyComp;
    private NetworkObject networkObjectComp;


    public void Initialize() {
        if (initialized)
            return;

        //Disable networking by defualt?
        SetupReferences();
        muzzleFlashSpawnerScript.Initialize();
        explosionSpawnerScript.Initialize();
        initialized = true;
    }
    public void Tick() {
        if (!initialized) {
            Debug.LogError("Attempted to tick uninitialized entity - " + gameObject.name);
            return;
        }
        if (currentPlayerType == PlayerType.NONE) {
            Debug.LogError("Attempting to tick player of type none : " + gameObject.name);
            return;
        }

        if (isDead)
            return;

        if (isFrozen) {
            UpdateFreezeTimer();
            return;
        }
        if (hitEffectOn)
            UpdateHitEffectTimer();

        CheckInput();
        
        if (isRotating)
            UpdateRotation(); //idk if should be in fixed input!
    }
    public void FixedTick() {
        if (!initialized) {
            Debug.LogError("Attempted to tick uninitialized entity - " + gameObject.name);
            return;
        }
        if (isDead)
            return;


        UpdateGravity();
        UpdateDrag();
        if (isMoving)
            UpdateMovement();
    }
    private void SetupReferences() {

        Transform explosionSpawnerTransform = transform.Find("ExplosionSpawner");
        Utility.Validate(explosionSpawnerTransform, "Failed to get reference to ExplosionSpawner gameobject - Player", true);

        explosionSpawnerScript = explosionSpawnerTransform.GetComponent<ExplosionSpawner>();
        Utility.Validate(explosionSpawnerScript, "Failed to get reference to DeathExplosionSpawner component - Player", true);

        spriteRendererComp = GetComponent<SpriteRenderer>();
        Utility.Validate(spriteRendererComp, "Failed to get reference to SpriteRenderer component - Player", true);

        rigidbodyComp = GetComponent<Rigidbody2D>();
        Utility.Validate(rigidbodyComp, "Failed to get reference to Rigidbody2D component - Player", true);

        boxCollider2DComp = GetComponent<BoxCollider2D>();
        Utility.Validate(boxCollider2DComp, "Failed to get reference to BoxCollider2D component - Player", true);

        networkObjectComp = GetComponent<NetworkObject>();
        Utility.Validate(networkObjectComp, "Failed to get reference to NetworkObject component - Player", true);

        Transform muzzleFlashSpawnerTransformn = transform.Find("MuzzleFlashSpawner");
        Utility.Validate(muzzleFlashSpawnerTransformn, "Failed to get reference to MuzzleFlashSpawner - Player", true);

        muzzleFlashSpawnerScript = muzzleFlashSpawnerTransformn.GetComponent<MuzzleFlashSpawner>();
        Utility.Validate(muzzleFlashSpawnerScript, "Failed to get reference to MuzzleFlashSpawner component - Player", true);
    }
    public void SetupStartState() {
        currentHealth = playerCharacterData.statsData.healthCap;
        currentFuel = playerCharacterData.statsData.fuelCap;
        UpdateAllHUDData();

        //Timers and color effects
        boxCollider2DComp.enabled = true;
        isDead = false;
        SetSpriteVisibility(true); //Questionable whether gameinstance does this or this
        //Remember own spawn location? SetSpawnPoint()?
    }

    public void SetHUDReference(HUD script) {
        HUDScript = script;
    }
    public void SetPlayerType(PlayerType type) {
        currentPlayerType = type;
        if (type == PlayerType.PLAYER_1)
            activeControlScheme = player1ControlScheme;
        else if (type == PlayerType.PLAYER_2)
            activeControlScheme = player2ControlScheme;

        activeControlScheme.usePickupInput.started += UsePickupInputCallback;
        activeControlScheme.boostInput.started += BoostInputCallback;
        activeControlScheme.pauseInput.started += PauseInputCallback;
    }


    public void SetPlayerData(PlayerCharacterData data) {
        playerCharacterData = data;
        spriteRendererComp.sprite = data.shipSprite;
    }
    public void SetPlayerColor(Color color) {
        spriteRendererComp.color = color;
        defaultColor = color;
    }

    private void UpdateAllHUDData() {
        HUDScript.UpdateHealth(currentPlayerType, currentHealth / playerCharacterData.statsData.healthCap);
        HUDScript.UpdateFuel(currentPlayerType, currentFuel / playerCharacterData.statsData.fuelCap);
        HUDScript.SetPickupIcon(currentPlayerType, null);
        HUDScript.SetCharacterPortrait(currentPlayerType, playerCharacterData.portraitSprite);
    }

    private void CheckInput() {
        isMoving = activeControlScheme.movementInput.IsPressed();
        isRotating = activeControlScheme.rotationInput.IsPressed();
    }

    private void PauseInputCallback(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
        GetInstance().PauseGame();
    }
    private void BoostInputCallback(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
        Boost();
    }
    private void UsePickupInputCallback(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
        UseEquippedPickup();
    }


    private void UpdateFreezeTimer() {
        if (freezeTimer > 0.0f) {
            freezeTimer -= Time.deltaTime;
            if (freezeTimer < 0.0f)
                RemoveFreeze();
        }
        else
            RemoveFreeze();
    }
    private void RemoveFreeze() {
        freezeTimer = 0.0f;
        isFrozen = false;
        EnableInput();
        spriteRendererComp.color = defaultColor;
    }
    public void ApplyFreeze(float duration) {
        if (isDead)
            return;

        freezeTimer = duration;
        isFrozen = true;
        DisableInput();
        spriteRendererComp.color = frozenColor;
    }


    private void UpdateHitEffectTimer() {
        if (hitEffectTimer > 0.0f) {
            hitEffectTimer -= Time.deltaTime;
            if (hitEffectTimer < 0.0f)
                RemoveHitEffect();
        }
        else
            RemoveHitEffect();
    }
    private void RemoveHitEffect() {
        hitEffectOn = false;
        spriteRendererComp.color = defaultColor;
        hitEffectTimer = 0.0f;
    }
    private void ApplyHitEffect() {
        hitEffectOn = true;
        spriteRendererComp.color = hitEffectColor;
        //Any camera shake or hit stop here!
        hitEffectTimer = hitEffectDuration;
    }


    private void UpdateDrag() {
        if (isMoving)
            rigidbodyComp.drag = playerCharacterData.statsData.movingDragRate;
        else
            rigidbodyComp.drag = playerCharacterData.statsData.stoppedDragRate;
    }
    private void UpdateGravity() {
        if (!isMoving)
            rigidbodyComp.gravityScale = playerCharacterData.statsData.gravityScale;
        else
            rigidbodyComp.gravityScale = 0.0f;
    }
    private void UpdateRotation() {
        float inputValue = activeControlScheme.rotationInput.ReadValue<float>();
        float Speed = playerCharacterData.statsData.turnRate; //Delta
        if (inputValue < 0.0f)
            Speed *= -1;

        transform.Rotate(new Vector3(0.0f, 0.0f, Speed));
    }
    private void UpdateMovement() {



        //NOTE: Mechanic: the moment 2 players bump into each other, the one with the higher velocity pushes the other one away! 
        //So when there are no pickups, pushing each other is the game!

        //BUG: The cost for fuel

        float inputValue = activeControlScheme.movementInput.ReadValue<float>();
        //Break doesnt work its weird and abusable
        if (inputValue < 0.0f) {
            if (rigidbodyComp.velocity.y > 0.0f)
                rigidbodyComp.velocity = new Vector2(rigidbodyComp.velocity.x, rigidbodyComp.velocity.y - 0.01f);
            if (rigidbodyComp.velocity.x > 0.0f)
                rigidbodyComp.velocity = new Vector2(rigidbodyComp.velocity.x - 0.01f, rigidbodyComp.velocity.y);
        } 
        else if (inputValue > 0.0f) {

            Vector2 force = Time.fixedDeltaTime * new Vector2(transform.up.x, transform.up.y) * playerCharacterData.statsData.accelerationRate;
            Vector2 velocity = rigidbodyComp.velocity + force;
            if (velocity.x > playerCharacterData.statsData.maxVelocity)
                velocity.x = playerCharacterData.statsData.maxVelocity;
            if (velocity.y > playerCharacterData.statsData.maxVelocity)
                velocity.y = playerCharacterData.statsData.maxVelocity;

            rigidbodyComp.AddForce(velocity, ForceMode2D.Force);
        }
    }





    private void Boost() {
        if (isDead)
            return;

        if (currentFuel >= playerCharacterData.statsData.boostCost) {
            UseFuel(playerCharacterData.statsData.boostCost);
            rigidbodyComp.AddForce(Time.fixedDeltaTime * transform.up * playerCharacterData.statsData.boostStrength, ForceMode2D.Impulse);
            Debug.Log("BOOST!");
        }
    }


    public void RegisterPickup(Pickup script, Sprite icon = null) {
        equippedPickup = script;
        HUDScript.SetPickupIcon(currentPlayerType, icon);
    }
    private void UseEquippedPickup() {
        if (!equippedPickup || isDead)
            return;

        //BUG: Shooting a projectile will also disable the pickup if it was respawned! 

        //This also makes it reset and deactivate itself so no worries
        if (equippedPickup.Activate(this)) {
            equippedPickup = null; //? is this good enough? no resets of any kind?
            HUDScript.SetPickupIcon(currentPlayerType, null);
        }
    }


    public void AddHealth(float amount) {
        if (isDead)
            return;

        if (amount < 0.0f)
            amount *= -1;

        currentHealth += amount;
        if (currentHealth > playerCharacterData.statsData.healthCap)
            currentHealth = playerCharacterData.statsData.healthCap;

        Debug.Log("Health Pickup Used!");
        HUDScript.UpdateHealth(currentPlayerType, currentHealth / playerCharacterData.statsData.healthCap);
    }
    public void TakeDamage(float amount) {
        if (isDead)
            return;

        if (amount < 0.0f)
            amount *= -1;

        currentHealth -= amount;
        if (currentHealth <= 0.0f) {
            currentHealth = 0.0f;
            Debug.Log("Player " + gameObject.name + " is dead!");
            Dead();
        }

        ApplyHitEffect();
        HUDScript.UpdateHealth(currentPlayerType, currentHealth / playerCharacterData.statsData.healthCap);
        Debug.Log("Damage Taken!");
    }
    private void Dead() {
        isDead = true;
        DisableInput();
        rigidbodyComp.velocity = Vector3.zero;
        rigidbodyComp.gravityScale = 0.0f;
        boxCollider2DComp.enabled = false;
        SetSpriteVisibility(false);
        explosionSpawnerScript.PlayAnimation("PlayerDead", DeadAnimationCallback);
    }
    public void DeadAnimationCallback() {
        GetInstance().RegisterPlayerDeath(currentPlayerType);
    }

    public void AddFuel(float amount) {
        if (isDead)
            return;

        if (amount < 0.0f)
            amount *= -1;

        currentFuel += amount;
        if (currentFuel > playerCharacterData.statsData.fuelCap)
            currentFuel = playerCharacterData.statsData.fuelCap;

        Debug.Log("Fuel Pickup Used!");
        HUDScript.UpdateFuel(currentPlayerType, currentFuel / playerCharacterData.statsData.fuelCap);
    }
    public void UseFuel(float amount) {
        if (isDead)
            return;

        if (amount < 0.0f)
            amount *= -1;

        currentFuel -= amount;
        if (currentFuel <= 0.0f) {
            currentFuel = 0.0f;
            //Out of fuel?
        }
        HUDScript.UpdateFuel(currentPlayerType, currentFuel / playerCharacterData.statsData.fuelCap);
    }

    public bool PlayMuzzleFlashAnim(string name, Action callback, Vector3 customSize) {
        return muzzleFlashSpawnerScript.PlayAnimation(name, callback, customSize);
    }
    public Vector3 GetMuzzleFlashPosition() {
        return muzzleFlashSpawnerScript.transform.position;
    }


    public void SetSpriteVisibility(bool state) {
        spriteRendererComp.enabled = state;
    }
    public void EnableInput() {
        activeControlScheme.movementInput.Enable();
        activeControlScheme.rotationInput.Enable();
        activeControlScheme.boostInput.Enable();
        activeControlScheme.pauseInput.Enable();
        activeControlScheme.usePickupInput.Enable();
    }
    public void DisableInput() {
        activeControlScheme.movementInput.Disable();
        activeControlScheme.rotationInput.Disable();
        activeControlScheme.boostInput.Disable();
        activeControlScheme.pauseInput.Disable();
        activeControlScheme.usePickupInput.Disable();
    }


    public Vector2 GetVelocity() {
        return rigidbodyComp.velocity;
    }
    public void ApplyImpulse(Vector2 direction, float force) {
        if (isDead)
            return;

        rigidbodyComp.AddForce(Time.fixedDeltaTime * transform.up * playerCharacterData.statsData.boostStrength, ForceMode2D.Impulse);
    }
    private void OnCollisionEnter2D(Collision2D collision) {
        if (isDead)
            return;

        var script = collision.gameObject.GetComponent<Player>();
        if (script) {
            var otherPlayerVelocity = script.GetVelocity();
            float otherPlayerForce = otherPlayerVelocity.magnitude;
            float ownForce = rigidbodyComp.velocity.magnitude;

            //Add = to make both fly away?
            if (ownForce > otherPlayerForce) {
                Debug.Log("I triggered it " + gameObject.name);
                script.ApplyImpulse(transform.up, 500.0f);
            }
            else
                Debug.Log("I couldnt " + gameObject.name);

            //NOTE: Doesnt work! both can not trigger and even trigger it! do same thing or similar for touching geometry but make damage propotional to speed
        }



    }

}
