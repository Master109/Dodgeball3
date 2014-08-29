using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    //import UnityEngine;
    //import System.Collections;

    class PlayerInfo5
    {
        public NetworkPlayer networkPlayer;
        public string name;
        public Transform transform;
        public bool isLocal;

        public PlayerInfo5 Clone()
        {
            PlayerInfo5 pla = new PlayerInfo5();
            pla.networkPlayer = networkPlayer;
            pla.name = name;
            pla.transform = transform;
            pla.isLocal = isLocal;
            return pla;
        }
    }

    public Transform playerPrefab;

    public static GameManager SP;

    private ArrayList PlayerList;

    private Transform myLocalTransform;

    private Camera mainCamera;

    void Awake()
    {
        SP = this;
        PlayerList = new ArrayList();
        mainCamera = Camera.main;

        InvokeRepeating("ServerRegisterHost", 0, 300);

        Network.isMessageQueueRunning = true;

        if (Network.isServer)
        {
            //Server
            SpawnLocalPlayer();
            ServerStarted();
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


    ArrayList GetPlayerList()
    {
        return PlayerList;
    }

    ArrayList GetPlayerListCopy()
    {
        return (ArrayList)PlayerList.Clone();
    }

    public GUIStyle customGUIStyle;
    private string debugMatchmakingStatus = "";

    void OnGUI()
    {
        GUILayout.Label("Current matchmaking status=" + debugMatchmakingStatus);

        //On-Screen
        foreach (PlayerInfo5 node in PlayerList)
        {
            if (node.transform)
            {
                //GUI.color=Color.white;
                Vector3 screenPos = mainCamera.WorldToScreenPoint(node.transform.position + new Vector3(0, 2, 0));
                if (screenPos.z > 0 && Vector3.Distance(mainCamera.transform.position, node.transform.position) <= 450)
                {
                    GUI.Label(new Rect(screenPos.x - 45, Screen.height - screenPos.y, 90, 15), node.name, customGUIStyle);
                }
            }
        }

    }



    [RPC]
    void AddPlayer(NetworkPlayer networkPlayer, string pname)
    {
        if (GetPlayer(networkPlayer) != null)
        {
            Debug.LogError("AddPlayer: Player already exists!");
            return;
        }
        PlayerInfo5 pla = new PlayerInfo5();
        pla.networkPlayer = networkPlayer;
        pla.name = pname;
        PlayerList.Add(pla);

        if (Network.player == networkPlayer || Network.player + "" == "-1")
        {
            pla.isLocal = true;
        }
    }

    void SetPlayerTransform(NetworkPlayer networkPlayer, Transform pTransform)
    {
        if (!pTransform)
        {
            Debug.LogError("SetPlayersTransform has a NULL playerTransform!");
        }
        PlayerInfo5 thePlayer = GetPlayer(networkPlayer);
        if (thePlayer == null)
        {
            Debug.LogError("SetPlayersPlayerTransform: No player found!");
        }
        thePlayer.transform = pTransform;
    }


    [RPC]
    void RemovePlayer(NetworkPlayer networkPlayer)
    {
        PlayerInfo5 thePlayer = GetPlayer(networkPlayer);

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


    PlayerInfo5 GetPlayer(NetworkPlayer networkPlayer)
    {
        foreach (PlayerInfo5 pla in PlayerList)
        {
            if (pla.networkPlayer == networkPlayer)
            {
                return pla;
            }
        }
        return null;
    }


    void ServerStarted()
    {
        debugMatchmakingStatus = "ServerStarted";

        PlayerList = new ArrayList();//Clear list

        MultiplayerFunctions.SP.RegisterHost(PlayerPrefs.GetString("playerName") + "s game", "NoComment");

        networkView.RPC("AddPlayer", RPCMode.AllBuffered, Network.player, PlayerPrefs.GetString("playerName"));

        NetworkViewID id1 = Network.AllocateViewID();
        SetNetworkViewIDs(myLocalTransform.gameObject, id1);
        SetPlayerTransform(Network.player, myLocalTransform);

        networkView.RPC("SpawnOnNetwork", RPCMode.OthersBuffered, transform.position, transform.rotation, id1, PlayerPrefs.GetString("playerName"), false, Network.player);
    }


    void ServerRegisterHost()
    {
        if (!Network.isServer)
        {
            return;
        }
        MultiplayerFunctions.SP.RegisterHost(PlayerPrefs.GetString("playerName") + "", "NoComment");
    }


    void SpawnLocalPlayer()
    {
        //Spawn local player
        // Randomize starting location
        Vector3 pos = transform.position;
        Quaternion rot = Quaternion.identity;

        GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("Spawnpoint");
        GameObject theGO = spawnPoints[Random.Range(0, spawnPoints.Length)];
        pos = theGO.transform.position;
        rot = theGO.transform.rotation;

        NetworkViewID id1 = new NetworkViewID();// = Network.AllocateViewID();
        if (Network.isClient)
        {
            id1 = Network.AllocateViewID();
        }

        AddPlayer(Network.player, PlayerPrefs.GetString("playerName"));
        SpawnOnNetwork(pos, rot, id1, PlayerPrefs.GetString("playerName"), true, Network.player);
        if (Network.isClient)
        {
            networkView.RPC("AddPlayer", RPCMode.OthersBuffered, Network.player, PlayerPrefs.GetString("playerName"));
            networkView.RPC("SpawnOnNetwork", RPCMode.OthersBuffered, pos, rot, id1, PlayerPrefs.GetString("playerName"), false, Network.player);
        }

    }


    [RPC]
    void SpawnOnNetwork(Vector3 pos, Quaternion rot, NetworkViewID id1, string playerName, bool amOwner, NetworkPlayer np)
    {
        Transform newPlayer = Instantiate(playerPrefab, pos, rot) as Transform;
        SetPlayerTransform(np, newPlayer);

        SetNetworkViewIDs(newPlayer.gameObject, id1);

        if (amOwner)
        {
            myLocalTransform = newPlayer;
        }
        Player5 tmp = newPlayer.GetComponent<Player5>();
        tmp.SetOwner(amOwner);
    }


    void SetNetworkViewIDs(GameObject go, NetworkViewID id1)
    {
        Component[] nViews = go.GetComponentsInChildren<NetworkView>();
        (nViews[0] as NetworkView).viewID = id1;
    }


    //On client: When just connected to a server
    IEnumerator OnConnectedToServer()
    {
        debugMatchmakingStatus = "ConnectedToServer (client)";
        PlayerList = new ArrayList();

        NetworkViewID id1 = Network.AllocateViewID();
        networkView.RPC("AddPlayer", RPCMode.AllBuffered, Network.player, PlayerPrefs.GetString("playerName"));
        networkView.RPC("SpawnOnNetwork", RPCMode.OthersBuffered, myLocalTransform.position, myLocalTransform.rotation, id1, PlayerPrefs.GetString("playerName"), false, Network.player);
        yield return 0;
        SetPlayerTransform(Network.player, myLocalTransform);
        SetNetworkViewIDs(myLocalTransform.gameObject, id1);

    }


    //On server: When client disconnects
    void OnPlayerDisconnected(NetworkPlayer player)
    {
        PlayerInfo5 pNode = GetPlayer(player);
        if (pNode != null)
        {
            string playerNameLeft = pNode.name;
            Chat.SP.addGameChatMessage(playerNameLeft + " left the game");
        }
        networkView.RPC("RemovePlayer", RPCMode.All, player);

        Network.RemoveRPCs(player);
        Network.DestroyPlayerObjects(player);


    }


    void OnPlayerConnected(NetworkPlayer player)
    {
        //Nothing
    }


    //On server: When this game just switched from non-networking to networked
    void OnServerInitialized()
    {
        ServerStarted();        
    }

    IEnumerator OnDisconnectedFromServer(NetworkDisconnection info)
    {

        if (Network.isServer)
        {
            //We shut down our own server          
            //Remove all players except yourself
            yield return new WaitForSeconds(1);
            ArrayList listCopy = (ArrayList)PlayerList.Clone();
            foreach (PlayerInfo5 pla in listCopy)
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

            yield return new WaitForSeconds(1);
            ArrayList listCopy2 = (ArrayList)PlayerList.Clone();
            foreach (PlayerInfo5 pla in listCopy2)
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

    private bool autoJoinRunning = false;

    //This runs on disconnected clients..trying to connect to every possible host..
    IEnumerator AutoJoinFeature()
    {
        autoJoinRunning = true;

        while (!MultiplayerFunctions.SP.ReadyLoading())
        {
            yield return 0;//Wait for masterserver connection and connection tester
        }


        //Try to join games...otherwise..HOST
        while (Network.connections.Length == 0)
        {
            if (!Network.isServer)
            {
                yield return StartCoroutine(AutoJoinTryConnecting());
            }
            //yield return 0;            
            if (Network.connections.Length <= 0)
            {
                yield return StartCoroutine(AutoJoinTryHosting());
            }
            yield return 0;
        }

        autoJoinRunning = false;
    }

    private HostData[] hostData = null;

    void FullHostListReceived()
    {
        hostData = MultiplayerFunctions.SP.GetHostData();
    }

    IEnumerator AutoJoinTryConnecting()
    {
        //Get host list
        for (int retries = 0; (retries < 3 && (hostData == null || hostData.Length >= 0)); retries++)
        {
            MultiplayerFunctions.SP.FetchHostList();
            yield return new WaitForSeconds(2);
        }

        if (hostData == null || hostData.Length == 0)
        {
            Debug.LogWarning("hostdata is empty :(");
            yield break;
        }

        //Connect to games in the list
        foreach (HostData element in hostData)
        {
            if (Network.connections.Length > 0)
            {
                yield break;
            }
            yield return 0;
            //Cant be full, must be same level	
            if (element.connectedPlayers < element.playerLimit) //&& int.Parse(element.comment) == Application.loadedLevel)
            {
                if (Network.connections.Length > 0)
                {
                    yield break;
                }
                MultiplayerFunctions.SP.HostDataConnect(element, "", true, null);
                yield return new WaitForSeconds(1.5f);
                if (Network.connections.Length > 0)
                {
                    yield break;
                }
            }
        }
    }


    IEnumerator AutoJoinTryHosting()
    {
        MultiplayerFunctions.SP.StartServer("", MultiplayerFunctions.SP.defaultServerPort, 8, true);

        for (int i = 0; i < 3; i++)
        {
            //Debug.LogWarning("trying to host...");
            yield return new WaitForSeconds(Random.Range(20, 50));
            if (Network.connections.Length > 0)
            {
                yield break;
            }
        }
        Network.Disconnect();
    }


}