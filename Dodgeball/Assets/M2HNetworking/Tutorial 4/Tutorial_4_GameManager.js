class PlayerInfo4
{
     public var networkPlayer : NetworkPlayer;
     public var name : String;
     public var transform : Transform;

    public function IsLocal() : boolean {
        //If disconnected we are "-1"
        return (Network.player==networkPlayer || Network.player + "" == "-1");
    }
    public function Clone () : PlayerInfo4 {
        var pla : PlayerInfo4= new PlayerInfo4();
        pla.networkPlayer = networkPlayer;
        pla.name = name;
        pla.transform = transform;
        return pla;
    }
}

    public static  var SP : Tutorial_4_GameManager;

    public var playerPrefab : Transform;
    
    private  var playerList : ArrayList =  new ArrayList();
    private var localPlayerInfo : PlayerInfo4;

    function Awake () : void {
        SP = this;
        Network.isMessageQueueRunning = true;
             
    }
    
    //////////////////////////////
    // Manage players

    @RPC
    function AddPlayer (networkPlayer : NetworkPlayer,   pname : String  ) : void {
        Debug.Log("AddPlayer "+networkPlayer+" name="+pname);
        if (GetPlayer(networkPlayer) != null)
        {
            Debug.LogError("AddPlayer: Player already exists!");
            return;
        }
        var pla : PlayerInfo4 = new PlayerInfo4();
        pla.networkPlayer = networkPlayer;
        pla.name = pname;
        playerList.Add(pla);

        if (pla.IsLocal())
        {
            localPlayerInfo = pla;
        }
    }


    @RPC
    function RemovePlayer ( networkPlayer : NetworkPlayer  ) : void {
         Debug.Log("RemovePlayer "+networkPlayer);
         var thePlayer : PlayerInfo4 = GetPlayer(networkPlayer);

        Network.RemoveRPCs(networkPlayer);
        if (Network.isServer)
        {
            Network.DestroyPlayerObjects(networkPlayer);
        }
        if (thePlayer.transform)
        {
            Destroy(thePlayer.transform.gameObject);
        }
        playerList.Remove(thePlayer);
    }


    function GetPlayer ( networkPlayer : NetworkPlayer  ) : PlayerInfo4 {
        for( var pla : PlayerInfo4 in playerList )
        {
            if (pla.networkPlayer == networkPlayer)
            {
                 return pla;
            }
        }
        return null;
    }

    ////////////////////////////
    // STARTUP: Spawn own player

    //Server
    function OnServerInitialized () : void {
        SpawnLocalPlayer(); 
    }
 
    //On server:
    function OnPlayerConnected ( player : NetworkPlayer  ) : void {
        //Nothing, await the clients own initiative..
    }
    
    //Client
    function OnConnectedToServer () : IEnumerator {
        SpawnLocalPlayer();       
    }    
    
    function SpawnLocalPlayer () : void {
        //Spawn local player
        Debug.Log("SpawnLocalPlayer ");
        
        //Get random spawnpoint
        var spawnPoints : GameObject[]= GameObject.FindGameObjectsWithTag("Spawnpoint");
        var theGO : GameObject= spawnPoints[Random.Range(0, spawnPoints.Length)];
        var pos : Vector3 = theGO.transform.position;
        var rot : Quaternion = theGO.transform.rotation;

        //Manually allocate NetworkViewID
        var id1 : NetworkViewID = Network.AllocateViewID();

        AddPlayer(Network.player, PlayerPrefs.GetString("playerName"));
        SpawnOnNetwork(pos, rot, id1, true, Network.player);

        networkView.RPC("AddPlayer", RPCMode.OthersBuffered, Network.player, PlayerPrefs.GetString("playerName"));
        networkView.RPC("SpawnOnNetwork", RPCMode.OthersBuffered, pos, rot, id1, false, Network.player);
    }


    @RPC
    function SpawnOnNetwork ( pos : Vector3 ,   rot : Quaternion ,   id1 : NetworkViewID ,  amOwner : boolean,   np : NetworkPlayer  )  {
        var newPlayer : Transform= Instantiate(playerPrefab, pos, rot) as Transform;
        //Set transform
        var pNode : PlayerInfo4 = GetPlayer(np);
        pNode.transform = newPlayer;        
        //Set networkviewID everywhere!
        SetNetworkViewIDs(newPlayer.gameObject, id1);

        if (pNode.IsLocal())
        {
            localPlayerInfo = pNode;
        }
        //Maybe call some specific action on the instantiated object?
        //var tmp : PLAYERSCRIPT = newPlayer.GetComponent(PLAYERSCRIPT);
        //tmp.SetPlayer(pNode.networkPlayer);
    }

    //When a NetworkView instantiates it has viewID=0 and is unusable.
    //We need to assign the right viewID -on all players(!)- for it to work
    function SetNetworkViewIDs ( go : GameObject ,   id1 : NetworkViewID  ) : void {
         var nViews : Component[]= go.GetComponentsInChildren(NetworkView);
        (nViews[0]  as NetworkView  ).viewID = id1;
    }




    //On server: When client disconnects, cleanup
    function OnPlayerDisconnected ( player : NetworkPlayer  ) : void {
        var pNode : PlayerInfo4 = GetPlayer(player);
        if(pNode!=null){
            var playerNameLeft = pNode.name;
            //I.e.: Chat.SP.addGameChatMessage(playerNameLeft+" left the game");
        }
        networkView.RPC("RemovePlayer", RPCMode.All, player);

        Network.RemoveRPCs(player);
        Network.DestroyPlayerObjects(player);
    }


    //Server OR client: Disconnect
    function OnDisconnectedFromServer(info : NetworkDisconnection) : IEnumerator {

        //Remove all players 
        yield 0;//Wait for actual disconnect
        for(var i : int=playerList.Count-1;i>=0;i--)
        {
            var pla : PlayerInfo4 = playerList[i];
            ////if (pla.networkPlayer != Network.player && (pla.networkPlayer + "") != "0") //except yourself
            //// { 
                RemovePlayer(pla.networkPlayer);
            //// }
        }
       
        //Other stuff?     
        if (Network.isServer)
        {
            //We shut down our own server          
            
        }
        else
        {
            if (info == NetworkDisconnection.LostConnection)
            {
                //Debug.LogWarning("Client Lost connection to the server");
            }
            else
            {
                //Debug.LogWarning("Client Successfully disconnected from the server");
            }          
        }
    }

   
