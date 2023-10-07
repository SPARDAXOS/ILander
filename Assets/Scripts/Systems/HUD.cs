using ILanderUtility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class HUD : MonoBehaviour
{
    private bool initialized = false;

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

        Transform player1HUD = transform.Find("Player1HUD");
        Transform player2HUD = transform.Find("Player2HUD");
        Utility.Validate(player1HUD, "Failed to get reference to Player1HUD - HUD", true);
        Utility.Validate(player1HUD, "Failed to get reference to player2HUD - HUD", true);

        Transform player1FuelBarTransform = player1HUD.Find("FuelBarFill");
        Transform player2FuelBarTransform = player2HUD.Find("FuelBarFill");
        Utility.Validate(player1FuelBarTransform, "Failed to get reference to FuelBarFill1 - HUD", true);
        Utility.Validate(player2FuelBarTransform, "Failed to get reference to FuelBarFill2 - HUD", true);

        player1FuelBar = player1FuelBarTransform.GetComponent<Image>();
        player2FuelBar = player2FuelBarTransform.GetComponent<Image>();
        Utility.Validate(player1FuelBar, "Failed to get component Image in player1FuelBar - HUD", true);
        Utility.Validate(player2FuelBar, "Failed to get component Image in player2FuelBar - HUD", true);

        Transform player1HealthBarTransform = player1HUD.Find("HealthBarFill");
        Transform player2HealthBarTransform = player2HUD.Find("HealthBarFill");
        Utility.Validate(player1HealthBarTransform, "Failed to get reference to HealthBarFill1 - HUD", true);
        Utility.Validate(player2HealthBarTransform, "Failed to get reference to HealthBarFill2 - HUD", true);

        player1HealthBar = player1HealthBarTransform.GetComponent<Image>();
        player2HealthBar = player2HealthBarTransform.GetComponent<Image>();
        Utility.Validate(player1HealthBar, "Failed to get component Image in player1HealthBar - HUD", true);
        Utility.Validate(player2HealthBar, "Failed to get component Image in player2HealthBar - HUD", true);

        Transform player1Background = player1HUD.Find("Background");
        Transform player2Background = player2HUD.Find("Background");
        Utility.Validate(player1Background, "Failed to get reference to Background1 - HUD", true);
        Utility.Validate(player2Background, "Failed to get reference to Background2 - HUD", true);

        Transform player1PortraitTransform = player1Background.Find("Portrait");
        Transform player2PortraitTransform = player2Background.Find("Portrait");
        Utility.Validate(player1PortraitTransform, "Failed to get reference to Portrait1 - HUD", true);
        Utility.Validate(player2PortraitTransform, "Failed to get reference to Portrait2 - HUD", true);

        player1Portrait = player1PortraitTransform.GetComponent<Image>();
        player2Portrait = player2PortraitTransform.GetComponent<Image>();
        Utility.Validate(player1Portrait, "Failed to get component Image in player1Portrait - HUD", true);
        Utility.Validate(player2Portrait, "Failed to get component Image in player2Portrait - HUD", true);

        Transform player1PickupIconTransform = player1Background.Find("PickupIcon");
        Transform player2PickupIconTransform = player2Background.Find("PickupIcon");
        Utility.Validate(player1PickupIconTransform, "Failed to get reference to PickupIcon1 - HUD", true);
        Utility.Validate(player2PickupIconTransform, "Failed to get reference to PickupIcon2 - HUD", true);

        player1PickupIcon = player1PickupIconTransform.GetComponent<Image>();
        player2PickupIcon = player2PickupIconTransform.GetComponent<Image>();
        Utility.Validate(player1PickupIcon, "Failed to get component Image in player1PickupIcon - HUD", true);
        Utility.Validate(player2PickupIcon, "Failed to get component Image in player2PickupIcon - HUD", true);

        player1PickupIcon.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
        player2PickupIcon.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
    }


    public void UpdateFuel(Player.PlayerType type, float amount) {
        Utility.Clamp(ref amount, 0.0f, 1.0f);
        if (type == Player.PlayerType.PLAYER_1)
            player1FuelBar.fillAmount = amount;
        else if (type == Player.PlayerType.PLAYER_2)
            player2FuelBar.fillAmount = amount;
    }
    public void UpdateHealth(Player.PlayerType type, float amount) {
        Utility.Clamp(ref amount, 0.0f, 1.0f);
        if (type == Player.PlayerType.PLAYER_1)
            player1HealthBar.fillAmount = amount;
        else if (type == Player.PlayerType.PLAYER_2)
            player2HealthBar.fillAmount = amount;
    }
    public void SetCharacterPortrait(Player.PlayerType type, Sprite portrait) {
        if (type == Player.PlayerType.PLAYER_1)
            player1Portrait.sprite = portrait;
        else if (type == Player.PlayerType.PLAYER_2)
            player2Portrait.sprite = portrait;
    }
    public void SetPickupIcon(Player.PlayerType type, Sprite icon) {
        if (type == Player.PlayerType.PLAYER_1) {
            player1PickupIcon.sprite = icon;
            if (!icon)
                player1PickupIcon.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
            else
                player1PickupIcon.color = Color.white;
        }
        else if (type == Player.PlayerType.PLAYER_2) {
            player2PickupIcon.sprite = icon;
            if (!icon)
                player2PickupIcon.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
            else
                player2PickupIcon.color = Color.white;
        }
    }
}