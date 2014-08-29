/*  This file is part of the "Ultimate Unity networking project" by M2H (http://www.M2H.nl)
 *  This project is available on the Unity Store. You are only allowed to use these
 *  resources if you've bought them from the Unity Assets Store.
 */

private var connectToIP : String = "127.0.0.1";
private var connectPort : int = 25001;


//Obviously the GUI is for both client&servers (mixed!)
function OnGUI ()
{
   
	if (Network.peerType == NetworkPeerType.Disconnected){
	//We are currently disconnected: Neither client nor host
		GUILayout.Label("Connection status: Disconnected");
		
		connectToIP = GUILayout.TextField(connectToIP, GUILayout.MinWidth(100));
		connectPort = parseInt(GUILayout.TextField(connectPort.ToString()));
		
		GUILayout.BeginVertical();
		if (GUILayout.Button ("Connect as client"))
		{
			//Connect to the "connectToIP" and "connectPort" as entered via the GUI
			Network.Connect(connectToIP, connectPort);
		}
		
		if (GUILayout.Button ("Start Server"))
		{
			//Start a server for 32 clients using the "connectPort" given via the GUI
			//Ignore the NAT setting for now (False)				
			Network.InitializeServer(32, connectPort, false);
		}
		GUILayout.EndVertical();
		
		
	}else{
		//We've got one or more connection(s)!		

		if (Network.peerType == NetworkPeerType.Connecting){
		
			GUILayout.Label("Connection status: Connecting");
			
		} else if (Network.peerType == NetworkPeerType.Client){
			
			GUILayout.Label("Connection status: Client!");
			GUILayout.Label("Ping to server: "+Network.GetAveragePing(  Network.connections[0] ) );		
			
		} else if (Network.peerType == NetworkPeerType.Server){
			
			GUILayout.Label("Connection status: Server!");
			GUILayout.Label("Connections: "+Network.connections.length);
			if(Network.connections.length>=1){
				GUILayout.Label("Ping to first player: "+Network.GetAveragePing(  Network.connections[0] ) );
			}			
		}

		if (GUILayout.Button ("Disconnect"))
		{
			Network.Disconnect();
		}
	}
	
	
}



// NONE of the functions below is of any use in this demo, the code below is only used for demonstration.
// First ensure you understand the code in the OnGUI() function above.

//Client functions called by Unity
function OnConnectedToServer() {
	Debug.Log("This CLIENT has connected to a server");	
}

function OnDisconnectedFromServer(info : NetworkDisconnection) {
	Debug.Log("This CLIENT has disconnected from a server OR this SERVER was just shut down");
}

function OnFailedToConnect(error: NetworkConnectionError){
	Debug.Log("Could not connect to server: "+ error);
}


//Server functions called by Unity
function OnPlayerConnected(player: NetworkPlayer) {
	Debug.Log("Player connected from: " + player.ipAddress +":" + player.port);
}

function OnServerInitialized() {
	Debug.Log("Server initialized and ready");
}

function OnPlayerDisconnected(player: NetworkPlayer) {
	Debug.Log("Player disconnected from: " + player.ipAddress+":" + player.port);
}

// OTHERS:
// To have a full overview of all network functions called by unity
// the next four have been added here too, but they can be ignored for now

function OnFailedToConnectToMasterServer(info: NetworkConnectionError){
	Debug.Log("Could not connect to master server: "+ info);
}

function OnNetworkInstantiate (info : NetworkMessageInfo) {
	Debug.Log("New object instantiated by " + info.sender);
}

function OnSerializeNetworkView(stream : BitStream, info : NetworkMessageInfo)
{
	//Custom code here (your code!)
}

/* 
 The last Networking function is the RPC.
 RPCs are custom functions that you'll have to define and call yourself.
 They allow you to send/receive any kind of information to one or more targets.
 
 @RPC
 function MyRPCKillMessage(){
	//Looks like I have been killed!
	//Someone send an RPC resulting in this function call
 }
*/