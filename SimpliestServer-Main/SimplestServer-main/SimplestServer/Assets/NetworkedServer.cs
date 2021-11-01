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

    LinkedList<PlayerAccount> playerAccounts;
    string playerAccountFilePath;

    // Start is called before the first frame update
    void Start()
    {
        // "Kind of a constant"
        playerAccountFilePath = Application.dataPath + Path.DirectorySeparatorChar + "PlayerAccountData.txt";


        NetworkTransport.Init();
        ConnectionConfig config = new ConnectionConfig();
        reliableChannelID = config.AddChannel(QosType.Reliable);
        unreliableChannelID = config.AddChannel(QosType.Unreliable);
        HostTopology topology = new HostTopology(config, maxConnections);
        hostID = NetworkTransport.AddHost(topology, socketPort, null);

        playerAccounts = new LinkedList<PlayerAccount>();
        //We need to load our saved Player Accounts.
        LoadPlayerAccounts();
        
    }

    // Update is called once per frame
    void Update() {

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
                break;
            case NetworkEventType.DataEvent:
                string msg = Encoding.Unicode.GetString(recBuffer, 0, dataSize);
                ProcessRecievedMsg(msg, recConnectionID);
                Debug.Log("Message Received: " + msg);
                break;
            case NetworkEventType.DisconnectEvent:
                Debug.Log("Disconnection, " + recConnectionID);
                break;
        }

    }
  
    public void SendMessageToClient(string msg, int id) {
        byte error = 0;
        byte[] buffer = Encoding.Unicode.GetBytes(msg);
        NetworkTransport.Send(hostID, id, reliableChannelID, buffer, msg.Length * sizeof(char), out error);
    }
    
    private void ProcessRecievedMsg(string msg, int id)
    {
        Debug.Log("msg recieved = " + msg + ".  connection id = " + id);

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
            //bool responseHasBeenSent = false;

            foreach(PlayerAccount pa in playerAccounts)
            {
                if(pa.Name == n)
                {
                    if(pa.Password == p)
                    {
                        SendMessageToClient(ServerToClientSignifiers.LoginResponse + "," + LoginResponses.Success + "," + "You have Successfully Logged In", id);
                        //responseHasBeenSent = true;
                    }
                    else
                    {
                        SendMessageToClient(ServerToClientSignifiers.LoginResponse + "," + LoginResponses.FailureIncorrectPassword + "," + "Incorrect Password", id);
                        //responseHasBeenSent = true;
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
        else
        {

        }


    }


    private void SavePlayerAccounts()
    {
        StreamWriter sw = new StreamWriter(playerAccountFilePath);

        foreach(PlayerAccount pa in playerAccounts)
        {
            sw.WriteLine(pa.Name + "," + pa.Password);
        }
        sw.Close();
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

}


public class GameSession
{

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


public static class ClientToServerSignifiers{
    public const int Login = 1;
    public const int CreateAccount = 2;
}

public static class ServerToClientSignifiers{
    public const int LoginResponse = 1;

}


public static class LoginResponses{
    public const int Success = 1;
    public const int FailureNameInUse = 2;
    public const int FailureNameNotFound = 3;
    public const int FailureIncorrectPassword = 4;
    public const int AccountCreated = 5;
}



public static class GameStates{
    public const int Login = 1;
    public const int MainMenu = 2;
    public const int WaitingForMatch = 3;
    public const int PlayingTicTacToe = 4;

}