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
    GameObject Loading,LoginSystem, ChatRoom, WaitingMatch, InGame;

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
            else if (go.name == "WaitingForMatch")
                WaitingMatch = go;
            else if (go.name == "Gameplay")
                InGame = go;


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


    // Setting Error Messages for LoginMenu off after getting reference
    ValueChanged();

    toggleCreate.GetComponent<Toggle>().onValueChanged.AddListener(toggleCreateValueChanged);

    }

    // Update is called once per frame
    void Update()
    {

        
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

    public void ChangeGameState(int newState)
    {
        if(newState == GameStates.Login)
        {
            LoginSystem.SetActive(true);
            ChatRoom.SetActive(false);
            Loading.SetActive(false);
            WaitingMatch.SetActive(false);
            InGame.SetActive(false);
        }
        else if (newState == GameStates.MainMenu)
        {
            LoginSystem.SetActive(false);
            ChatRoom.SetActive(true);
            Loading.SetActive(false);
            WaitingMatch.SetActive(false);
            InGame.SetActive(false);
            
        }
        else if (newState == GameStates.Loading)
        {
            LoginSystem.SetActive(false);
            ChatRoom.SetActive(false);
            Loading.SetActive(true);
            WaitingMatch.SetActive(false);
            InGame.SetActive(false);
        }

        else if (newState == GameStates.WaitingForMatch)
        {
            LoginSystem.SetActive(false);
            ChatRoom.SetActive(false);
            Loading.SetActive(false);
            WaitingMatch.SetActive(true);
            InGame.SetActive(false);
        }

        else if (newState == GameStates.ToGame)
        {
            LoginSystem.SetActive(false);
            ChatRoom.SetActive(false);
            Loading.SetActive(false);
            WaitingMatch.SetActive(false);
            InGame.SetActive(true);
        }

    }


    public void FindGameButtonPressed()
    {
        networkedClient.GetComponent<NetworkedClient>().SendMessageToHost(ClientToServerSignifiers.Match + "," + GameSignifiers.FindMatch);
        ChangeGameState(GameStates.WaitingForMatch);

    }


    #region ErrorMessages

    public void ValueChanged()
    {
        invalidPass.SetActive(false);
        invalidUser.SetActive(false);
        invalidUserExist.SetActive(false);
        accCreated.SetActive(false);
        invalidIn.SetActive(false);
    }


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


#region Signifiers

// Front Signifiers
public static class ClientToServerSignifiers
{
    public const int Login = 1;
    public const int CreateAccount = 2;
    public const int Match = 3;

}

public static class ServerToClientSignifiers
{
    public const int LoginResponse = 1;
    public const int MatchResponse = 4;

}


public static class GameSignifiers
{
    public const int FindMatch = 1;
    public const int SendMoveToServer = 2;
    public const int AddToGameSession = 3;
    public const int SendMoveToClients = 4;
    public const int EndGame = 5;
    public const int ResetGame = 6;
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
    public const int ToGame = 6;

}

public static class ChatStates
{
    public const int ClientToServer = 7;
    public const int ServerToClient = 8;
    public const int ConnectedUserList = 9;
}

#endregion Signifiers
