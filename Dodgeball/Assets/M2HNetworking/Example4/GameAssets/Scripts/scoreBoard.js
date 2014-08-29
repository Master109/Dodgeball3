/*  This file is part of the "Ultimate Unity networking project" by M2H (http://www.M2H.nl)
 *  This project is available on the Unity Store. You are only allowed to use these
 *  resources if you've bought them from the Unity Assets Store.
 */
import System.Collections.Generic;

var skin : GUISkin;

//static var newRecordMessage : String = "";

private var displayingHighscore : boolean = false;
private var highscoreText : GUIText;

private var playerName : String = "";

private var scoreText : String = "Loading scores";

private var hidestats : boolean = true;
private var scoreBoardHeight :int = 60;
private var gameSetupScript : GameSetup;

function Awake(){
	gameSetupScript = GetComponent(GameSetup) as GameSetup;
	
	highscoreText = GetComponent(GUIText) as GUIText;
	highscoreText.enabled=false;
	playerName= PlayerPrefs.GetString("playerName");
}

function OnGUI () {
	GUI.skin = skin;

	scoreText = "Scoreboard:\n";
	for(var entry : FPSPlayerNode in  gameSetupScript.playerList){
		scoreText += entry.playerName+" \t"+entry.kills+" kills "+entry.deaths+" deaths\n";		
	}

	GUILayout.BeginArea (Rect ((Screen.width-185),20,175,scoreBoardHeight));
	GUILayout.Box(scoreText);
	GUILayout.EndArea ();
}

function GetPlayer(networkP : NetworkPlayer) : FPSPlayerNode{
    for (var playerInstance : FPSPlayerNode in gameSetupScript.playerList as List.<FPSPlayerNode>) {
		if (networkP == playerInstance.networkPlayer) {
			return playerInstance;
		}
	}
	Debug.LogError("GetPlayer couldn't find player! "+networkP);
	return null;
}

function LocalPlayerHasKilled(){
	
	var pNode : FPSPlayerNode= GetPlayer(Network.player);
	pNode.kills +=1;		
	
	//Overwrite the data of other players with the new correct score
	networkView.RPC("UpdateScore",RPCMode.All, Network.player, pNode.kills, pNode.deaths); 
}

function LocalPlayerDied(){
	var pNode : FPSPlayerNode= GetPlayer(Network.player);
	pNode.deaths +=1;
	
	//Overwrite with new correct score
	networkView.RPC("UpdateScore",RPCMode.All, Network.player, pNode.kills, pNode.deaths); 
}

		
@RPC
function UpdateScore(player : NetworkPlayer, kills : int, deaths : int){
	Debug.Log((Network.player==player)+"=local "+kills+"kills & deaths="+deaths);

	var pNode : FPSPlayerNode= GetPlayer(player);
	if(pNode!=null){
		pNode.kills=kills;
		pNode.deaths=deaths;		
	} else {
		Debug.LogError("Could not find network player "+player+" in the gamesetup playerlist!");
	}
	scoreBoardHeight = gameSetupScript.playerList.Count*15+40;		
}