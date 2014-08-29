/*  This file is part of the "Ultimate Unity networking project" by M2H (http://www.M2H.nl)
 *  This project is available on the Unity Store. You are only allowed to use these
 *  resources if you've bought them from the Unity Assets Store.
 */
import System.Collections.Generic;

var playerPref : Transform;
var chatScript : FPSChat4;
var playerName : String = "";

//Server-only playerlist
public var playerList = new List.<FPSPlayerNode>();
class FPSPlayerNode {
	var playerName : String;
	var networkPlayer : NetworkPlayer;
	
	var kills : int =0;
	var deaths : int =0;	
}


function Awake() 
{
	playerName = PlayerPrefs.GetString("playerName");
	
	chatScript = GetComponent(FPSChat4) as FPSChat4;
	Network.isMessageQueueRunning = true;
	Screen.lockCursor=true;	
	
	if(Network.isServer){
		
		MultiplayerFunctions.SP.RegisterHost(playerName+"s game", "No description");
	
		chatScript.ShowChatWindow();
		
		networkView.RPC ("TellOurName", RPCMode.AllBuffered, playerName, Network.player);
		
		for (var go : GameObject in FindObjectsOfType(GameObject) as GameObject[]){
			go.SendMessage("OnNetworkLoadedLevel", SendMessageOptions.DontRequireReceiver);	
		}		
		
	}else if(Network.isClient){
		
		networkView.RPC ("TellOurName", RPCMode.AllBuffered, playerName, Network.player);
		chatScript.ShowChatWindow();
		
		for (var go : GameObject in FindObjectsOfType(GameObject) as GameObject[]){
			go.SendMessage("OnNetworkLoadedLevel", SendMessageOptions.DontRequireReceiver);	
		}	
		
		
		
	}else{
		//How did we even get here without connection?
		Screen.lockCursor=false;	
		Application.LoadLevel((Application.loadedLevel-1));		
	}
}


//Server function
function OnPlayerDisconnected(player: NetworkPlayer) {
	Network.RemoveRPCs(player, 0);
	Network.DestroyPlayerObjects(player);
	
	//Remove player from the server list
	for(var entry : FPSPlayerNode in  playerList as List.<FPSPlayerNode>){
		if(entry.networkPlayer==player){
			chatScript.addGameChatMessage(entry.playerName+" disconnected from: " + player.ipAddress+":" + player.port);
			playerList.Remove(entry);
			break;
		}
	}
}

//Server function
function OnPlayerConnected(player: NetworkPlayer) {
	chatScript.addGameChatMessage("Player connected from: " + player.ipAddress +":" + player.port);
}

@RPC
//Sent by newly connected clients, recieved by server
function TellOurName(name : String, fromPlayer: NetworkPlayer, info : NetworkMessageInfo){
	var netPlayer : NetworkPlayer = fromPlayer;

	if(netPlayer+""=="-1"){
		//This hack is required to fix the local players networkplayer when the RPC is sent to itself.
		netPlayer=Network.player;
	}
	var newEntry : FPSPlayerNode = new FPSPlayerNode();
	newEntry.playerName=name;
	newEntry.networkPlayer=netPlayer;
	playerList.Add(newEntry);
	
	if(Network.isServer){
		chatScript.addGameChatMessage(name+" joined the game");
	}
}

//Called via Awake()
function OnNetworkLoadedLevel()
{
	// Randomize starting location
	var spawnpoints : GameObject[] = GameObject.FindGameObjectsWithTag ("Spawnpoint");
	Debug.Log("spawns: "+spawnpoints.length);
	
	var spawnpoint : Transform = spawnpoints[Random.Range(0, spawnpoints.length)].transform;
	var newTrans : Transform = Network.Instantiate(playerPref,spawnpoint.position, spawnpoint.rotation, 0) as Transform;
}


function OnDisconnectedFromServer () {
	//Load main menu
	Screen.lockCursor=false;
	Application.LoadLevel((Application.loadedLevel-1));
}




