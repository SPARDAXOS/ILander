using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    private bool initialized = false;

    //NOTE: Both players share the same hud but differnt indices? would work for online mode too!

    private Image player1Portrait;
    private Image player2Portrait;

    private Image player1PickupIcon;
    private Image player2PickupIcon;

    private Image player1FuelBar;
    private Image player2FuelBar;

    private Image player1HealthBar;
    private Image player2HealthBar;


    public void Initialize() {
        if (initialized) 
            return;

        SetupReferences();
        initialized = true;
    }


    private void SetupReferences() {
        //TODO: Add validations

        Transform player1HUD = transform.Find("Player1HUD");
        Transform player2HUD = transform.Find("Player2HUD");

        Transform player1FuelBarTransform = player1HUD.Find("FuelBarFill");
        Transform player2FuelBarTransform = player2HUD.Find("FuelBarFill");
        player1FuelBar = player1FuelBarTransform.GetComponent<Image>();
        player2FuelBar = player2FuelBarTransform.GetComponent<Image>();

        Transform player1HealthBarTransform = player1HUD.Find("HealthBarFill");
        Transform player2HealthBarTransform = player2HUD.Find("HealthBarFill");
        player1HealthBar = player1HealthBarTransform.GetComponent<Image>();
        player2HealthBar = player2HealthBarTransform.GetComponent<Image>();


        Transform player1Background = player1HUD.Find("Background");
        Transform player2Background = player2HUD.Find("Background");

        Transform player1PortraitTransform = player1Background.Find("Portrait");
        Transform player2PortraitTransform = player2Background.Find("Portrait");
        player1Portrait = player1PortraitTransform.GetComponent<Image>();
        player2Portrait = player2PortraitTransform.GetComponent<Image>();


        Transform player1PickupIconTransform = player1Background.Find("PickupIcon");
        Transform player2PickupIconTransform = player2Background.Find("PickupIcon");
        player1PickupIcon = player1PickupIconTransform.GetComponent<Image>();
        player2PickupIcon = player2PickupIconTransform.GetComponent<Image>();
    }


    public void UpdateFuel(Player.PlayerType type, float amount) {
        //Check invalid user input then apply data to specific bar
    }
    public void UpdateHealth(Player.PlayerType type, float amount) {
        //Check invalid user input then apply data to specific bar
    }
    public void SetCharacterPortrait(Player.PlayerType type, Sprite portrait) {

    }
    public void SetEquippedPickupIcon(Player.PlayerType type, Sprite icon) {

    }
}
