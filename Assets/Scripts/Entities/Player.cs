using UnityEngine;
using ILanderUtility;
using Unity.Netcode;
using static GameInstance;
using System;

public class Player : NetworkBehaviour {
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

    private bool initialized = false;
    private PlayerCharacterData playerCharacterData;
    private PlayerType currentPlayerType = PlayerType.NONE;
    private GameMode currentGameMode = GameMode.NONE;
    private Color defaultColor = Color.white;
    private Vector3 spawnPoint = Vector3.zero;


    private float currentHealth = 0.0f;
    private float currentFuel   = 0.0f;

    private bool isMoving       = false;
    private bool isRotating     = false;
    private bool isFrozen       = false;
    private bool isDead         = false;
    private bool isInvincible   = false;

    private bool hitEffectOn = false;

    private float freezeTimer   = 0.0f;
    private float hitEffectTimer = 0.0f;

    private Pickup equippedPickup                           = null;
    private HUD HUDScript                                   = null;
    private PlayerControlScheme activeControlScheme         = null;
    private MuzzleFlashSpawner muzzleFlashSpawnerScript     = null;
    private ExplosionSpawner explosionSpawnerScript         = null;
    private SpriteRenderer spriteRendererComp               = null;
    private BoxCollider2D boxCollider2DComp                 = null;
    private Rigidbody2D rigidbodyComp                       = null;
    private NetworkObject networkObjectComp                 = null;


    public void Initialize() {
        if (initialized)
            return;


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

        currentGameMode = GetGameInstance().GetCurrentGameMode();
        if (isDead)
            return;

        if (isFrozen) {
            UpdateFreezeTimer();
            return;
        }
        if (hitEffectOn)
            UpdateHitEffectTimer();

        CheckInput();
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
        
        if (isMoving) {
            if (networkObjectComp.IsOwner && currentGameMode == GameMode.LAN && currentPlayerType == PlayerType.PLAYER_2)
                UpdateNetworkedMovement();
            else
                UpdateMovement();
        }
        if (isRotating) {
            if (networkObjectComp.IsOwner && currentGameMode == GameMode.LAN && currentPlayerType == PlayerType.PLAYER_2)
                UpdateNetworkedRotation();
            else
                UpdateRotation();
        }
    }
    private void SetupReferences() {

        Transform explosionSpawnerTransform = transform.Find("ExplosionSpawner");
        Utility.Validate(explosionSpawnerTransform, "Failed to get reference to ExplosionSpawner gameobject - Player", Utility.ValidationLevel.ERROR, true);

        explosionSpawnerScript = explosionSpawnerTransform.GetComponent<ExplosionSpawner>();
        Utility.Validate(explosionSpawnerScript, "Failed to get reference to DeathExplosionSpawner component - Player", Utility.ValidationLevel.ERROR, true);

        spriteRendererComp = GetComponent<SpriteRenderer>();
        Utility.Validate(spriteRendererComp, "Failed to get reference to SpriteRenderer component - Player", Utility.ValidationLevel.ERROR, true);

        rigidbodyComp = GetComponent<Rigidbody2D>();
        Utility.Validate(rigidbodyComp, "Failed to get reference to Rigidbody2D component - Player", Utility.ValidationLevel.ERROR, true);

        boxCollider2DComp = GetComponent<BoxCollider2D>();
        Utility.Validate(boxCollider2DComp, "Failed to get reference to BoxCollider2D component - Player", Utility.ValidationLevel.ERROR, true);

        networkObjectComp = GetComponent<NetworkObject>();
        Utility.Validate(networkObjectComp, "Failed to get reference to NetworkObject component - Player", Utility.ValidationLevel.ERROR, true);

        Transform muzzleFlashSpawnerTransformn = transform.Find("MuzzleFlashSpawner");
        Utility.Validate(muzzleFlashSpawnerTransformn, "Failed to get reference to MuzzleFlashSpawner - Player", Utility.ValidationLevel.ERROR, true);

        muzzleFlashSpawnerScript = muzzleFlashSpawnerTransformn.GetComponent<MuzzleFlashSpawner>();
        Utility.Validate(muzzleFlashSpawnerScript, "Failed to get reference to MuzzleFlashSpawner component - Player", Utility.ValidationLevel.ERROR, true);
    }
    public void SetupStartState() {
        currentHealth = playerCharacterData.statsData.healthCap;
        currentFuel = playerCharacterData.statsData.fuelCap;
        equippedPickup = null;

        freezeTimer = 0.0f;
        isFrozen = false;
        spriteRendererComp.color = defaultColor;

        SetInvincible(false);
        RemoveHitEffect();
        ResetAllHUDData();

        transform.position = spawnPoint;

        boxCollider2DComp.enabled = true;
        isDead = false;
        SetSpriteVisibility(true);
    }
    public override void OnDestroy() {
        base.OnDestroy();

        activeControlScheme.usePickupInput.started -= UsePickupInputCallback;
        activeControlScheme.boostInput.started -= BoostInputCallback;
        activeControlScheme.pauseInput.started -= PauseInputCallback;
        DisableInput();
    }

    public void SetPlayerData(PlayerCharacterData data) {
        playerCharacterData = data;
        spriteRendererComp.sprite = data.shipSprite;
    }
    public void SetPlayerColor(Color color) {
        spriteRendererComp.color = color;
        defaultColor = color;
    }
    public void SetSpriteVisibility(bool state) {
        spriteRendererComp.enabled = state;
    }
    public void SetSpawnPoint(Vector3 position) {
        spawnPoint = position;
    }
    public void SetHUDReference(HUD script) {
        HUDScript = script;
    }
    public void SetPlayerType(PlayerType type) {
        currentPlayerType = type;
    }
    public void SetActiveControlScheme(PlayerType type) {
        if (type == PlayerType.PLAYER_1)
            activeControlScheme = player1ControlScheme;
        else if (type == PlayerType.PLAYER_2)
            activeControlScheme = player2ControlScheme;

        activeControlScheme.usePickupInput.started += UsePickupInputCallback;
        activeControlScheme.boostInput.started += BoostInputCallback;
        activeControlScheme.pauseInput.started += PauseInputCallback;
    }
    public void SetInvincible(bool state) {
        isInvincible = state;
    }

    public PlayerCharacterData GetPlayerData() {
        return playerCharacterData;
    }
    public PlayerType GetPlayerType() {
        return currentPlayerType;
    }
    public float GetCurrentHealth() {
        return currentHealth;
    }
    public float GetCurrentFuel() {
        return currentFuel;
    }
    public Vector2 GetVelocity() {
        return rigidbodyComp.velocity;
    }
    public Vector3 GetSpawnPoint() {
        return spawnPoint;
    }

    private void CheckInput() {
        if (!activeControlScheme)
            return;

        isMoving = activeControlScheme.movementInput.IsPressed();
        isRotating = activeControlScheme.rotationInput.IsPressed();
    }
    private void PauseInputCallback(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
        GetGameInstance().PauseGame();
    }
    private void BoostInputCallback(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
        Boost();
    }
    private void UsePickupInputCallback(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
        UseEquippedPickup();
    }
    public void EnableInput() {
        if (!activeControlScheme)
            return;

        activeControlScheme.movementInput.Enable();
        activeControlScheme.rotationInput.Enable();
        activeControlScheme.boostInput.Enable();
        activeControlScheme.pauseInput.Enable();
        activeControlScheme.usePickupInput.Enable();
    }
    public void DisableInput() {
        if (!activeControlScheme)
            return;

        activeControlScheme.movementInput.Disable();
        activeControlScheme.rotationInput.Disable();
        activeControlScheme.boostInput.Disable();
        activeControlScheme.pauseInput.Disable();
        activeControlScheme.usePickupInput.Disable();
    }
    public void ActivateNetworkedEntity() {
        spriteRendererComp.enabled = true;
        boxCollider2DComp.enabled = true;
        rigidbodyComp.isKinematic = false;
    }
    public void DeactivateNetworkedEntity() {
        spriteRendererComp.enabled = false;
        boxCollider2DComp.enabled = false;
        rigidbodyComp.isKinematic = true;
        rigidbodyComp.velocity = Vector3.zero;
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
        spriteRendererComp.color = defaultColor;
        EnableInput();
    }
    public void ApplyFreeze(float duration) {
        if (isDead)
            return;

        freezeTimer = duration;
        isFrozen = true;
        spriteRendererComp.color = frozenColor;
        DisableInput();
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
    public void ApplyImpulse(Vector2 direction, float force) {
        if (isDead)
            return;

        rigidbodyComp.AddForce(Time.deltaTime * direction * force, ForceMode2D.Impulse);
    }

    private void ResetAllHUDData() {
        HUDScript.UpdateHealth(currentPlayerType, currentHealth / playerCharacterData.statsData.healthCap);
        HUDScript.UpdateFuel(currentPlayerType, currentFuel / playerCharacterData.statsData.fuelCap);
        HUDScript.SetPickupIcon(currentPlayerType, null);
        HUDScript.SetCharacterPortrait(currentPlayerType, playerCharacterData.portraitSprite);
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

    private Vector2 CalculateVelocity(float input) {
        Vector2 velocity = Vector2.zero;
        Vector2 force = Time.fixedDeltaTime * new Vector2(transform.up.x, transform.up.y) * playerCharacterData.statsData.accelerationRate;

        if (input < 0.0f) {
            force *= -1;

            velocity = rigidbodyComp.velocity + force;
            if (velocity.x < -playerCharacterData.statsData.maxVelocity.x)
                velocity.x = -playerCharacterData.statsData.maxVelocity.x;
            if (velocity.y > -playerCharacterData.statsData.maxVelocity.y)
                velocity.y = -playerCharacterData.statsData.maxVelocity.y;
        }
        else if (input > 0.0f) {

            velocity = rigidbodyComp.velocity + force;
            if (velocity.x > playerCharacterData.statsData.maxVelocity.x)
                velocity.x = playerCharacterData.statsData.maxVelocity.x;
            if (velocity.y > playerCharacterData.statsData.maxVelocity.y)
                velocity.y = playerCharacterData.statsData.maxVelocity.y;
        }

        return velocity;
    }
    private float CalculateRotation(float input) {
        float rotation = playerCharacterData.statsData.turnRate;
        if (input < 0.0f)
            rotation *= -1;

        return rotation * Time.fixedDeltaTime;
    }

    private void UpdateMovement() {
        float inputValue = activeControlScheme.movementInput.ReadValue<float>();
        rigidbodyComp.AddForce(CalculateVelocity(inputValue), ForceMode2D.Force);
    }
    private void UpdateRotation() {
        float inputValue = activeControlScheme.rotationInput.ReadValue<float>();
        transform.Rotate(new Vector3(0.0f, 0.0f, CalculateRotation(inputValue)));
    }

    private void UpdateNetworkedMovement() {
        float inputValue = activeControlScheme.movementInput.ReadValue<float>();
        if (inputValue > 0.0f | inputValue < 0.0f)
            GetGameInstance().GetRpcManagerScript().CalculatePlayer2PositionServerRpc(inputValue);
    }
    private void UpdateNetworkedRotation() {
        float inputValue = activeControlScheme.rotationInput.ReadValue<float>();
        if (inputValue > 0.0f || inputValue < 0.0f)
            GetGameInstance().GetRpcManagerScript().CalculatePlayer2RotationServerRpc(inputValue);
    }

    public void ProccessReceivedMovementRpc(float input) {
        rigidbodyComp.AddForce(CalculateVelocity(input), ForceMode2D.Force);
    }
    public void ProccessReceivedRotationRpc(float input) {
        transform.Rotate(new Vector3(0.0f, 0.0f, CalculateRotation(input)));
    }

    public void RegisterPickup(Pickup script, Sprite icon = null) {
        equippedPickup = script;
        HUDScript.SetPickupIcon(currentPlayerType, icon);
    }
    private void UseEquippedPickup() {
        if (!equippedPickup || isDead)
            return;

        if (equippedPickup.Activate(this)) {
            equippedPickup = null;
            HUDScript.SetPickupIcon(currentPlayerType, null);
        }
    }
    private void Boost() {
        if (isDead)
            return;

        if (currentFuel >= playerCharacterData.statsData.boostCost) {
            UseFuel(playerCharacterData.statsData.boostCost);
            rigidbodyComp.AddForce(playerCharacterData.statsData.boostStrength * Time.fixedDeltaTime * transform.up, ForceMode2D.Impulse);
        }
    }
    private void Kill() {
        isDead = true;
        DisableInput();
        rigidbodyComp.velocity = Vector3.zero;
        rigidbodyComp.gravityScale = 0.0f;
        boxCollider2DComp.enabled = false;
        SetSpriteVisibility(false);
        explosionSpawnerScript.PlayAnimation("PlayerDead", null);
        GetGameInstance().RegisterPlayerDeath(currentPlayerType);
    }
    public void ReceivePickupUsageConfirmationRpc() {
        equippedPickup = null;
        HUDScript.SetPickupIcon(currentPlayerType, null);
    }

    public void AddHealth(float amount) {
        if (isDead)
            return;

        if (amount < 0.0f)
            amount *= -1;

        if (currentGameMode == GameMode.COOP) {
            currentHealth += amount;
            if (currentHealth > playerCharacterData.statsData.healthCap)
                currentHealth = playerCharacterData.statsData.healthCap;

            HUDScript.UpdateHealth(currentPlayerType, currentHealth / playerCharacterData.statsData.healthCap);
        }
        else if (currentGameMode == GameMode.LAN && IsOwner)
            GetGameInstance().GetRpcManagerScript().ExecutePlayerHealthProcessServerRpc(RpcManager.PlayerHealthProcess.ADDITION, currentPlayerType, amount);
    }
    public void TakeDamage(float amount) {
        if (isDead || isInvincible)
            return;

        if (amount < 0.0f)
            amount *= -1;

        if (currentGameMode == GameMode.COOP) {
            currentHealth -= amount;
            if (currentHealth <= 0.0f) {
                currentHealth = 0.0f;
                Kill();
            }

            HUDScript.UpdateHealth(currentPlayerType, currentHealth / playerCharacterData.statsData.healthCap);
        }
        else if (currentGameMode == GameMode.LAN && IsOwner)
            GetGameInstance().GetRpcManagerScript().ExecutePlayerHealthProcessServerRpc(RpcManager.PlayerHealthProcess.SUBTRACTION, currentPlayerType, amount);

        ApplyHitEffect();
    }
    public void AddFuel(float amount) {
        if (isDead)
            return;

        if (amount < 0.0f)
            amount *= -1;

        if (currentGameMode == GameMode.COOP && IsOwner) {
            currentFuel += amount;
            if (currentFuel > playerCharacterData.statsData.fuelCap)
                currentFuel = playerCharacterData.statsData.fuelCap;

            HUDScript.UpdateFuel(currentPlayerType, currentFuel / playerCharacterData.statsData.fuelCap);
        }
        else if (currentGameMode == GameMode.LAN)
            GetGameInstance().GetRpcManagerScript().ExecutePlayerFuelProcessServerRpc(RpcManager.PlayerFuelProcess.ADDITION, currentPlayerType, amount);
    }
    public void UseFuel(float amount) {
        if (isDead)
            return;

        if (amount < 0.0f)
            amount *= -1;

        if (currentGameMode == GameMode.COOP && IsOwner) {
            currentFuel -= amount;
            if (currentFuel <= 0.0f)
                currentFuel = 0.0f;

            HUDScript.UpdateFuel(currentPlayerType, currentFuel / playerCharacterData.statsData.fuelCap);
        }
        else if (currentGameMode == GameMode.LAN)
            GetGameInstance().GetRpcManagerScript().ExecutePlayerFuelProcessServerRpc(RpcManager.PlayerFuelProcess.SUBTRACTION, currentPlayerType, amount);
    }

    public void ReceivePlayerHealthProcessRpc(PlayerType player, float health, float percentage) {
        if (player == PlayerType.NONE)
            return;

        if (player == currentPlayerType) {
            currentHealth = health;
            if (currentHealth == 0.0f)
                Kill();

            HUDScript.UpdateHealth(currentPlayerType, percentage);
        }
        else
            HUDScript.UpdateHealth(player, percentage);
    }
    public void ReceivePlayerFuelProcessRpc(PlayerType player, float fuel, float percentage) {
        if (player == PlayerType.NONE)
            return;

        if (player == currentPlayerType) {
            currentFuel = fuel;
            HUDScript.UpdateFuel(currentPlayerType, percentage);
        }
        else
            HUDScript.UpdateFuel(player, percentage);
    }
    
    public bool PlayMuzzleFlashAnim(string name, Action callback, Vector3 customSize) {
        return muzzleFlashSpawnerScript.PlayAnimation(name, callback, customSize);
    }
    public Vector3 GetMuzzleFlashPosition() {
        return muzzleFlashSpawnerScript.transform.position;
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        if (isDead)
            return;

        var script = collision.gameObject.GetComponent<Player>();
        if (script) {
            var otherPlayerVelocity = script.GetVelocity();
            float otherPlayerForce = otherPlayerVelocity.magnitude;
            float ownForce = rigidbodyComp.velocity.magnitude;

            if (ownForce > otherPlayerForce)
                script.ApplyImpulse(transform.up, playerCharacterData.statsData.impulseStrength);
            else if (ownForce == otherPlayerForce)
                script.ApplyImpulse(transform.up, playerCharacterData.statsData.impulseStrength / 2);
        }
    }
}
