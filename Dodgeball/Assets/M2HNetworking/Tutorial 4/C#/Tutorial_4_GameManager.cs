using UnityEngine;
using System.Collections;

public class Tutorial_4_GameManager : MonoBehaviour
{
    class PlayerInfo4
    {
        public NetworkPlayer networkPlayer;
        public string name;
        public Transform transform;

        public bool IsLocal()
        {
            //If disconnected we are "-1"
            return (Network.player == networkPlayer || Network.player + "" == "-1");
        }
        public PlayerInfo4 Clone()
        {
            PlayerInfo4 pla = new PlayerInfo4();
            pla.networkPlayer = networkPlayer;
            pla.name = name;
            pla.transform = transform;
            return pla;
        }
    }

    public static Tutorial_4_GameManager SP;

    public Transform playerPrefab;

    private ArrayList playerList = new ArrayList();
    private PlayerInfo4 localPlayerInfo;

    void Awake()
    {
        SP = this;
        Network.isMessageQueueRunning = true;

    }

    //////////////////////////////
    // Manage players

    [RPC]
    void AddPlayer(NetworkPlayer networkPlayer, string pname)
    {
        Debug.Log("AddPlayer " + networkPlayer + " name=" + pname);
        if (GetPlayer(networkPlayer) != null)
        {
            Debug.LogError("AddPlayer: Player already exists!");
            return;
        }
        PlayerInfo4 pla = new PlayerInfo4();
        pla.networkPlayer = networkPlayer;
        pla.name = pname;
        playerList.Add(pla);

        if (pla.IsLocal())
        {
            if (localPlayerInfo != null) { Debug.LogError("localPlayerInfo already set?"); }
            localPlayerInfo = pla;
        }
    }


    [RPC]
    void RemovePlayer(NetworkPlayer networkPlayer)
    {
        Debug.Log("RemovePlayer " + networkPlayer);
        PlayerInfo4 thePlayer = GetPlayer(networkPlayer);

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


    PlayerInfo4 GetPlayer(NetworkPlayer networkPlayer)
    {
        foreach (PlayerInfo4 pla in playerList)
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
    void OnServerInitialized()
    {
        SpawnLocalPlayer();
        //SetupLocalPlayer(); 
    }

    //On server:
    void OnPlayerConnected(NetworkPlayer player)
    {
        //Nothing, await the clients own initiative..
    }

    //Client
    void OnConnectedToServer()
    {
        SpawnLocalPlayer();
        //  SetupLocalPlayer();         
    }

    void SpawnLocalPlayer()
    {
        //Spawn local player
        Debug.Log("SpawnLocalPlayer ");

        //Get random spawnpoint
        GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("Spawnpoint");
        GameObject theGO = spawnPoints[Random.Range(0, spawnPoints.Length)];
        Vector3 pos = theGO.transform.position;
        Quaternion rot = theGO.transform.rotation;

        //Manually allocate NetworkViewID
        NetworkViewID id1 = Network.AllocateViewID();

        AddPlayer(Network.player, PlayerPrefs.GetString("playerName"));
        SpawnOnNetwork(pos, rot, id1, true, Network.player);

        networkView.RPC("AddPlayer", RPCMode.OthersBuffered, Network.player, PlayerPrefs.GetString("playerName"));
        networkView.RPC("SpawnOnNetwork", RPCMode.OthersBuffered, pos, rot, id1, false, Network.player);
    }


    [RPC]
    void SpawnOnNetwork(Vector3 pos, Quaternion rot, NetworkViewID id1, bool amOwner, NetworkPlayer np)
    {
        Transform newPlayer = Instantiate(playerPrefab, pos, rot) as Transform;
        //Set transform
        PlayerInfo4 pNode = GetPlayer(np);
        pNode.transform = newPlayer;
        //Set networkviewID everywhere!
        SetNetworkViewIDs(newPlayer.gameObject, id1);

        if (pNode.IsLocal())
        {
            localPlayerInfo = pNode;
        }
        //Maybe call some specific action on the instantiated object?
        //PLAYERSCRIPT tmp = newPlayer.GetComponent<PLAYERSCRIPT>();
        //tmp.SetPlayer(pNode.networkPlayer);
    }

    //When a NetworkView instantiates it has viewID=0 and is unusable.
    //We need to assign the right viewID -on all players(!)- for it to work
    void SetNetworkViewIDs(GameObject go, NetworkViewID id1)
    {
        Component[] nViews = go.GetComponentsInChildren<NetworkView>();
        (nViews[0] as NetworkView).viewID = id1;
    }




    //On server: When client disconnects, cleanup
    void OnPlayerDisconnected(NetworkPlayer player)
    {
        PlayerInfo4 pNode = GetPlayer(player);
        if (pNode != null)
        {
            //string playerNameLeft= pNode.name;
            //I.e.: Chat.SP.addGameChatMessage(playerNameLeft+" left the game");
        }
        networkView.RPC("RemovePlayer", RPCMode.All, player);

        Network.RemoveRPCs(player);
        Network.DestroyPlayerObjects(player);
    }


    //Server OR client: Disconnect
    IEnumerator OnDisconnectedFromServer(NetworkDisconnection info)
    {

        //Remove all players 
        yield return 0;//Wait for actual disconnect
        for (int i = playerList.Count - 1; i >= 0; i--)
        {
            PlayerInfo4 pla = (PlayerInfo4)playerList[i];
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



}