using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameSystemManager : MonoBehaviour
{

    // Login Input Fields
    GameObject inputFieldUserName, inputFieldPassword, 
    buttonSubmit, toggleLogin, toggleCreate;

    // Network Client
    GameObject networkedClient;

    // States
    GameObject Loading,LoginSystem, ChatRoom;

    // Login Menu Messages 
    GameObject invalidPass,invalidUser,invalidUserExist,accCreated, invalidIn;


    public string name;

    // Start is called before the first frame update
    void Start()
    {
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();


        // Get Reference to all needed Game Objects
        foreach (GameObject go in allObjects)
        {
            // Network Client
            if (go.name == "NetworkedClient")
                networkedClient = go;

            // States
            else if (go.name == "LoginSystem")
                LoginSystem = go;
            else if (go.name == "Loading")
                Loading = go;
            else if (go.name == "ChatRoomSelection")
                ChatRoom = go;


            // Game Objects - Login System
            else if (go.name == "inputFieldUserName")
                inputFieldUserName = go;
            else if (go.name == "inputFieldPassword")
                inputFieldPassword = go;
            else if (go.name == "buttonSubmit")
                buttonSubmit = go;
            else if (go.name == "toggleCreate")
                toggleCreate = go;
            else if (go.name == "InvalidPassword")
                invalidPass = go;
            else if (go.name == "InvalidUsername")
                invalidUser = go;
            else if (go.name == "InvalidUsernameExists")
                invalidUserExist = go;
            else if (go.name == "AccountCreated")
                accCreated = go;
            else if (go.name == "InvalidInput")
                invalidIn = go;

        }


    // After getting References Disable the ones we don't need right now

    // Ensure that State is being set to Login
    //ChangeGameState(GameStates.Login);

    // Setting Error Messages for LoginMenu off after getting reference
    ValueChanged();

    toggleCreate.GetComponent<Toggle>().onValueChanged.AddListener(toggleCreateValueChanged);



    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
            networkedClient.GetComponent<NetworkedClient>().SendMessageToHost(ClientToServerSignifiers.test + "," + "Hello from client");
        
    }


    public void SubmitButtonPressed()
    {
        string n = inputFieldUserName.GetComponent<InputField>().text;
        string p = inputFieldPassword.GetComponent<InputField>().text;
        ValueChanged();

        if(n != "" && p != "")
        {
            if(toggleCreate.GetComponent<Toggle>().isOn)
                networkedClient.GetComponent<NetworkedClient>().SendMessageToHost(ClientToServerSignifiers.CreateAccount + "," + n + "," + p);
            else
                networkedClient.GetComponent<NetworkedClient>().SendMessageToHost(ClientToServerSignifiers.Login + "," + n + "," + p);
        }
        else
        {
            invalidInput();
        }
    }


    public void toggleCreateValueChanged(bool newValue)
    {
        toggleCreate.GetComponent<Toggle>().SetIsOnWithoutNotify(newValue);

        if (newValue)
            buttonSubmit.GetComponentInChildren<Text>().text = "Create";
        else
            buttonSubmit.GetComponentInChildren<Text>().text = "Login";

    }

    public void ValueChanged()
    {
        invalidPass.SetActive(false);
        invalidUser.SetActive(false);
        invalidUserExist.SetActive(false);
        accCreated.SetActive(false);
        invalidIn.SetActive(false);
    }


    public void ChangeGameState(int newState)
    {
        if(newState == GameStates.Login)
        {
            LoginSystem.SetActive(true);
            ChatRoom.SetActive(false);
            Loading.SetActive(false);
        }
        else if (newState == GameStates.MainMenu)
        {
            LoginSystem.SetActive(false);
            ChatRoom.SetActive(true);
            Loading.SetActive(false);
            
        }
        else if (newState == GameStates.Loading)
        {
            LoginSystem.SetActive(false);
            ChatRoom.SetActive(false);
            Loading.SetActive(true);
        }

        else if (newState == GameStates.PlayingTicTacToe)
        {

        }

    }



    #region ErrorMessages

    public void invalidPassword()
    {
        invalidPass.SetActive(true);
    }

    public void invalidUsername()
    {
        invalidUser.SetActive(true);
    }

    public void usernameExists()
    {
        invalidUserExist.SetActive(true);
    }

    public void accountCreate()
    {
        accCreated.SetActive(true);
    }

    public void invalidInput()
    {
        invalidIn.SetActive(true);
    }

    #endregion ErrorMessages

}



public static class ClientToServerSignifiers
{
    public const int Login = 1;
    public const int CreateAccount = 2;
    public const int AddToGameSeesion = 3;
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
    public const int AccountCreated = 5;
    public const int SendUsername = 6;
}


public static class GameStates
{
    public const int Login = 1;
    public const int MainMenu = 2;
    public const int WaitingForMatch = 3;
    public const int PlayingTicTacToe = 4;
    public const int Loading = 5;

}

public static class ChatStates
{
    public const int ClientToServer = 7;
    public const int ServerToClient = 8;
}