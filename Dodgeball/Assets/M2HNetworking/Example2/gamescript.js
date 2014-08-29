/*  This file is part of the "Ultimate Unity networking project" by M2H (http://www.M2H.nl)
 *  This project is available on the Unity Store. You are only allowed to use these
 *  resources if you've bought them from the Unity Assets Store.
 */
#pragma strict


function Awake(){
	//RE-enable the network messages now we've loaded the right level
	Network.isMessageQueueRunning = true;
	
	if(Network.isServer){
		Debug.Log("Server registered the game at the masterserver.");
		MultiplayerFunctions.SP.RegisterHost(GameSettings.serverTitle, GameSettings.description);
	}
}


function OnGUI ()
{

	if (Network.peerType == NetworkPeerType.Disconnected){
	//We are currently disconnected: Not a client or host
		GUILayout.Label("Connection status: We've (been) disconnected");
		if(GUILayout.Button("Back to main menu")){
			Application.LoadLevel(Application.loadedLevel-1);
		}
		
	}else{
		//We've got a connection(s)!
		

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

function OnDisconnectedFromServer(info : NetworkDisconnection) {
	if (Network.isServer) {
        Debug.Log("Local server connection disconnected");
    }else {
        if (info == NetworkDisconnection.LostConnection)
            Debug.Log("Lost connection to the server");
        else
            Debug.Log("Successfully diconnected from the server");
    }
}


//Server functions called by Unity
function OnPlayerConnected(player: NetworkPlayer) {
	Debug.Log("Player connected from: " + player.ipAddress +":" + player.port);
}

function OnPlayerDisconnected(player: NetworkPlayer) {
	Debug.Log("Player disconnected from: " + player.ipAddress+":" + player.port);
}