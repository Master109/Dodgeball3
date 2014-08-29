/*  This file is part of the "Ultimate Unity networking project" by M2H (http://www.M2H.nl)
 *  This project is available on the Unity Store. You are only allowed to use these
 *  resources if you've bought them from the Unity Assets Store.
 */
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MultiplayerMenu2 : MonoBehaviour {

    //GUI vars
 
    void Awake (){
        GameSettings.Clear(); 
    }

    void  Start (){

        //Default join values
        joinPort = MultiplayerFunctions.SP.defaultServerPort;
        joinIP = joinPW = "";

        //Default host values
        hostTitle = PlayerPrefs.GetString("hostTitle", "Guests server");
        hostDescription = PlayerPrefs.GetString("hostDescription", "Servers description");
        hostMOTD = PlayerPrefs.GetString("hostMOTD", "Servers message of the day");
        hostPW = PlayerPrefs.GetString("hostPassword", "");
        hostMaxPlayers = PlayerPrefs.GetInt("hostPlayers" , 8);
        hostPort = PlayerPrefs.GetInt("hostPort", MultiplayerFunctions.SP.defaultServerPort);

        hostDataList = new List<MyHostData>();
        MultiplayerFunctions.SP.SetHostListDelegate(FullHostListReceived);

    }



    void EnableMenu (){
        ReloadSettings();
    }
    void DisableMenu (){
        AbortRandomConnect();
        if (MultiplayerFunctions.SP.IsConnecting()) MultiplayerFunctions.SP.CancelConnection();
    }


    void ReloadSettings (){
        MultiplayerFunctions.SP.FetchHostList();
    }

 
    void OnConnectedToServer (){
        GameSettings.Clear();
        //Stop communication until in the game
        Network.isMessageQueueRunning = false;
        Application.LoadLevel(Application.loadedLevel+1);  
    }

    void OnServerInitialized (){
        //Load game
        Application.LoadLevel(Application.loadedLevel+1); 
    }

    void OnGUI (){
        GUILayout.Window(2, new Rect(Screen.width/ 2 - 600/2, Screen.height/2-550/2, 600, 550), WindowGUI, "");
    }

  


    private string currentGUIMethod= "join";



    void WindowGUI (int wID){
        GUILayout.BeginHorizontal();
        GUILayout.Label("Multiplayer menu");
        GUILayout.FlexibleSpace();        
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Select an option:");
        GUILayout.Space(5);
        if (currentGUIMethod == "join")
        {
            GUILayout.Label("Join", GUILayout.Width(75));
        }else{
            if (GUILayout.Button("Join", GUILayout.Width(75)))
            {
                currentGUIMethod = "join";
            }
        }
        if (currentGUIMethod == "host"){
            GUILayout.Label("Host", GUILayout.Width(75));
        }else{
            if (GUILayout.Button("Host", GUILayout.Width(75)))
            {
                currentGUIMethod = "host";
            }
        }
        
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.Space(25);
            
        
        if (currentGUIMethod == "join") 
            JoinMenu();      
        else        
            HostMenu();
                 
    }

    private Vector2 JoinScrollPosition;
    private List<MyHostData> hostDataList = new List<MyHostData>();

    private  int joinPort;
    private  string joinIP= "";
    private  string joinPW= "";
    private  bool  joinUsePW= false;

    private  string failConnectMesage= "";

    void JoinMenu (){
        if (MultiplayerFunctions.SP.IsConnecting())
        {
             float timeSince= Mathf.Round(MultiplayerFunctions.SP.TimeSinceLastConnect() * 10) / 10;
             string status= "Trying to connect to [" + MultiplayerFunctions.SP.ConnectingToAddress() + "]";
            if (joinPW != "")
            {
                status += " using password.";
            }
            GUILayout.Label(status);
            GUILayout.Label("Waiting: " + timeSince);
            if (timeSince >= 2 && GUILayout.Button("Cancel"))
            {
                MultiplayerFunctions.SP.CancelConnection();
            }
        }
        else if (failConnectMesage != "")
        {
            GUILayout.Label("The game failed to connect:\n" + failConnectMesage);
            if (lastConnectError == NetworkConnectionError.InvalidPassword)
            {
                GUILayout.Label("You entered a wrong password, try again here:");
                joinIP = MultiplayerFunctions.SP.LastIP()[0];
                joinPort = MultiplayerFunctions.SP.LastPort();
                GUILayout.BeginHorizontal();
                GUILayout.Space(5);
                GUILayout.Label("IP");
                GUILayout.Label(joinIP, GUILayout.Width(100));
                GUILayout.Label("Port");
                GUILayout.Label(joinPort + "", GUILayout.Width(50));
                GUILayout.Label("Password");
                joinPW = GUILayout.TextField(joinPW, GUILayout.Width(100));
                if (GUILayout.Button("Connect"))
                {
                    MultiplayerFunctions.SP.DirectConnect(joinIP, joinPort, joinPW, true, OnFinalFailedToConnect);
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

            }
            GUILayout.Space(10);
            if (GUILayout.Button("Cancel"))
            {
                failConnectMesage = "";
            }
        }
        else
        {
            if (joiningRandomGame)
            {
                GUILayout.Label("Trying to connect to first possible game...");
                if (GUILayout.Button("Cancel"))
                {
                    joiningRandomGame = false;
                    MultiplayerFunctions.SP.CancelConnection();
                }
            }
            else
            {     
                //Masterlist
                GUILayout.BeginHorizontal();
                GUILayout.Label("Game list:");

                GUILayout.FlexibleSpace();
                if (hostDataList != null && hostDataList.Count > 0 && GUILayout.Button("Join random game"))
                {
                    StartCoroutine(StartJoinRandom());
                }
                if (GUILayout.Button("Refresh list"))
                {
                    MultiplayerFunctions.SP.FetchHostList();
                }
                GUILayout.EndHorizontal();

                GUILayout.Space(2);
                GUILayout.BeginHorizontal();
                GUILayout.Space(24);

                GUILayout.Label("Title", GUILayout.Width(200));
                GUILayout.Label("Players", GUILayout.Width(55));
                GUILayout.Label("IP", GUILayout.Width(150));
                GUILayout.Label("Dedicated", GUILayout.Width(70));
                GUILayout.EndHorizontal();


                JoinScrollPosition = GUILayout.BeginScrollView(JoinScrollPosition);
                foreach(MyHostData hData  in hostDataList )
                {
                   GUILayout.BeginHorizontal();

                    if (hData.passwordProtected)
                        GUILayout.Label("PW", GUILayout.MaxWidth(16));
                    else
                        GUILayout.Space(24);

                    if (GUILayout.Button("" + hData.title, GUILayout.Width(200)))
                    {
                        MultiplayerFunctions.SP.HostDataConnect(hData.realHostData, "", true, OnFinalFailedToConnect);
                    }
                    GUILayout.Label(hData.connectedPlayers + "/" + hData.maxPlayers, GUILayout.Width(55));
                    GUILayout.Label(hData.IP[0] + ":" + hData.port, GUILayout.Width(150));

                    //Options
                    GUILayout.Space(35 - 8);
                    if (hData.isDedicated)
                        GUILayout.Label("D", GUILayout.Width(70));
                    else
                        GUILayout.Space(70);



                    GUILayout.EndHorizontal();
                }
                if(hostDataList.Count==0){
                    GUILayout.Label("No servers running right now");
                }
                GUILayout.EndScrollView();

                 string text= hostDataList.Count + " total servers";
                GUILayout.Label(text);

                //DIRECT JOIN

                GUILayout.BeginHorizontal();
                GUILayout.Label("Direct join:");
                GUILayout.Space(5);
                GUILayout.Label("IP");
                joinIP = GUILayout.TextField(joinIP, GUILayout.Width(100));
                GUILayout.Label("Port");
                joinPort = int.Parse(GUILayout.TextField(joinPort + "", GUILayout.Width(50)) + "");
                GUILayout.Label("Password");
                joinUsePW = GUILayout.Toggle(joinUsePW, "", GUILayout.MaxWidth(22));
                if (joinUsePW)
                {
                    joinPW = GUILayout.TextField(joinPW, GUILayout.Width(100));
                }
                if (GUILayout.Button("Connect"))
                {
                    MultiplayerFunctions.SP.DirectConnect(joinIP, joinPort, joinPW, true, OnFinalFailedToConnect);
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.Space(4);
            }
        }
    }

 
    private  bool  joiningRandomGame= false;
    private  int randConnectNr= 0;


    IEnumerator StartJoinRandom ( ){
        if (joiningRandomGame) yield break; ;
        joiningRandomGame = true;

        while (joiningRandomGame && (!hasParsedHostListOnce || !MultiplayerFunctions.SP.ReadyLoading() || !MultiplayerFunctions.SP.HasReceivedHostList()))
        {
            yield return 0;
        }
        if (joiningRandomGame)
        {
            randConnectNr = 1;
            foreach( MyHostData hData  in  hostDataList)
            {
                MultiplayerFunctions.SP.HostDataConnect(hData.realHostData, "", true, OnFinalFailedToConnect);
                yield return new WaitForSeconds(2);
                if (Network.isClient || !joiningRandomGame) break;
                randConnectNr++;
            }
        }
        joiningRandomGame = false;
    }

    void AbortRandomConnect (){
        joiningRandomGame = false;
    }
    bool IsDoingRandomConnect (){
          return joiningRandomGame;
    }
    string RandConnectNr (){
        return randConnectNr + "/" + hostDataList.Count;
    }


     private NetworkConnectionError lastConnectError;

    void OnFinalFailedToConnect (){
        lastConnectError = MultiplayerFunctions.SP.LastConnectionError();
        failConnectMesage = failConnectMesage + "Attempting to connect to [" + MultiplayerFunctions.SP.LastIP()[0] + ":" + MultiplayerFunctions.SP.LastPort() + "]: " + lastConnectError + "\n";
        Debug.Log("OnFinalFailedToConnect=" + failConnectMesage);
    }



    private string hostTitle;
    private string hostMOTD;
    private string hostDescription;
    private string hostPW;
    private int hostMaxPlayers;
    private int hostPort;
    private bool  hostUsePassword= false;


    void HostMenu (){


        GUILayout.BeginHorizontal();
        GUILayout.Label("Host a new game:");
        GUILayout.EndHorizontal();

        //GUILayout.Toggle(true, "Construction gamemode");

        GUILayout.BeginHorizontal();
        GUILayout.Label("Title:");
        GUILayout.FlexibleSpace();
        hostTitle = GUILayout.TextField(hostTitle, GUILayout.Width(200));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Server description");
        GUILayout.FlexibleSpace();
        hostDescription = GUILayout.TextField(hostDescription, GUILayout.Width(200));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("MOTD");
        GUILayout.FlexibleSpace();
        hostMOTD = GUILayout.TextField(hostMOTD, GUILayout.Width(200));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Server password ", GUILayout.Width(200), GUILayout.Height(23));
        GUILayout.FlexibleSpace();
        hostUsePassword = GUILayout.Toggle(hostUsePassword, "", GUILayout.MaxWidth(40));
        if (hostUsePassword)
        {
            hostPW = GUILayout.TextField(hostPW, GUILayout.Width(200));

        }
        else
        {
            GUILayout.Label("", GUILayout.Height(23), GUILayout.Width(200));
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Max players (1-32)");
        GUILayout.FlexibleSpace();
        hostMaxPlayers = int.Parse(GUILayout.TextField(hostMaxPlayers + "", GUILayout.Width(50)) + "");
        GUILayout.EndHorizontal();

        CheckHostVars();

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Start server", GUILayout.Width(150)))
        {
            StartHostingGame(hostTitle,hostMaxPlayers, hostPort,hostMOTD, hostDescription, hostPW);
        }
        GUILayout.EndHorizontal();
    }

    void CheckHostVars (){
        hostMaxPlayers = Mathf.Clamp(hostMaxPlayers, 1, 64);
        hostPort = Mathf.Clamp(hostPort, 10000, 100000);
        //hostTitle = (hostTitle);
        //hostMOTD = (hostMOTD);
        //hostDescription = (hostDescription);
        //hostPW = (hostPW);
    }


    void StartHostingGame (  string hostSettingTitle  ,     int hostPlayers  ,     int hostPort  ,    string motd  ,     string description  ,     string password    ){
        if (Network.isServer)
        {
            Network.Disconnect();

        }
        if (hostSettingTitle == "")
        {
            hostSettingTitle = "NoTitle";
        }
        
        hostPlayers = Mathf.Clamp(hostPlayers, 0, 64);
        hostPort = Mathf.Clamp(hostPort, 10000, 100000);
        //hostSettingTitle = (hostSettingTitle);
        //description = (description);
        //password = (password);
        
        GameSettings.Clear();
        GameSettings.motd = motd;
        GameSettings.description = description;
        GameSettings.serverTitle = hostSettingTitle;
        GameSettings.port = hostPort;
        GameSettings.IP = "localhost";
        GameSettings.players = hostPlayers;
        GameSettings.password = password;

        //maxplayers =2 should open only 1 more connection.
        //if (!isDedicated)
        //{
            hostPlayers -= 1;
        //}

        MultiplayerFunctions.SP.StartServer(password, hostPort, hostPlayers, true);
        
    }


    void FullHostListReceived (){
        StartCoroutine(ReloadHosts());
    }

     private bool  hasParsedHostListOnce= false;
     private bool  parsingHostList= false;

    IEnumerator ReloadHosts (){
        if (parsingHostList) yield break;
        parsingHostList = true;
         HostData[] newData= MultiplayerFunctions.SP.GetHostData();
         int hostLenght= -1;
        while (hostLenght != newData.Length)
        {
            yield return new WaitForSeconds(0.5f);
            newData = MultiplayerFunctions.SP.GetHostData();
            hostLenght = newData.Length;
        }

        hostDataList.Clear();
        foreach(HostData hData    in newData )
        {
             MyHostData cHost= new MyHostData();
            cHost.realHostData = hData;
            cHost.connectedPlayers = hData.connectedPlayers;
            cHost.IP = hData.ip;
            cHost.port = hData.port;
            cHost.maxPlayers = hData.playerLimit;

            cHost.passwordProtected = hData.passwordProtected;
            cHost.title = hData.gameName;
            cHost.useNAT = hData.useNat;
            
            /*//EXAMPLE CUSTOM FIELDS
            string[] comments= hData.comment.Split("#"[0]);
            cHost.gameVersion = int.Parse(comments[2]);

            //cHost.isDedicated = comments[1] == "1";         
            if (cHost.isDedicated)
            {
                cHost.connectedPlayers -= 1;
                cHost.maxPlayers -= 1;
            }*/

            hostDataList.Add(cHost);
            
            if (hostDataList.Count % 3 == 0)
            {
                yield return 0;
            }
        }
        parsingHostList = false;
        hasParsedHostListOnce = true;
    }



public class MyHostData
{
     public HostData realHostData;
     public string title;
     public bool useNAT;
     public int connectedPlayers;
     public int maxPlayers;
     public string[] IP;
     public int port;
     public bool passwordProtected;
     
     //Example custom fields
     public bool isDedicated = false;
     public int gameVersion;
}


}