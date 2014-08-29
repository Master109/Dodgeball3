/*  This file is part of the "Ultimate Unity networking project" by M2H (http://www.M2H.nl)
 *  This project is available on the Unity Store. You are only allowed to use these
 *  resources if you've bought them from the Unity Assets Store.
 */

function Awake(){
	if(!networkView.isMine){
		//We aren't the networkView owner, disable this script
		//RPC's and OnSerializeNetworkView will STILL get trough but we prevent Update from running
		enabled=false;	
	}
}

function Update(){
	
	if(networkView.isMine){
		//Only the owner can move the cube!	
		//(Ok this check is a bit overkill since we did already disable the script in Awake)		
		var moveDirection : Vector3 = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
		var speed : float = 5;
		transform.Translate(speed * moveDirection * Time.deltaTime);
	}
	
}


function OnSerializeNetworkView(stream : BitStream, info : NetworkMessageInfo)
{
	if (stream.isWriting){
		//Executed on the owner of this networkview; 
		//The server sends it's position over the network
		
		var pos : Vector3 = transform.position;		
		stream.Serialize(pos);//"Encode" it, and send it
				
	}else{
		//Executed on the others; 
		//receive a position and set the object to it
		
		var posReceive : Vector3 = Vector3.zero;
		stream.Serialize(posReceive); //"Decode" it and receive it
		transform.position = posReceive;
		
	}
}