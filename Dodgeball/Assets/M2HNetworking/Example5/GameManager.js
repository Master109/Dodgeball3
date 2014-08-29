 //import UnityEngine;
//import System.Collections;

class PlayerInfo5
{
     public var networkPlayer : NetworkPlayer;
     public var name : String;
     public var transform : Transform;
     public var isLocal : boolean;

    public function Clone () : PlayerInfo5 {
        var pla : PlayerInfo5= new PlayerInfo5();
        pla.networkPlayer = networkPlayer;
        pla.name = name;
        pla.transform = transform;
        pla.isLocal = isLocal;
        return pla;
    }
}

    public var playerPrefab : Transform;

    public static  var SP : GameManager;

    private  var PlayerList : ArrayList;
    private var localPlayerInfo : PlayerInfo5;

    private  var myLocalTransform : Transform;

    private  var mainCamera : Camera;
    private  var serverStartedWithMaxPlayers : int= 0;

    function Awake () : void {
        SP = this;
        PlayerList = new ArrayList();
        mainCamera = Camera.main;

        InvokeRepeating("ServerRegisterHost", 0, 300);

        Network.isMessageQueueRunning = true;

        if (Network.isServer)
        {
            //Server
            serverStartedWithMaxPlayers = Network.maxConnections;
            SpawnLocalPlayer();
            StartCoroutine(ServerStarted());
        }
        else
        {
            SpawnLocalPlayer();
            if (Network.isClient)
            {
                //Client: Already connected                
            }
            else
            {
                //No connection: Try connecting, otherwise host
                StartCoroutine(AutoJoinFeature());
            }
        }
        
        MultiplayerFunctions.SP.SetHostListDelegate(FullHostListReceived);
    }
    
  
    function GetPlayerList () : ArrayList {
         return PlayerList;
    }

    function GetPlayerListCopy () : ArrayList {
        return PlayerList.Clone();
    }

    public var customGUIStyle : GUIStyle;
    private var debugMatchmakingStatus : String = "";
    
    function OnGUI () : void {
        GUILayout.Label("Current matchmaking status="+debugMatchmakingStatus);

        //On-Screen
        for ( var node : PlayerInfo5  in  PlayerList ) {
            if (node.transform)
            {
                //GUI.color=Color.white;
                var screenPos : Vector3= mainCamera.WorldToScreenPoint(node.transform.position  + new Vector3(0,2,0)  );
                if ( screenPos.z > 0 && Vector3.Distance(mainCamera.transform.position, node.transform.position) <= 450)
                {
                    GUI.Label(new Rect(screenPos.x - 45, Screen.height - screenPos.y, 90, 15), node.name, customGUIStyle);
                }
            }
        }
    
    }



    @RPC
    function AddPlayer (networkPlayer : NetworkPlayer,   pname : String  ) : void {
        if (GetPlayer(networkPlayer) != null)
        {
            Debug.LogError("AddPlayer: Player already exists!");
            return;
        }
        var pla : PlayerInfo5= new PlayerInfo5();
        pla.networkPlayer = networkPlayer;
        pla.name = pname;
        PlayerList.Add(pla);

        if (Network.player == networkPlayer || Network.player + "" == "-1")
        {
            pla.isLocal = true;
            localPlayerInfo = pla;
        }
    }

    function SetPlayerTransform ( networkPlayer : NetworkPlayer ,   pTransform : Transform  ) : void {
        if (!pTransform)
        {
            Debug.LogError("SetPlayersTransform has a NULL playerTransform!");
        }
         var thePlayer : PlayerInfo5= GetPlayer(networkPlayer);
        if (thePlayer == null)
        {
            Debug.LogError("SetPlayersPlayerTransform: No player found!");
        }
        thePlayer.transform = pTransform;
    }


    @RPC
    function RemovePlayer ( networkPlayer : NetworkPlayer  ) : void {
         var thePlayer : PlayerInfo5= GetPlayer(networkPlayer);

        Network.RemoveRPCs(networkPlayer);
        if (Network.isServer)
        {
            Network.DestroyPlayerObjects(networkPlayer);
        }
        if (thePlayer.transform)
        {
            Destroy(thePlayer.transform.gameObject);
        }
        PlayerList.Remove(thePlayer);
    }


    function GetPlayer ( networkPlayer : NetworkPlayer  ) : PlayerInfo5 {
        for( var pla : PlayerInfo5 in PlayerList )
        {
            if (pla.networkPlayer == networkPlayer)
            {
                 return pla;
            }
        }
        return null;
    }


    function ServerStarted () : IEnumerator {
        debugMatchmakingStatus="ServerStarted";
                
        PlayerList = new ArrayList();//Clear list
        
        Invoke("ServerRegisterHost", 10); 
        networkView.RPC("AddPlayer", RPCMode.AllBuffered, Network.player, PlayerPrefs.GetString("playerName"));

        var id1 : NetworkViewID= Network.AllocateViewID();
        SetNetworkViewIDs(myLocalTransform.gameObject, id1);
        SetPlayerTransform(Network.player, myLocalTransform);

        networkView.RPC("SpawnOnNetwork", RPCMode.OthersBuffered, transform.position, transform.rotation, id1, PlayerPrefs.GetString("playerName"), false, Network.player);
    }


    function ServerRegisterHost () : void {
        if (!Network.isServer)
        {
            return;
        }
        MultiplayerFunctions.SP.RegisterHost(PlayerPrefs.GetString("playerName") + "", "NoComment");
    }


    function SpawnLocalPlayer () : void {
        //Spawn local player
        // Randomize starting location
        var pos : Vector3= transform.position;
        var rot : Quaternion= Quaternion.identity;

        var spawnPoints : GameObject[]= GameObject.FindGameObjectsWithTag("Spawnpoint");
        var theGO : GameObject= spawnPoints[Random.Range(0, spawnPoints.Length)];
        pos = theGO.transform.position;
        rot = theGO.transform.rotation;

         var id1 : NetworkViewID= new NetworkViewID();// = Network.AllocateViewID();
        if (Network.isClient)
        {
            id1 = Network.AllocateViewID();
        }

        AddPlayer(Network.player, PlayerPrefs.GetString("playerName"));
        StartCoroutine(SpawnOnNetwork(pos, rot, id1, PlayerPrefs.GetString("playerName"), true, Network.player));
        if (Network.isClient)
        {
            networkView.RPC("AddPlayer", RPCMode.OthersBuffered, Network.player, PlayerPrefs.GetString("playerName"));
            networkView.RPC("SpawnOnNetwork", RPCMode.OthersBuffered, pos, rot, id1,  PlayerPrefs.GetString("playerName"), false, Network.player);
        }

    }


    @RPC
    function SpawnOnNetwork ( pos : Vector3 ,   rot : Quaternion ,   id1 : NetworkViewID ,    playerName : String ,   amOwner : boolean ,   np : NetworkPlayer  ) : IEnumerator {
        var newPlayer : Transform= Instantiate(playerPrefab, pos, rot) as Transform;
        SetPlayerTransform(np, newPlayer);

        SetNetworkViewIDs(newPlayer.gameObject, id1);

        if (amOwner)
        {
            myLocalTransform = newPlayer;
        }
         var tmp : Player5 = newPlayer.GetComponent(Player5);
        tmp.SetOwner(amOwner);
    }


    function SetNetworkViewIDs ( go : GameObject ,   id1 : NetworkViewID  ) : void {
         var nViews : Component[]= go.GetComponentsInChildren(NetworkView);
        (nViews[0]  as NetworkView  ).viewID = id1;
    }


    //On client: When just connected to a server
    function OnConnectedToServer () : IEnumerator {
        debugMatchmakingStatus = "ConnectedToServer (client)";
        PlayerList = new ArrayList();

         var id1 : NetworkViewID= Network.AllocateViewID();
        networkView.RPC("AddPlayer", RPCMode.AllBuffered, Network.player, PlayerPrefs.GetString("playerName"));
        networkView.RPC("SpawnOnNetwork", RPCMode.OthersBuffered, myLocalTransform.position, myLocalTransform.rotation, id1,  PlayerPrefs.GetString("playerName"), false, Network.player);
        yield 0;
        SetPlayerTransform(Network.player, myLocalTransform);
        SetNetworkViewIDs(myLocalTransform.gameObject, id1);

    }


    //On server: When client disconnects
    function OnPlayerDisconnected ( player : NetworkPlayer  ) : void {
        var pNode : PlayerInfo5 = GetPlayer(player);
        if(pNode!=null){
            var playerNameLeft = pNode.name;
            Chat.SP.addGameChatMessage(playerNameLeft+" left the game");
        }
        networkView.RPC("RemovePlayer", RPCMode.All, player);

        Network.RemoveRPCs(player);
        Network.DestroyPlayerObjects(player);


    }


    function OnPlayerConnected ( player : NetworkPlayer  ) : void {
        //Nothing
    }


    //On server: When this game just switched from non-networking to networked
    function OnServerInitialized () : void {
        StartCoroutine(ServerStarted());
          
          MultiplayerFunctions.SP.RegisterHost(PlayerPrefs.GetString("playerName") + "s game", "NoComment");
   
    }

    function OnDisconnectedFromServer ( info : NetworkDisconnection  ) : IEnumerator {

        if (Network.isServer)
        {
            //We shut down our own server          
            //Remove all players except yourself
            yield new WaitForSeconds(1);
             var listCopy : ArrayList= PlayerList.Clone();
            for( var pla : PlayerInfo5 in listCopy )
            {
                if (pla.networkPlayer != Network.player && (pla.networkPlayer + "") != "0")
                {
                    RemovePlayer(pla.networkPlayer);
                }
            }
            //The auto join feature should make sure you get a new connection
            if (!autoJoinRunning)
            {
                StartCoroutine(AutoJoinFeature());
            }
        }
        else
        {
            if (info == NetworkDisconnection.LostConnection)
            {
                //Debug.LogWarning("Lost connection to the server");
            }
            else
            {
                //Debug.LogWarning("Successfully disconnected from the server !?");
            }
            //Remove all players except yourself

            yield new WaitForSeconds(1);
             var listCopy2 : ArrayList= PlayerList.Clone();
            for(var pla : PlayerInfo5   in listCopy2 )
            {
                if (pla.networkPlayer != Network.player)
                {
                    RemovePlayer(pla.networkPlayer);
                    Network.CloseConnection(pla.networkPlayer, false);
                }
            }

            if (!autoJoinRunning)
            {
                StartCoroutine(AutoJoinFeature());
            }
        }
    }

    private var autoJoinRunning : boolean= false;

    //This runs on disconnected clients..trying to connect to every possible host..
    function AutoJoinFeature () : IEnumerator {
        autoJoinRunning = true;
        
        while(!MultiplayerFunctions.SP.ReadyLoading()){
            yield 0;//Wait for masterserver connection and connection tester
        }
        
 
        //Try to join games...otherwise..HOST
        while (Network.connections.Length == 0)
        {
            if (!Network.isServer)
            {
                yield StartCoroutine(AutoJoinTryConnecting());
            }
            //yield 0;            
            if (Network.connections.Length <= 0)
            {
                yield StartCoroutine(AutoJoinTryHosting());
            }
            yield 0;
        }
        
        autoJoinRunning = false;
    }

    private var hostData : HostData[]= null;

    function FullHostListReceived() : void {
        hostData = MultiplayerFunctions.SP.GetHostData();        
    }
    
    function AutoJoinTryConnecting () : IEnumerator {
        //Get host list
        for ( var retries : int= 0; (retries < 3 && (hostData == null || hostData.Length >= 0)); retries++)
        {
            MultiplayerFunctions.SP.FetchHostList();
            yield new WaitForSeconds(2);
        }
        
        if (hostData == null || hostData.Length == 0)
        {
            Debug.LogWarning("hostdata is empty :(");
             return;
        }

        //Connect to games in the list
        for (var element  : HostData  in hostData ){
            if (Network.connections.Length > 0)
            {
                 return;
            }
            yield 0;
            //Cant be full, must be same level	
            if (element.connectedPlayers < element.playerLimit) //&& int.Parse(element.comment) == Application.loadedLevel)
            {
                if (Network.connections.Length > 0)
                {
                     return;
                }
                MultiplayerFunctions.SP.HostDataConnect(element, "", true, null);            
                yield new WaitForSeconds(1.5f);
                if (Network.connections.Length > 0)
                {
                     return;
                }              
            }
        }
    }


    function AutoJoinTryHosting () : IEnumerator {
        MultiplayerFunctions.SP.StartServer("", MultiplayerFunctions.SP.defaultServerPort, 8, true);
        serverStartedWithMaxPlayers = 8;
        
        for ( var i : int= 0; i < 3; i++)
        {
            //Debug.LogWarning("trying to host...");
            yield new WaitForSeconds(Random.Range(20, 50));
            if (Network.connections.Length > 0)
            {
                 return;
            }
        }
        Network.Disconnect();
       return;
    }

