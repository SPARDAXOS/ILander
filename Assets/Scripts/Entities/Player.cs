using Initialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using ILanderUtility;
using Unity.Netcode;
using Unity.Multiplayer;

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

    public float currentThrusterStrength = 0.0f;
    private Vector3 thrusterDirection = Vector2.zero;


    private SpriteRenderer spriteRendererComp;
    private NetworkObject networkObjectComp;


    public void Initialize() {
        if (initialized)
            return;

        //Disable networking by defualt?
        SetupReferences();
        initialized = true;
        Debug.Log("Initialized!");
        //EnableInput(); Call this from instance instead at game start!
    }
    public void Tick() {
        if (!initialized) {
            Debug.LogError("Attempted to tick uninitialized entity - " + gameObject.name);
            return;
        }

        //Problem is that the struct is not nullable. Either make it so or just roll with it!
        if (currentPlayerType == PlayerType.NONE)
            return;//mESSGE?

        CheckInput();
        UpdateMovement();
    }



    private void SetupReferences() {

        spriteRendererComp = GetComponent<SpriteRenderer>();
        Utility.Validate(spriteRendererComp, "Failed to get reference to SpriteRenderer component - Player", true);

        networkObjectComp = GetComponent<NetworkObject>();
        Utility.Validate(networkObjectComp, "Failed to get reference to NetworkObject component - Player", true);
        
    }



    public void SetPlayerType(PlayerType type) {
        currentPlayerType = type;
        if (type == PlayerType.PLAYER_1)
            activeControlScheme = player1ControlScheme;
        else if (type == PlayerType.PLAYER_2)
            activeControlScheme = player2ControlScheme;

        //Other stuff?
    }
    //Get skin instead! Change skin to data and data to stats?
    public void SetPlayerData(PlayerCharacterData data) {
        playerCharacterData = data;

        spriteRendererComp.sprite = data.shipSprite;
        //Apply HUD data! probably have 2 hud modes cause if online or not!
        //Apply data from it!
    }
    public void SetPlayerColor(Color color) {
        spriteRendererComp.color = color;
    }


    private void CheckInput()
    {
        //Break into 2 funcs - rot and move
        if (activeControlScheme.boosterInput.IsPressed()) {
            float inputValue = activeControlScheme.boosterInput.ReadValue<float>();
            if (inputValue < 0.0f)
                BreakThruster();
            else if (inputValue > 0.0f)
                AccelerateThruster();

            thrusterDirection = Vector3.Normalize(thrusterDirection + transform.up * playerCharacterData.statsData.steeringRate);


            //    //Temp
            //    currentThrusterStrength = 7.0f;

            //Vector3 current = transform.position;
            //float Speed = currentThrusterStrength;
            //if (inputValue < 0.0f)
            //    Speed *= -1;

            //transform.position = current + (transform.up * Speed * Time.deltaTime);
        }
        else if (currentThrusterStrength > 0.0f)
            DeccelerateThruster();

        if (activeControlScheme.rotationInput.IsPressed()) {
            float inputValue = activeControlScheme.rotationInput.ReadValue<float>();
            float Speed = playerCharacterData.statsData.turnRate;
            if (inputValue < 0.0f)
                Speed *= -1;

            transform.Rotate(new Vector3(0.0f, 0.0f, Speed));
        }

        //NOTE: Input just increases or decreases the thrusters. 
        //Current Thruster value is constantly applied to movement!
        //NOTE: Carefull cause Tick is called in update and not FIXEDUPDATE!!!!!!!!!!!!!!!!!!!!!!!!!!!
    }
    private void UpdateMovement() {
        Vector3 current = transform.position;
        //Add to current "velocity" to make it adjust instead of snap!
        transform.position = current + (thrusterDirection * currentThrusterStrength * Time.deltaTime); 
        //Unless i go with velocity instead. Assignment wants that i think!
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

    //SetMode? for coop and lan modes!

    public void EnableInput() {
        activeControlScheme.boosterInput.Enable();
        activeControlScheme.rotationInput.Enable();
    }
    public void DisableInput() {
        activeControlScheme.boosterInput.Disable();
        activeControlScheme.rotationInput.Disable();
    }
}
