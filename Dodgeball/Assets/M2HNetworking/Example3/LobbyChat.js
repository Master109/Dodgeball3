/*  This file is part of the "Ultimate Unity networking project" by M2H (http://www.M2H.nl)
 *  This project is available on the Unity Store. You are only allowed to use these
 *  resources if you've bought them from the Unity Assets Store.
 */
import System.Collections.Generic;

public var usingChat : boolean = false;	//Can be used to determine if we need to stop player movement since we're chatting
var skin : GUISkin;						//Skin
var showChat : boolean= false;			//Show/Hide the chat

//Private vars used by the script
private var inputField : String= "";

private var scrollPosition : Vector2;
private var width : int= 500;
private var height : int= 180;
private var playerName : String;
private var lastUnfocus : float =0;
private var window : Rect;
	
//Server-only playerlist
private var playerList = new List.<LobbyPlayerNode>();
class LobbyPlayerNode {
	var playerName : String;
	var networkPlayer : NetworkPlayer;
}

private var chatEntries = new List.<LobbyChatEntry>();
class LobbyChatEntry
{
	var name : String= "";
	var text : String= "";	
}


//Client function
function OnConnectedToServer() {
	ShowChatWindow();
	networkView.RPC ("TellServerOurName", RPCMode.Server, playerName);
	// //We could have also announced ourselves:
	// addGameChatMessage(playerName" joined the chat");
	// //But using "TellServer.." we build a list of active players which we can use it for other stuff.
}

//Server function
function OnServerInitialized() {
	ShowChatWindow();
	//I wish Unity supported sending an RPC to the server itself :( (Like OnConnectedToServer();)
	var newEntry : LobbyPlayerNode = new LobbyPlayerNode();
	newEntry.playerName=playerName;
	newEntry.networkPlayer=Network.player;
	playerList.Add(newEntry);	
	addGameChatMessage(playerName+" joined the chat");
}

//Server function
function OnPlayerDisconnected(player: NetworkPlayer) {
	addGameChatMessage("Player disconnected from: " + player.ipAddress+":" + player.port);
	
	//Remove player from the server list
	for(var entry  in  playerList as List.<LobbyPlayerNode>){
		var entry2 = entry as LobbyPlayerNode;
		if(entry2.networkPlayer==player){
			playerList.Remove(entry2);
			break;
		}
	}
}

function OnDisconnectedFromServer(){
	CloseChatWindow();
}

//Server function
function OnPlayerConnected(player: NetworkPlayer) {
	addGameChatMessage("Player connected from: " + player.ipAddress +":" + player.port);
}

@RPC
//Sent by newly connected clients, recieved by server
function TellServerOurName(name : String, info : NetworkMessageInfo){
	var newEntry : LobbyPlayerNode = new LobbyPlayerNode();
	newEntry.playerName=name;
	newEntry.networkPlayer=info.sender;
	playerList.Add(newEntry);
	
	addGameChatMessage(name+" joined the chat");
}


function Awake(){
	window = Rect(Screen.width/2-width/2, Screen.height-height+5, width, height);	
    chatEntries =new List.<LobbyChatEntry>();
}

function CloseChatWindow ()
{
	showChat = false;
	inputField = "";
	chatEntries = new List.<LobbyChatEntry>();
}

function ShowChatWindow ()
{
	//We get the name from the previous masterserver example, if you entered your name there ;).
	playerName = PlayerPrefs.GetString("playerName", "");
	if(!playerName || playerName==""){
		playerName = "RandomName"+Random.Range(1,999);
	}	
	
	showChat = true;
	inputField = "";
	chatEntries =new List.<LobbyChatEntry>();
}

function OnGUI ()
{
	if(!showChat){
		return;
	}
	
	GUI.skin = skin;		
			
	if (Event.current.type == EventType.keyDown && Event.current.character == "\n" && inputField.Length <= 0)
	{
		if(lastUnfocus+0.25<Time.time){
			usingChat=true;
			GUI.FocusWindow(5);
			GUI.FocusControl("Chat input field");
		}
	}
	
	window = GUI.Window (5, window, GlobalChatWindow, "");
}


function GlobalChatWindow (id : int) {
	
	GUILayout.BeginVertical();
	GUILayout.Space(10);
	GUILayout.EndVertical();
	
	// Begin a scroll view. All rects are calculated automatically - 
    // it will use up any available screen space and make sure contents flow correctly.
    // This is kept small with the last two parameters to force scrollbars to appear.
	scrollPosition = GUILayout.BeginScrollView (scrollPosition);

	for (var entry2  in chatEntries as List.<LobbyChatEntry>)
	{
	    var entry : LobbyChatEntry = entry2 as LobbyChatEntry;
		GUILayout.BeginHorizontal();
		if(entry.name==""){//Game message
			GUILayout.Label (entry.text);
		}else{
			GUILayout.Label (entry.name+": "+entry.text);
		}
		GUILayout.EndHorizontal();
		GUILayout.Space(3);
		
	}
	// End the scrollview we began above.
    GUILayout.EndScrollView ();
	
	if (Event.current.type == EventType.keyDown && Event.current.character == "\n" && inputField.Length > 0)
	{
		HitEnter(inputField);
	}
	GUI.SetNextControlName("Chat input field");
	inputField = GUILayout.TextField(inputField);
	
	
	if(Input.GetKeyDown("mouse 0")){
		if(usingChat){
			usingChat=false;
			GUI.UnfocusWindow ();//Deselect chat
			lastUnfocus=Time.time;
		}
	}
}

function HitEnter(msg : String){
	msg = msg.Replace("\n", "");
	networkView.RPC("ApplyGlobalChatText", RPCMode.All, playerName, msg);
	inputField = ""; //Clear line
	GUI.UnfocusWindow ();//Deselect chat
	lastUnfocus=Time.time;
	usingChat=false;
}


@RPC
function ApplyGlobalChatText (name : String, msg : String)
{
	var entry = new LobbyChatEntry();
	entry.name = name;
	entry.text = msg;

	chatEntries.Add(entry);
	
	//Remove old entries
	if (chatEntries.Count > 4){
		chatEntries.RemoveAt(0);
	}

	scrollPosition.y = 1000000;	
}

//Add game messages etc
function addGameChatMessage(str : String){
	ApplyGlobalChatText("", str);
	if(Network.connections.length>0){
		networkView.RPC("ApplyGlobalChatText", RPCMode.Others, "", str);	
	}	
}