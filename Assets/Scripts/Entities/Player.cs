using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Player : MonoBehaviour
{

    [SerializeField] private PlayerControlScheme controlScheme;
    [SerializeField] private PlayerData playerData;


    public float currentThrusterStrength = 0.0f;
    private Vector3 thrusterDirection = Vector2.zero;

    public void Initialize() {
        //Add check to avoid allowing multiple calls to this!
        EnableInput();
    }
    public void Tick() {

        CheckInput();
        UpdateMovement();
    }

    private void CheckInput()
    {
        //Break into 2 funcs - rot and move
        if (controlScheme.boosterInput.IsPressed()) {
            float inputValue = controlScheme.boosterInput.ReadValue<float>();
            if (inputValue < 0.0f)
                BreakThruster();
            else if (inputValue > 0.0f)
                AccelerateThruster();

            thrusterDirection = transform.up;


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

        if (controlScheme.rotationInput.IsPressed()) {
            float inputValue = controlScheme.rotationInput.ReadValue<float>();
            float Speed = playerData.turnRate;
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
        currentThrusterStrength += playerData.thrusterAccelerationRate * Time.deltaTime;
        if (currentThrusterStrength >= playerData.thrusterStrengthLimit) {
            currentThrusterStrength = playerData.thrusterStrengthLimit;
            //Stuff
        }
    }
    private void DeccelerateThruster() {
        currentThrusterStrength -= playerData.thrusterDecelerationRate * Time.deltaTime;
        if (currentThrusterStrength <= 0.0f) {
            currentThrusterStrength = 0.0f;
            //Stuff
        }
    }
    private void BreakThruster() {
        Debug.Log("Breaks!");
        currentThrusterStrength -= playerData.thrusterBreaksRate * Time.deltaTime;
        if (currentThrusterStrength <= 0.0f) {
            currentThrusterStrength = 0.0f;
            //Stuff
        }
    }

    private void EnableInput() {
        controlScheme.boosterInput.Enable();
        controlScheme.rotationInput.Enable();
    }
    private void DisableInput() {
        controlScheme.boosterInput.Disable();
        controlScheme.rotationInput.Disable();
    }
}
