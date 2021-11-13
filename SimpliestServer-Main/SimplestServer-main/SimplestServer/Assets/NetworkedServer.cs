using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using UnityEngine.UI;

public class NetworkedServer : MonoBehaviour
{
    int maxConnections = 1000;
    int reliableChannelID;
    int unreliableChannelID;
    int hostID;
    int socketPort = 3389;

    List<int> idlist;

    LinkedList<PlayerAccount> playerAccounts;
    string playerAccountFilePath;
    LinkedList<GameSession> gameSessions;
    
    int playerWaitingForMatch = -1;

    // Start is called before the first frame update
    void Start()
    {

        Debug.Log("Started Server");
        // "Kind of a constant"
        playerAccountFilePath = Application.dataPath + Path.DirectorySeparatorChar + "PlayerAccountData.txt";


        NetworkTransport.Init();
        ConnectionConfig config = new ConnectionConfig();
        reliableChannelID = config.AddChannel(QosType.Reliable);
        unreliableChannelID = config.AddChannel(QosType.Unreliable);
        HostTopology topology = new HostTopology(config, maxConnections);
        hostID = NetworkTransport.AddHost(topology, socketPort, null);

        // List of Player accounts and current connected ID's
        playerAccounts = new LinkedList<PlayerAccount>();
        idlist = new List<int>();
        gameSessions = new LinkedList<GameSession>();


        //We need to load our saved Player Accounts.
        LoadPlayerAccounts();
        
    }

    // Update is called once per frame
    void Update() 
    {

        int recHostID;
        int recConnectionID;
        int recChannelID;
        byte[] recBuffer = new byte[1024];
        int bufferSize = 1024;
        int dataSize;
        byte error = 0;

        NetworkEventType recNetworkEvent = NetworkTransport.Receive(out recHostID, out recConnectionID, out recChannelID, recBuffer, bufferSize, out dataSize, out error);

        switch (recNetworkEvent)
        {
            case NetworkEventType.Nothing:
                break;
            case NetworkEventType.ConnectEvent:
                Debug.Log("Connection, " + recConnectionID);
                idlist.Add(recConnectionID);

                /*
                foreach(IDName identity in idNameLink)
                {
                    if (identity.id != recConnectionID)
                        SendMessageToClient(ChatStates.ConnectedUserList + "," + identity.name + "," + identity.id, identity.id);
                }
                */

                break;
            case NetworkEventType.DataEvent:
                string msg = Encoding.Unicode.GetString(recBuffer, 0, dataSize);
                ProcessRecievedMsg(msg, recConnectionID);
                Debug.Log("Message Received: " + msg);
                break;
            case NetworkEventType.DisconnectEvent:
                Debug.Log("Disconnection, " + recConnectionID);
                idlist.Remove(recConnectionID);
                break;
        }

    }
  
    public void SendMessageToClient(string msg, int id) 
    {
        byte error = 0;
        byte[] buffer = Encoding.Unicode.GetBytes(msg);
        NetworkTransport.Send(hostID, id, reliableChannelID, buffer, msg.Length * sizeof(char), out error);
    }
    
    private void ProcessRecievedMsg(string msg, int id)
    {
        //Debug.Log("msg recieved = " + msg + ".  connection id = " + id);

        string[] csv = msg.Split(',');

        int signifier = int.Parse(csv[0]);



        if(signifier == ClientToServerSignifiers.CreateAccount)
        {
            string n = csv[1];
            string p = csv[2];

            bool isUnique = true;

            foreach(PlayerAccount pa in playerAccounts) 
            {
                if(pa.Name == n)
                {
                    isUnique = false;
                    break;
                }
            }

            if(isUnique) 
            {
                playerAccounts.AddLast(new PlayerAccount(n,p));
                SendMessageToClient(ServerToClientSignifiers.LoginResponse + "," + LoginResponses.AccountCreated + "," + "Account Created", id);
                SavePlayerAccounts();
                foreach(PlayerAccount pa in playerAccounts)
                    Debug.Log(pa);
            }
            else
            {
                SendMessageToClient(ServerToClientSignifiers.LoginResponse + "," + LoginResponses.FailureNameInUse + "," + "This Account already Exists", id);
            }
        }

        else if (signifier == ClientToServerSignifiers.Login)
        {
            string n = csv[1];
            string p = csv[2];

            bool hasBeenFound = false;
            

            foreach(PlayerAccount pa in playerAccounts)
            {
                if(pa.Name == n)
                {
                    if(pa.Password == p)
                    {
                        SendMessageToClient(ServerToClientSignifiers.LoginResponse + "," + LoginResponses.Success + "," + "You have Successfully Logged In", id);
                        SendMessageToClient(ServerToClientSignifiers.LoginResponse + "," + LoginResponses.SendUsername + "," + n, id);
                        /*
                        foreach (int identifier in idlist)
                        {
                            SendMessageToClient(ChatStates.ConnectedUserList + "," + n + "," + id, id);
                        }

                        idNameLink.Add(new IDName(id, n));
                        */
                        

                    }
                    else
                    {
                        SendMessageToClient(ServerToClientSignifiers.LoginResponse + "," + LoginResponses.FailureIncorrectPassword + "," + "Incorrect Password", id);
                        
                    }
                    hasBeenFound = true;
                    break;
                }

            }
            if(!hasBeenFound)
            {
                SendMessageToClient(ServerToClientSignifiers.LoginResponse + "," + LoginResponses.FailureNameNotFound + "," + "Failure name not found", id);
            }

        }

        else if (signifier == ChatStates.ClientToServer)
        {
            string name = csv[1];
            string message = csv[2];
            foreach (int identifier in idlist)
            {
                SendMessageToClient(ChatStates.ServerToClient + "," + name + "," + message, identifier);
            }
        }




        // Match Signifier Handling

        else if (signifier == ClientToServerSignifiers.Match)
        {
            int MatchSignifier = int.Parse(csv[1]);

            if (MatchSignifier == GameSignifiers.FindMatch)
            {
                if(playerWaitingForMatch == -1)
                {
                    playerWaitingForMatch = id;
                }
                else 
                {
                    GameSession gs = new GameSession(playerWaitingForMatch, id);
                    gameSessions.AddLast(gs);
                    SendMessageToClient(ServerToClientSignifiers.MatchResponse + "," + GameSignifiers.AddToGameSession, gs.playerID1);
                    SendMessageToClient(ServerToClientSignifiers.MatchResponse + "," + GameSignifiers.AddToGameSession, gs.playerID2);
                    playerWaitingForMatch = -1;
                    Debug.Log("User found match!");
                }
            }

            else if (MatchSignifier == GameSignifiers.SendMoveToServer)
            {
                int move = int.Parse(csv[2]);
                Debug.Log("Square User placed on is: " + move);

                GameSession gs = FindGameSessionWithPlayerID(id);

                if(gs != null) 
                {
                    if(gs.playerID1 == id)
                        SendMessageToClient(ServerToClientSignifiers.MatchResponse + "," + GameSignifiers.SendMoveToClients + "," + move, gs.playerID2);
                    else
                        SendMessageToClient(ServerToClientSignifiers.MatchResponse + "," + GameSignifiers.SendMoveToClients + "," + move, gs.playerID1);
                }
            }

            else if (MatchSignifier == GameSignifiers.EndGame)
            {
                GameSession gs = FindGameSessionWithPlayerID(id);
                if(gs != null) 
                {
                    SendMessageToClient(ServerToClientSignifiers.MatchResponse + "," + GameSignifiers.EndGame, gs.playerID1);
                    SendMessageToClient(ServerToClientSignifiers.MatchResponse + "," + GameSignifiers.EndGame, gs.playerID2);
                }
            }
        }

    }


    private void SavePlayerAccounts()
    {
        //StreamWriter sw = new StreamWriter(playerAccountFilePath);
        using (StreamWriter sw = File.AppendText(playerAccountFilePath))
        {
            foreach(PlayerAccount pa in playerAccounts)
            {
                sw.WriteLine(pa.Name + "," + pa.Password);
            }
            sw.Close();
        }
        
    }


    private void LoadPlayerAccounts()
    {
        if(File.Exists(playerAccountFilePath))
        {
            StreamReader sr = new StreamReader(playerAccountFilePath);

            string line;

            while((line = sr.ReadLine()) != null)
            {
              string[] csv = line.Split(',');
             PlayerAccount pa = new PlayerAccount(csv[0],csv[1]);
             playerAccounts.AddLast(pa);
            }
        }
    }


    private GameSession FindGameSessionWithPlayerID(int id)
    {
        foreach(GameSession gs in gameSessions)
        {
            if(gs.playerID1 == id || gs.playerID2 == id)
                return gs;
        }
        return null;
    }

}

public class GameSession
{
    public int playerID1, playerID2;

    public GameSession(int id1, int id2)
    {
        playerID1 = id1;
        playerID2 = id2;
    }


}


public class IDName
{
    public int id;
    public string name;

    public IDName(int identifitier, string Name)
    {
        id = identifitier;
        name = Name;
    }

}



public class PlayerAccount
{
    public string Name, Password;

    public PlayerAccount(string name, string password)
    {
        Name = name;
        Password = password;
    }
}


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
}




public static class ChatStates
{
    public const int ClientToServer = 7;
    public const int ServerToClient = 8;
    public const int ConnectedUserList = 9;
}


// Back Signifiers
public static class LoginResponses{
    public const int Success = 1;
    public const int FailureNameInUse = 2;
    public const int FailureNameNotFound = 3;
    public const int FailureIncorrectPassword = 4;
    public const int AccountCreated = 5;
    public const int SendUsername = 6;
}





public static class GameStates{
    public const int Login = 1;
    public const int MainMenu = 2;
    public const int WaitingForMatch = 3;
    public const int PlayingTicTacToe = 4;

}