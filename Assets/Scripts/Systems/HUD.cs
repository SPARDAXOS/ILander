using ILanderUtility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


struct HUDDataSet {
    public float health;
    public float fuel;
    public Sprite pickupIcon; //Is this even needed?
}

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

    private Animation player1HealthBarAnimation;
    private Animation player2HealthBarAnimation;

    private Animation player1FuelBarAnimation;
    private Animation player2FuelBarAnimation;

    private Animation player1PortraitAnimation;
    private Animation player2PortraitAnimation;


    public void Initialize() {
        if (initialized) 
            return;

        SetupReferences();
        initialized = true;
    }
    private void SetupReferences() {

        Transform player1HUD = transform.Find("Player1HUD");
        Transform player2HUD = transform.Find("Player2HUD");
        Utility.Validate(player1HUD, "Failed to get reference to Player1HUD - HUD", Utility.ValidationLevel.ERROR, true);
        Utility.Validate(player1HUD, "Failed to get reference to player2HUD - HUD", Utility.ValidationLevel.ERROR, true);

        Transform player1FuelBarTransform = player1HUD.Find("FuelBarFill");
        Transform player2FuelBarTransform = player2HUD.Find("FuelBarFill");
        Utility.Validate(player1FuelBarTransform, "Failed to get reference to FuelBarFill1 - HUD", Utility.ValidationLevel.ERROR, true);
        Utility.Validate(player2FuelBarTransform, "Failed to get reference to FuelBarFill2 - HUD", Utility.ValidationLevel.ERROR, true);

        player1FuelBar = player1FuelBarTransform.GetComponent<Image>();
        player2FuelBar = player2FuelBarTransform.GetComponent<Image>();
        Utility.Validate(player1FuelBar, "Failed to get component Image in player1FuelBar - HUD", Utility.ValidationLevel.ERROR, true);
        Utility.Validate(player2FuelBar, "Failed to get component Image in player2FuelBar - HUD", Utility.ValidationLevel.ERROR, true);

        player1FuelBarAnimation = player1FuelBar.GetComponent<Animation>();
        player2FuelBarAnimation = player2FuelBar.GetComponent<Animation>();
        Utility.Validate(player1FuelBarAnimation, "Failed to get component Animation in player1FuelBar - HUD", Utility.ValidationLevel.ERROR, true);
        Utility.Validate(player2FuelBarAnimation, "Failed to get component Animation in player2FuelBar - HUD", Utility.ValidationLevel.ERROR, true);


        Transform player1HealthBarTransform = player1HUD.Find("HealthBarFill");
        Transform player2HealthBarTransform = player2HUD.Find("HealthBarFill");
        Utility.Validate(player1HealthBarTransform, "Failed to get reference to HealthBarFill1 - HUD", Utility.ValidationLevel.ERROR, true);
        Utility.Validate(player2HealthBarTransform, "Failed to get reference to HealthBarFill2 - HUD", Utility.ValidationLevel.ERROR, true);

        player1HealthBar = player1HealthBarTransform.GetComponent<Image>();
        player2HealthBar = player2HealthBarTransform.GetComponent<Image>();
        Utility.Validate(player1HealthBar, "Failed to get component Image in player1HealthBar - HUD", Utility.ValidationLevel.ERROR, true);
        Utility.Validate(player2HealthBar, "Failed to get component Image in player2HealthBar - HUD", Utility.ValidationLevel.ERROR, true);


        player1HealthBarAnimation = player1HealthBar.GetComponent<Animation>();
        player2HealthBarAnimation = player2HealthBar.GetComponent<Animation>();
        Utility.Validate(player1HealthBarAnimation, "Failed to get component Animation in player1HealthBar - HUD", Utility.ValidationLevel.ERROR, true);
        Utility.Validate(player2HealthBarAnimation, "Failed to get component Animation in player2HealthBar - HUD", Utility.ValidationLevel.ERROR, true);


        Transform player1Background = player1HUD.Find("Background");
        Transform player2Background = player2HUD.Find("Background");
        Utility.Validate(player1Background, "Failed to get reference to Background1 - HUD", Utility.ValidationLevel.ERROR, true);
        Utility.Validate(player2Background, "Failed to get reference to Background2 - HUD", Utility.ValidationLevel.ERROR, true);

        Transform player1PortraitTransform = player1Background.Find("Portrait");
        Transform player2PortraitTransform = player2Background.Find("Portrait");
        Utility.Validate(player1PortraitTransform, "Failed to get reference to Portrait1 - HUD", Utility.ValidationLevel.ERROR, true);
        Utility.Validate(player2PortraitTransform, "Failed to get reference to Portrait2 - HUD", Utility.ValidationLevel.ERROR, true);

        player1Portrait = player1PortraitTransform.GetComponent<Image>();
        player2Portrait = player2PortraitTransform.GetComponent<Image>();
        Utility.Validate(player1Portrait, "Failed to get component Image in player1Portrait - HUD", Utility.ValidationLevel.ERROR, true);
        Utility.Validate(player2Portrait, "Failed to get component Image in player2Portrait - HUD", Utility.ValidationLevel.ERROR, true);


        player1PortraitAnimation = player1PortraitTransform.GetComponent<Animation>();
        player2PortraitAnimation = player2PortraitTransform.GetComponent<Animation>();
        Utility.Validate(player1PortraitAnimation, "Failed to get component Animation in player1Portrait - HUD", Utility.ValidationLevel.ERROR, true);
        Utility.Validate(player2PortraitAnimation, "Failed to get component Animation in player2Portrait - HUD", Utility.ValidationLevel.ERROR, true);


        Transform player1PickupIconTransform = player1Background.Find("PickupIcon");
        Transform player2PickupIconTransform = player2Background.Find("PickupIcon");
        Utility.Validate(player1PickupIconTransform, "Failed to get reference to PickupIcon1 - HUD", Utility.ValidationLevel.ERROR, true);
        Utility.Validate(player2PickupIconTransform, "Failed to get reference to PickupIcon2 - HUD", Utility.ValidationLevel.ERROR, true);

        player1PickupIcon = player1PickupIconTransform.GetComponent<Image>();
        player2PickupIcon = player2PickupIconTransform.GetComponent<Image>();
        Utility.Validate(player1PickupIcon, "Failed to get component Image in player1PickupIcon - HUD", Utility.ValidationLevel.ERROR, true);
        Utility.Validate(player2PickupIcon, "Failed to get component Image in player2PickupIcon - HUD", Utility.ValidationLevel.ERROR, true);

        player1PickupIcon.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
        player2PickupIcon.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
    }


    public void UpdateFuel(Player.PlayerType type, float amount) {
        Utility.Clamp(ref amount, 0.0f, 1.0f);

        if (type == Player.PlayerType.PLAYER_1) {
            if (amount < player1FuelBar.fillAmount && !player1FuelBarAnimation.isPlaying) //On Decrease
                player1FuelBarAnimation.Play("FuelBar_FuelUsed");
            else if (amount > player1FuelBar.fillAmount && !player1FuelBarAnimation.isPlaying) //On Increase
                player1FuelBarAnimation.Play("FuelBar_FuelAdded");

            player1FuelBar.fillAmount = amount;
        }
        else if (type == Player.PlayerType.PLAYER_2) {
            if (amount < player2FuelBar.fillAmount && !player2FuelBarAnimation.isPlaying) //On Decrease
                player2FuelBarAnimation.Play("FuelBar_FuelUsed");
            else if (amount > player2FuelBar.fillAmount && !player2FuelBarAnimation.isPlaying) //On Increase
                player2FuelBarAnimation.Play("FuelBar_FuelAdded");

            player2FuelBar.fillAmount = amount;
        }
    }
    public void UpdateHealth(Player.PlayerType type, float amount) {
        Utility.Clamp(ref amount, 0.0f, 1.0f);

        if (type == Player.PlayerType.PLAYER_1) {
            if (amount < player1HealthBar.fillAmount && !player1HealthBarAnimation.isPlaying) { //On Decrease
                player1HealthBarAnimation.Play("HealthBar_DamageTaken");
                player1PortraitAnimation.Play("Portrait_DamageTaken");
            }
            else if (amount > player1HealthBar.fillAmount && !player1HealthBarAnimation.isPlaying) { //On Increase
                player1HealthBarAnimation.Play("HealthBar_HealthAdded");
                player1PortraitAnimation.Play("Portrait_HealthAdded");
            }

            player1HealthBar.fillAmount = amount;
        }
        else if (type == Player.PlayerType.PLAYER_2) {
            if (amount < player2HealthBar.fillAmount && !player2HealthBarAnimation.isPlaying) {
                player2HealthBarAnimation.Play("HealthBar_DamageTaken");
                player2PortraitAnimation.Play("Portrait_DamageTaken");
            }
            else if (amount > player2HealthBar.fillAmount && !player2HealthBarAnimation.isPlaying) {
                player2HealthBarAnimation.Play("HealthBar_HealthAdded");
                player2PortraitAnimation.Play("Portrait_HealthAdded");
            }

            player2HealthBar.fillAmount = amount;
        }

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
