using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "PlayerControlScheme", menuName = "Data/PlayerControlScheme", order = 3)]
public class PlayerControlScheme : ScriptableObject
{
    public InputAction movementInput;
    public InputAction rotationInput;
    public InputAction boostInput;
    public InputAction usePickupInput;
    public InputAction pauseInput;


}
