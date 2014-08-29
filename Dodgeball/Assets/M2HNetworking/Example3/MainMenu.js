/*  This file is part of the "Ultimate Unity networking project" by M2H (http://www.M2H.nl)
 *  This project is available on the Unity Store. You are only allowed to use these
 *  resources if you've bought them from the Unity Assets Store.
 */
#pragma strict

static var SP : MainMenu;

private var joinMenuScript : JoinMenu;
private var gameLobbyScript : GameLobby;
private var multiplayerScript : MultiplayerMenu;

private var requirePlayerName : boolean = false;
private var playerNameInput : String = "";

function Awake(){
	SP=this;
	
	playerNameInput = PlayerPrefs.GetString("playerName", "");
	requirePlayerName=true;

	
	joinMenuScript = GetComponent(JoinMenu) as JoinMenu;
	gameLobbyScript = GetComponent(GameLobby) as GameLobby;
	multiplayerScript = GetComponent(MultiplayerMenu) as MultiplayerMenu;	

	OpenMenu("multiplayer");
}


function OnGUI(){
	if(requirePlayerName){
		GUILayout.Window (9, Rect(Screen.width/2-150,Screen.height/2-100,300,100), NameMenu, "Please enter a name:");	
	}
}


function OpenMenu(newMenu : String){
	if(requirePlayerName){
		return;
	}
	
	if(newMenu=="multiplayer-host"){ 
		gameLobbyScript.EnableLobby();		
		
	}else if(newMenu=="multiplayer-join"){ 
		joinMenuScript.EnableMenu();
		
	}else if(newMenu=="multiplayer"){ 
		multiplayerScript.EnableMenu();
		
	}else{			
		Debug.LogError("Wrong menu:"+newMenu);	
		
	}
}


function NameMenu(id : int){
	
	GUILayout.BeginVertical();
	GUILayout.Space(10);
			
	
	GUILayout.BeginHorizontal();
	GUILayout.Space(10);
		GUILayout.Label("Please enter your name");
	GUILayout.Space(10);
	GUILayout.EndHorizontal();
	
	
	GUILayout.BeginHorizontal();
	GUILayout.Space(10);
	playerNameInput = GUILayout.TextField(playerNameInput);
	GUILayout.Space(10);
	GUILayout.EndHorizontal();	
	
	
	
	
	 GUILayout.BeginHorizontal();
	GUILayout.Space(10);
		if(playerNameInput.length>=1){
			if(GUILayout.Button("Save")){
				requirePlayerName=false;
				PlayerPrefs.SetString("playerName", playerNameInput);
				OpenMenu("multiplayer");
			}
		}else{
			GUILayout.Label("Enter a name to continue...");
		}
	GUILayout.Space(10);
	GUILayout.EndHorizontal();
	
	
	GUILayout.EndVertical();
	
}

