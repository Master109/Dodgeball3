using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class JoinMenu : MonoBehaviour
{
    /*  This file is part of the "Ultimate Unity networking project" by M2H (http://www.M2H.nl)
     *  This project is available on the Unity Store. You are only allowed to use these
     *  resources if you've bought them from the Unity Assets Store.
     */

    public GUISkin skin;
    private Rect windowRect1;
    private Rect windowRect2;

    private string errorMessage = "";
    public  GUIStyle customButton;

    private bool showMenu = false;

    private MainMenu mainMenuScript;
    private List<MyHostDataLobby> hostDataList;

    /////////////////////////////

    void Awake()
    {
        windowRect1 = new Rect(Screen.width / 2 - 305, Screen.height / 2 - 140, 380, 280);
        windowRect2 = new Rect(Screen.width / 2 + 160, Screen.height / 2 - 140, 220, 100);
        hostDataList = new List<MyHostDataLobby>();
    }

    void Start()
    {
        mainMenuScript = MainMenu.SP;

        MultiplayerFunctions.SP.SetHostListDelegate(FullHostListReceived);
        MultiplayerFunctions.SP.FetchHostList();
    }
    public void EnableMenu()
    {
        showMenu = true;
    }
    void OnConnectedToServer()
    {
        Debug.Log("Connected to lobby!");
        showMenu = false;
        GameLobby gameLobbyScript = GetComponent<GameLobby>() as GameLobby;
        gameLobbyScript.EnableLobby();

    }

    void OnGUI()
    {
        if (!showMenu)
        {
            return;
        }
        //Back to main menu
        if (GUI.Button(new Rect(40, 10, 150, 20), "Back to main menu"))
        {
            showMenu = false;
            mainMenuScript.OpenMenu("multiplayer");
        }


        if (errorMessage != null && errorMessage != "")
        {
            GUI.Box(new Rect(Screen.width / 2 - 100, Screen.height / 2 - 30, 200, 60), "Error");
            GUI.Label(new Rect(Screen.width / 2 - 90, Screen.height / 2 - 15, 180, 50), errorMessage);
            if (GUI.Button(new Rect(Screen.width / 2 + 40, Screen.height / 2 + 5, 50, 20), "Close"))
            {
                errorMessage = "";
            }
        }


        if (errorMessage == "")
        { //Hide windows on error
            windowRect1 = GUILayout.Window(0, windowRect1, listGUI, "Join a game via the list");
            windowRect2 = GUILayout.Window(1, windowRect2, directConnectGUIWindow, "Directly join a game via an IP");
            //windowRect3 = GUILayout.Window (2, windowRect3, hostGUI, "Host a game");
        }



    }

    private string remoteIP = "";
    private int remotePort = 20000;
    private string password = "";

    void directConnectGUIWindow(int id)
    {

        GUILayout.BeginVertical();
        GUILayout.Space(5);
        GUILayout.EndVertical();


        GUILayout.BeginHorizontal();
        GUILayout.Space(10);
        remoteIP = GUILayout.TextField(remoteIP, GUILayout.MinWidth(70));
        remotePort = int.Parse(GUILayout.TextField(remotePort + ""));
        GUILayout.Space(10);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Space(10);
        GUILayout.Label("Password");
        password = GUILayout.TextField(password, GUILayout.MinWidth(50));
        GUILayout.Space(10);
        GUILayout.EndHorizontal();




        GUILayout.BeginHorizontal();
        GUILayout.Space(10);
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Connect"))
        {
            MultiplayerFunctions.SP.DirectConnect(remoteIP, remotePort, password, true, OnFinalFailedToConnect);
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

    }

    private Vector2 joinScrollPosition;

    void listGUI(int id)
    {

        GUILayout.BeginVertical();
        GUILayout.Space(6);
        GUILayout.EndVertical();

        //Masterlist
        GUILayout.BeginHorizontal();
        GUILayout.Label("Game list:");

        GUILayout.FlexibleSpace();
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
        GUILayout.EndHorizontal();


        joinScrollPosition = GUILayout.BeginScrollView(joinScrollPosition);
        foreach (var hData2 in hostDataList)
        {
            MyHostDataLobby hData = hData2 as MyHostDataLobby;
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

            GUILayout.EndHorizontal();
        }
        if (hostDataList.Count == 0)
        {
            GUILayout.Label("No servers running right now");
        }
        GUILayout.EndScrollView();

        string text = hostDataList.Count + " total servers";
        GUILayout.Label(text);


    }
    void OnFinalFailedToConnect()
    {
        Debug.Log("OnFinalFailedToConnect=");
    }

    void FullHostListReceived()
    {
        StartCoroutine(ReloadHosts());
    }

    private bool parsingHostList = false;

    IEnumerator ReloadHosts()
    {
        if (parsingHostList) yield break;
        parsingHostList = true;
        HostData[] newData = MultiplayerFunctions.SP.GetHostData();
        int hostLenght = -1;
        while (hostLenght != newData.Length)
        {
            yield return new WaitForSeconds(0.5f);
            newData = MultiplayerFunctions.SP.GetHostData();
            hostLenght = newData.Length;
        }

        hostDataList.Clear();
        foreach (HostData hData in newData)
        {
            MyHostDataLobby cHost = new MyHostDataLobby();
            cHost.realHostData = hData;
            cHost.connectedPlayers = hData.connectedPlayers;
            cHost.IP = hData.ip;
            cHost.port = hData.port;
            cHost.maxPlayers = hData.playerLimit;

            cHost.passwordProtected = hData.passwordProtected;
            cHost.title = hData.gameName;
            cHost.useNAT = hData.useNat;

            hostDataList.Add(cHost);

            if (hostDataList.Count % 3 == 0)
            {
                yield return 0;
            }
        }
        parsingHostList = false;
    }



    public class MyHostDataLobby
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
        public int gameVersion;
    }


}