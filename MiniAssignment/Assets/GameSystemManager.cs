using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameSystemManager : MonoBehaviour
{

    GameObject inputFieldUserName, inputFieldPassword, buttonSubmit, toggleLogin, toggleCreate;
    GameObject networkedClient;

    // Start is called before the first frame update
    void Start()
    {
        GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();

        foreach (GameObject go in allObjects)
        {
            if(go.name == "inputFieldUserName")
                inputFieldUserName = go;
            else if(go.name == "inputFieldPassword")
                inputFieldPassword = go;
            else if(go.name == "buttonSubmit")
                buttonSubmit = go;
            else if(go.name == "toggleLogin")
                toggleLogin = go;
            else if(go.name == "toggleCreate")
                toggleCreate = go;
            else if (go.name == "NetworkedClient")
                networkedClient = go;
        }

    buttonSubmit.GetComponent<Button>().onClick.AddListener(SubmitButtonPressed);
    toggleCreate.GetComponent<Toggle>().onValueChanged.AddListener(ToggleCreateValueChanged);
    toggleLogin.GetComponent<Toggle>().onValueChanged.AddListener(ToggleLoginValueChanged);

    }

    // Update is called once per frame
    void Update()
    {

    }


    public void SubmitButtonPressed()
    {
        string n = inputFieldUserName.GetComponent<InputField>().text;
        string p = inputFieldPassword.GetComponent<InputField>().text;

        if(toggleLogin.GetComponent<Toggle>().isOn)
            networkedClient.GetComponent<NetworkedClient>().SendMessageToHost(ClientToServerSignifiers.Login + "," + n + "," + p);
        else
            networkedClient.GetComponent<NetworkedClient>().SendMessageToHost(ClientToServerSignifiers.CreateAccount + "," + n + "," + p);
    }


    public void ToggleCreateValueChanged(bool newValue)
    {
        toggleCreate.GetComponent<Toggle>().SetIsOnWithoutNotify(!newValue);
    }
    public void ToggleLoginValueChanged(bool newValue)
    {
        toggleLogin.GetComponent<Toggle>().SetIsOnWithoutNotify(!newValue);
    }


    public void ChangeGameState(int newState)
    {
     inputFieldUserName.SetActive(false);
     inputFieldPassword.SetActive(false);
     buttonSubmit.SetActive(false);
     toggleLogin.SetActive(false);
     toggleCreate.SetActive(false);

     if(newState == GameStates.Login)
     {
      inputFieldUserName.SetActive(true);
      inputFieldPassword.SetActive(true);
      buttonSubmit.SetActive(true);
      toggleLogin.SetActive(true);
      toggleCreate.SetActive(true);
     }

    }
}

public static class ClientToServerSignifiers
{
    public const int Login = 1;
    public const int CreateAccount = 2;
}

public static class ServerToClientSignifiers
{
    public const int LoginResponse = 1;

}


public static class LoginResponses
{
    public const int Success = 1;
    public const int FailureNameInUse = 2;
    public const int FailureNameNotFound = 3;
    public const int FailureIncorrectPassword = 4;
}


public static class GameStates
{
    public const int Login = 1;
    public const int MainMenu = 2;
    public const int WaitingForMatch = 3;
    public const int PlayingTicTacToe = 4;

}