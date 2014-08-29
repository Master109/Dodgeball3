/*  This file is part of the "Ultimate Unity networking project" by M2H (http://www.M2H.nl)
 *  This project is available on the Unity Store. You are only allowed to use these
 *  resources if you've bought them from the Unity Assets Store.
 */
#pragma strict
import System.Collections.Generic;

var skin : GUISkin;


private var windowRect1 : Rect;
private var windowRect2 : Rect;

private var readyToConnect : boolean = false;

private var errorMessage : String = "";


var customButton : GUIStyle;

private var showMenu : boolean = false;

private var mainMenuScript : MainMenu;
private var hostDataList : List.<MyHostDataLobby>;

/////////////////////////////

function Awake(){
	windowRect1 = Rect (Screen.width/2-305,Screen.height/2-140,380,280);
  	windowRect2 = Rect (Screen.width/2+160,Screen.height/2-140,220,100);
  	hostDataList = new List.<MyHostDataLobby>();
}

function Start(){
	mainMenuScript =  MainMenu.SP;
	
	MultiplayerFunctions.SP.SetHostListDelegate(FullHostListReceived);
	MultiplayerFunctions.SP.FetchHostList();
}


function EnableMenu(){
	showMenu=true;
}


function OnConnectedToServer(){
	Debug.Log("Connected to lobby!");
	showMenu=false;
	var gameLobbyScript : GameLobby = GetComponent(GameLobby) as GameLobby;
	gameLobbyScript.EnableLobby();
	
}

function OnGUI ()
{		
	if(!showMenu){
		return;
	}


	//Back to main menu
	if(GUI.Button(Rect(40,10,150,20), "Back to main menu")){
		showMenu=false;
		mainMenuScript.OpenMenu("multiplayer");
	}

	
	if(errorMessage && errorMessage!=""){	
		GUI.Box(Rect(Screen.width/2-100,Screen.height/2-30,200,60), "Error");
		GUI.Label(Rect(Screen.width/2-90,Screen.height/2-15,180,50), errorMessage);
		if(GUI.Button(Rect(Screen.width/2+40,Screen.height/2+5,50,20), "Close")){
			errorMessage="";
		}
	}
	

	if(!errorMessage || errorMessage==""){ //Hide windows on error
		windowRect1 = GUILayout.Window (0, windowRect1, listGUI, "Join a game via the list");	
		windowRect2 = GUILayout.Window (1, windowRect2, directConnectGUIWindow, "Directly join a game via an IP");	
		//windowRect3 = GUILayout.Window (2, windowRect3, hostGUI, "Host a game");
	}	
	
		
	
}

private var remoteIP : String = "";
private var remotePort : int = 20000;
private var password : String = "";

function directConnectGUIWindow(id : int){

	GUILayout.BeginVertical();
	GUILayout.Space(5);
	GUILayout.EndVertical();
		
	
	GUILayout.BeginHorizontal();
	GUILayout.Space(10);
		remoteIP = GUILayout.TextField(remoteIP, GUILayout.MinWidth(70));
		remotePort = parseInt(GUILayout.TextField(remotePort+""));
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
	if (GUILayout.Button ("Connect"))
	{	
		MultiplayerFunctions.SP.DirectConnect(remoteIP, remotePort, password, true,  OnFinalFailedToConnect);
	}		
	GUILayout.FlexibleSpace();
	GUILayout.EndHorizontal();
	
}

private var joinScrollPosition : Vector2;

function listGUI (id : int) {
	
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
        for(var  hData2   in hostDataList )
        {
           var hData : MyHostDataLobby= hData2 as MyHostDataLobby ;
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
        if(hostDataList.Count==0){
            GUILayout.Label("No servers running right now");
        }
        GUILayout.EndScrollView();

         var text : String= hostDataList.Count + " total servers";
        GUILayout.Label(text);
           		
	
}


 function OnFinalFailedToConnect () : void {
         Debug.Log("OnFinalFailedToConnect=" );
    }



    function FullHostListReceived () : void {
        StartCoroutine(ReloadHosts());
    }

     private var hasParsedHostListOnce : boolean= false;
     private var parsingHostList : boolean= false;

    function ReloadHosts () : IEnumerator {
        if (parsingHostList) return;
        parsingHostList = true;
         var newData : HostData[]= MultiplayerFunctions.SP.GetHostData();
         var hostLenght : int= -1;
        while (hostLenght != newData.Length)
        {
            yield new WaitForSeconds(0.5f);
            newData = MultiplayerFunctions.SP.GetHostData();
            hostLenght = newData.Length;
        }

        hostDataList.Clear();
        for(var hData  : HostData    in newData )
        {
             var cHost : MyHostDataLobby = new MyHostDataLobby();
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
                yield 0;
            }
        }
        parsingHostList = false;
        hasParsedHostListOnce = true;
    }



class MyHostDataLobby
{
     var realHostData : HostData;
     var title : String;
     var useNAT : boolean;
     var connectedPlayers : int;
     var maxPlayers : int;
     var IP : String[];
     var port : int;
     var passwordProtected : boolean;
     
     //Example custom fields
     var gameVersion : int;
}

