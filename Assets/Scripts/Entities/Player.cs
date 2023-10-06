using Initialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using ILanderUtility;
using Unity.Netcode;
using Unity.Multiplayer;
using static GameInstance;

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

    public bool initialized = false;
    private PlayerCharacterData playerCharacterData; //Add Check to validate if player has data before doing any updates!
    private PlayerType currentPlayerType = PlayerType.NONE;


    private PlayerControlScheme activeControlScheme;
    private HUD HUDScript;

    public float currentThrusterStrength = 0.0f;
    private Vector2 thrusterDirection = Vector2.zero;

    public bool whilemoving = false;


    public float currentHealth = 0.0f;
    public float currentFuel = 0.0f;
    public Pickup equippedPickup = null;

    private bool isMoving = false;
    private bool isRotating = false;
    private bool isBoosting = false;

    private SpriteRenderer spriteRendererComp;
    private Rigidbody2D rigidbodyComp;
    private NetworkObject networkObjectComp;


    public void Initialize() {
        if (initialized)
            return;

        //Disable networking by defualt?
        SetupReferences();
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

        CheckInput();

        if (isRotating)
            UpdateRotation(); //idk if should be in fixed input!
    }
    public void FixedTick() {
        if (!initialized) {
            Debug.LogError("Attempted to tick uninitialized entity - " + gameObject.name);
            return;
        }


        UpdateGravity();
        UpdateDrag();

        if (isBoosting)
            Boost();
        if (isMoving)
            UpdateMovement();
    }
    private void SetupReferences() {

        spriteRendererComp = GetComponent<SpriteRenderer>();
        Utility.Validate(spriteRendererComp, "Failed to get reference to SpriteRenderer component - Player", true);

        rigidbodyComp = GetComponent<Rigidbody2D>();
        Utility.Validate(rigidbodyComp, "Failed to get reference to Rigidbody2D component - Player", true);

        networkObjectComp = GetComponent<NetworkObject>();
        Utility.Validate(networkObjectComp, "Failed to get reference to NetworkObject component - Player", true);
        
    }
    public void SetupStartState() {
        currentHealth = playerCharacterData.statsData.healthCap;
        currentFuel = playerCharacterData.statsData.fuelCap;
        UpdateAllHUDData();
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

        player2ControlScheme.pauseInput.performed += PauseInputCallback;
    }

    private void PauseInputCallback(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
        Debug.Log("Pause callback");
        Pause();
    }

    public void SetPlayerData(PlayerCharacterData data) {
        playerCharacterData = data;
        spriteRendererComp.sprite = data.shipSprite;
    }
    public void SetPlayerColor(Color color) {
        spriteRendererComp.color = color;
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
        isBoosting = activeControlScheme.boostInput.triggered;
        //isUsingPickup = activeControlScheme.usePickupInput.triggered;
        //pause




        //Break into 2 funcs - rot and move
        // if (moving) {
        //     float inputValue = activeControlScheme.movementInput.ReadValue<float>();
        //     if (inputValue < 0.0f)
        //         BreakThruster();
        //     else if (inputValue > 0.0f)
        //         AccelerateThruster();
        //
        //     thrusterDirection = new Vector2(transform.up.x, transform.up.y); //* playerCharacterData.statsData.steeringRate;
        // }
        // else if (currentThrusterStrength > 0.0f)
        //     DeccelerateThruster();



        //if (moving)
        //    rigidbodyComp.gravityScale = 0.0f;
        //else
        //    rigidbodyComp.gravityScale = playerCharacterData.statsData.gravityScale;



        //NOTE: Input just increases or decreases the thrusters. 
        //Current Thruster value is constantly applied to movement!
        //NOTE: Carefull cause Tick is called in update and not FIXEDUPDATE!!!!!!!!!!!!!!!!!!!!!!!!!!!
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
        //BUG: Boosting input doesnt trigger most of the time

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






        //Debug.Log("Value " + Time.fixedDeltaTime * transform.up * playerCharacterData.statsData.accelerationRate);

        //Vector2 result = currentThrusterStrength * Time.fixedDeltaTime * thrusterDirection;

        //if (result.x > playerCharacterData.statsData.maxSpeed)
        //    result.x = playerCharacterData.statsData.maxSpeed;
        //if (result.y > playerCharacterData.statsData.maxSpeed)
        //    result.y = playerCharacterData.statsData.maxSpeed;


        //if (currentPlayerType == PlayerType.PLAYER_1) {
        //    Debug.Log("Strength " + currentThrusterStrength);
        //    Debug.Log("Direction " + thrusterDirection);
        //    //Debug.Log("Velocity " + (Time.fixedDeltaTime * thrusterDirection * result));
        //}

        //rigidbodyComp.velocity = result;


        //rigidbodyComp.AddForce(speed * Time.fixedDeltaTime * transform.up, ForceMode2D.Force);
        //Debug.Log("Velocity " + (Time.fixedDeltaTime * transform.up * speed));


        //rigidbodyComp.AddForce(Time.fixedDeltaTime * thrusterDirection * result, ForceMode2D.Force);
    }



    private void AccelerateThruster() {
        currentThrusterStrength += playerCharacterData.statsData.thrusterAccelerationRate * Time.deltaTime;
        if (currentThrusterStrength >= playerCharacterData.statsData.thrusterStrengthLimit) {
            currentThrusterStrength = playerCharacterData.statsData.thrusterStrengthLimit;
            //Stuff
        }
    }
    private void DeccelerateThruster() {
        currentThrusterStrength -= playerCharacterData.statsData.thrusterDecelerationRate * Time.deltaTime;
        if (currentThrusterStrength <= 0.0f) {
            currentThrusterStrength = 0.0f;
            //Stuff
        }
    }
    private void BreakThruster() {
        Debug.Log("Breaks!");
        currentThrusterStrength -= playerCharacterData.statsData.thrusterBreaksRate * Time.deltaTime;
        if (currentThrusterStrength <= 0.0f) {
            currentThrusterStrength = 0.0f;
            //Stuff
        }
    }



    private void Boost() {
        if(currentFuel >= playerCharacterData.statsData.boostCost)
            UseFuel(playerCharacterData.statsData.boostCost);
        rigidbodyComp.AddForce(Time.fixedDeltaTime * transform.up * playerCharacterData.statsData.boostStrength, ForceMode2D.Impulse);
        Debug.Log("BOOST!");
    }
    private void Pause() {
        GetInstance().SetGameState(GameState.PAUSE_MENU);
    }


    public void RegisterPickup(Pickup script, Sprite icon = null) {
        equippedPickup = script;
        HUDScript.SetPickupIcon(currentPlayerType, icon);
    }
    private void UseEquippedPickup() {
        if (!equippedPickup)
            return;

        equippedPickup.Activate(this);
        equippedPickup = null; //? is this good enough? no resets of any kind?
        HUDScript.SetPickupIcon(currentPlayerType, null);
    }


    public void AddHealth(float amount) {
        if (amount < 0.0f)
            amount *= -1;

        currentHealth += amount;
        if (currentHealth > playerCharacterData.statsData.healthCap)
            currentHealth = playerCharacterData.statsData.healthCap;

        Debug.Log("Health Pickup Used!");
        HUDScript.UpdateHealth(currentPlayerType, currentHealth / playerCharacterData.statsData.healthCap);
    }
    public void TakeDamage(float amount) {
        if (amount < 0.0f)
            amount *= -1;

        currentHealth -= amount;
        if (currentHealth <= 0.0f) {
            currentHealth = 0.0f;
            Debug.Log("Player " + gameObject.name + " is dead!");
            //DEAD!
        }
        HUDScript.UpdateHealth(currentPlayerType, currentHealth / playerCharacterData.statsData.healthCap);
        Debug.Log("Damage Taken!");
    }
    public void AddFuel(float amount) {
        if (amount < 0.0f)
            amount *= -1;

        currentFuel += amount;
        if (currentFuel > playerCharacterData.statsData.fuelCap)
            currentFuel = playerCharacterData.statsData.fuelCap;

        Debug.Log("Fuel Pickup Used!");
        HUDScript.UpdateFuel(currentPlayerType, currentFuel / playerCharacterData.statsData.fuelCap);
    }
    public void UseFuel(float amount) {
        if (amount < 0.0f)
            amount *= -1;

        currentFuel -= amount;
        if (currentFuel <= 0.0f) {
            currentFuel = 0.0f;
            //Out of fuel?
        }
        HUDScript.UpdateFuel(currentPlayerType, currentFuel / playerCharacterData.statsData.fuelCap);
        Debug.Log("Fuel Used! " + amount);
    }



    public void SetSpriteVisible(bool state) {
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
        rigidbodyComp.AddForce(Time.fixedDeltaTime * transform.up * playerCharacterData.statsData.boostStrength, ForceMode2D.Impulse);
    }
    private void OnCollisionEnter2D(Collision2D collision) {
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
