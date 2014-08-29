/*  This file is part of the "Ultimate Unity networking project" by M2H (http://www.M2H.nl)
 *  This project is available on the Unity Store. You are only allowed to use these
 *  resources if you've bought them from the Unity Assets Store.
 */

var thisName : String = "Bugged name";
var rigidBodyView : NetworkView;
var hp : int = 100;
var theScoreBoard : scoreBoard;
var localPlayer : boolean = false;

var metalMaterial : Material;
private var orgMaterial : Material;


private var coloredUntill : float;
private var invincible : boolean;


function Awake(){
	orgMaterial = renderer.material;
	
	theScoreBoard= GameObject.Find("Generalscripts").GetComponent(scoreBoard) as scoreBoard;
}

function OnNetworkInstantiate (msg : NetworkMessageInfo) {
	// This is our own player
	if (networkView.isMine)
	{
		//camera.main.enabled=false;

		
		localPlayer=true;
		networkView.RPC("setName", RPCMode.Others, thisName);
		
		Destroy(GameObject.Find("LevelCamera"));
		thisName=PlayerPrefs.GetString("playerName");
		
		var gun : Machinegun = transform.Find("CrateCamera/Weapon").GetComponent("Machinegun") as Machinegun;
		gun.localPlayer=true;
		

		
	}
	// This is just some remote controlled player, don't execute direct
	// user input on this. DO enable multiplayer controll
	else
	{
		thisName="Remote"+Random.Range(1,10);
		name += thisName;
	
		transform.Find("CrateCamera").gameObject.active=false;

		var tmp2 : FPSWalker4 = GetComponent(FPSWalker4) as FPSWalker4;
		tmp2.enabled = false;
		var tmp5 : MouseLook = GetComponent(MouseLook) as MouseLook;
		tmp5.enabled = false;
		
		networkView.RPC("askName", networkView.viewID.owner, Network.player);
		
	
	}
}



function OnGUI(){
	if(localPlayer){
		GUILayout.Label("HP: "+hp);
	}
}


@RPC
function StartInvincibility(){
	invincible=true;
	renderer.material=metalMaterial;
	
	yield new WaitForSeconds (10);
	
	renderer.material=orgMaterial;	
	invincible=false;
}


function ApplyDamage (info : String[]){
	var damage : float= parseFloat(info[0]);
	var killerName : String= info[1];

	if(invincible){
		return;
	}
	
	hp -= damage;
	if(hp<0){
		theScoreBoard.LocalPlayerHasKilled();
		networkView.RPC("Respawn",RPCMode.All);
	}else{
		networkView.RPC("setHP",RPCMode.Others, hp); 
	}
}


@RPC
function setHP(newHP : int){
	hp=newHP;
}



@RPC
function Respawn(){
	if (networkView.isMine)
	{
		theScoreBoard.LocalPlayerDied();
			
		// Randomize starting location
		var spawnpoints : GameObject[] = GameObject.FindGameObjectsWithTag ("Spawnpoint");
		var spawnpoint : Transform = spawnpoints[Random.Range(0, spawnpoints.length)].transform;
	
		transform.position=spawnpoint.position;
		transform.rotation=spawnpoint.rotation;	
	}
	hp=100;
}



@RPC
function setName(name : String){
	thisName=name;
}

@RPC
function askName(asker : NetworkPlayer){
	networkView.RPC("setName", asker, thisName);
}