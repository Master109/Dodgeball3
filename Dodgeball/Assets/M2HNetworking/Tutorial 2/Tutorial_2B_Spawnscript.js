/*  This file is part of the "Ultimate Unity networking project" by M2H (http://www.M2H.nl)
 *  This project is available on the Unity Store. You are only allowed to use these
 *  resources if you've bought them from the Unity Assets Store.
 */

public var playerPrefab : Transform;


function OnServerInitialized(){
	Spawnplayer();
}

function OnConnectedToServer(){
	Spawnplayer();
}

function Spawnplayer(){	
	var myNewTrans : Transform = Network.Instantiate(playerPrefab, transform.position, transform.rotation, 0) as Transform;
}




function OnPlayerDisconnected(player: NetworkPlayer) {
	Debug.Log("Clean up after player " + player);
	Network.RemoveRPCs(player);
	Network.DestroyPlayerObjects(player);
}

function OnDisconnectedFromServer(info : NetworkDisconnection) {
	Debug.Log("Clean up a bit after server quit");
	Network.RemoveRPCs(Network.player);
	Network.DestroyPlayerObjects(Network.player);
	
	/* 
	* Note that we only remove our own objects, but we cannot remove the other players 
	* objects since we don't know what they are; we didn't keep track of them. 
	* In a game you would usually reload the level or load the main menu level anyway ;).
	* 
	* To reset the scene we'll just reload it:
	*/
	Application.LoadLevel(Application.loadedLevel);
}