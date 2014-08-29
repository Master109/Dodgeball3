/*  This file is part of the "Ultimate Unity networking project" by M2H (http://www.M2H.nl)
 *  This project is available on the Unity Store. You are only allowed to use these
 *  resources if you've bought them from the Unity Assets Store.
 */
 import System.Collections.Generic;

public var playerPrefab : Transform;
public var playerScripts : List.<Tutorial_3_Playerscript> = new List.<Tutorial_3_Playerscript>();

function OnServerInitialized(){
	//Spawn a player for the server itself
	Spawnplayer(Network.player);
}

function OnPlayerConnected(newPlayer: NetworkPlayer) {
	//A player connected to me(the server), spawn a player for it:
	Spawnplayer(newPlayer);
}	

	
function Spawnplayer(newPlayer : NetworkPlayer){
	//Called on the server only
	
	var playerNumber : int = parseInt(newPlayer+"");
	//Instantiate a new object for this player, remember; the server is therefore the owner.
	var myNewTrans : Transform = Network.Instantiate(playerPrefab, transform.position, transform.rotation, 0) as Transform;
	
	//Get the networkview of this new transform
	var newObjectsNetworkview : NetworkView = myNewTrans.networkView;
	
	//Keep track of this new player so we can properly destroy it when required.
	playerScripts.Add(myNewTrans.GetComponent(Tutorial_3_Playerscript) as Tutorial_3_Playerscript);
	
	//Call an RPC on this new networkview, set the NetworkPlayer who controls this new player
	newObjectsNetworkview.RPC("SetPlayer", RPCMode.AllBuffered, newPlayer);//Set it on the owner
}



function OnPlayerDisconnected(player: NetworkPlayer) {
	Debug.Log("Clean up after player " + player);

	for(var scripta  in playerScripts){
	var script : Tutorial_3_Playerscript = scripta as Tutorial_3_Playerscript;
		if(player==script.owner){//We found the players object
			Network.RemoveRPCs(script.gameObject.networkView.viewID);//remove the bufferd SetPlayer call
			Network.Destroy(script.gameObject);//Destroying the GO will destroy everything
			playerScripts.Remove(script);//Remove this player from the list
			break;
		}
	}
	
	//Remove the buffered RPC call for this player (SetPlayer, line 37)
	var playerNumber : int = parseInt(player+"");
	Network.RemoveRPCs(Network.player, playerNumber);
	
	// The next destroys will not destroy anything since the players never
	// instantiated anything nor buffered RPCs
	Network.RemoveRPCs(player);
	Network.DestroyPlayerObjects(player);
}

function OnDisconnectedFromServer(info : NetworkDisconnection) {
	Debug.Log("Resetting the scene the easy way.");
	Application.LoadLevel(Application.loadedLevel);	
}
