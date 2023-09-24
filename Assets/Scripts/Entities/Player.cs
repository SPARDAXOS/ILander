using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{

    [SerializeField] private PlayerControlScheme controlScheme;
    [SerializeField] private PlayerData playerData;


    private float currentThrusterStrength = 0.0f;

    public void Initialize() {
        EnableInput();
    }
    public void Tick() {

        CheckInput();
    }

    private void CheckInput()
    {
        if (controlScheme.boosterInput.IsPressed()) {
            float inputValue = controlScheme.boosterInput.ReadValue<float>();

            //Temp
            currentThrusterStrength = 7.0f;

            Vector3 current = transform.position;
            float Speed = currentThrusterStrength;
            if (inputValue < 0.0f)
                Speed *= -1;

            transform.position = current + (transform.up * Speed * Time.deltaTime);
        }
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


        //transform.position = current + (transform.up * currentThrusterStrength * Time.deltaTime); 
        //Unless i go with velocity instead. Assignment wants that i think!
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
