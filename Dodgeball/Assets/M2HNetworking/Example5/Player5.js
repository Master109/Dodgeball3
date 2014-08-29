
private var myPlayer : boolean = false;

function SetOwner ( amOwner : boolean  ) : void {
   myPlayer = amOwner;
   
    // This is our own player
    if (amOwner){
       
        

        //////Destroy(transform.Find("playerName"));
    }
    // This is just some remote controlled player, don't execute direct
    // user input on this
    else
    {
      
    }
}

function Update(){
    if(myPlayer){			
		var moveDirection : Vector3 = new Vector3(-1*Input.GetAxis("Vertical"), 0, Input.GetAxis("Horizontal"));
		var speed : float = 5;
		transform.Translate(speed * moveDirection * Time.deltaTime);//now really move!
	}
}

