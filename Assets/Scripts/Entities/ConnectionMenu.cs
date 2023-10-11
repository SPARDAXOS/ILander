using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ILanderUtility;

public class ConnectionMenu : MonoBehaviour
{
    public enum ConnectionMenuMode
    {
        NONE = 0,
        NORMAL,
        HOST,
        CLIENT
    }

    private ConnectionMenuMode currentConnectionMenuMode = ConnectionMenuMode.NONE;

    private bool initialized = false;


    private GameObject normalMode;
    private GameObject hostMode;
    private GameObject clientMode;


    public void Initialize() {
        if (initialized)
            return;



        SetupReferences();
        SetConnectionMenuMode(ConnectionMenuMode.NORMAL);
        initialized = true;
    }

    private void SetupReferences() {


        normalMode = transform.Find("NormalMode").gameObject;
        hostMode   = transform.Find("HostMode").gameObject;
        clientMode = transform.Find("ClientMode").gameObject;

        Utility.Validate(normalMode, "Failed to find reference for NormalMode - ConnectionMenu", Utility.ValidationLevel.ERROR, true);
        Utility.Validate(hostMode, "Failed to find reference for HostMode - ConnectionMenu", Utility.ValidationLevel.ERROR, true);
        Utility.Validate(clientMode, "Failed to find reference for ClientMode - ConnectionMenu", Utility.ValidationLevel.ERROR, true);
    }


    public void SetConnectionMenuMode(ConnectionMenuMode mode) {
        if (mode == currentConnectionMenuMode)
            return;

        currentConnectionMenuMode = mode;
        normalMode.SetActive(false);
        hostMode.SetActive(false);
        clientMode.SetActive(false);

        if (mode == ConnectionMenuMode.NONE)
            return;

        if (mode == ConnectionMenuMode.NORMAL)
            normalMode.SetActive(true);
        else if (mode == ConnectionMenuMode.HOST)
            hostMode.SetActive(true);
        else if (mode == ConnectionMenuMode.CLIENT)
            clientMode.SetActive(true);
    }

    public void HostButton() {
        GameInstance.GetGameInstance().StartAsHost();
        SetConnectionMenuMode(ConnectionMenuMode.HOST);
        //Transition? or rework menu mode
    }
    public void JoinButton() {
        GameInstance.GetGameInstance().StartAsClient();
        SetConnectionMenuMode(ConnectionMenuMode.CLIENT);
        //Transition? or rework menu mode
    }
}
