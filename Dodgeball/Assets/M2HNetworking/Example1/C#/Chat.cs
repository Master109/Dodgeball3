using UnityEngine;
using System.Collections;

public class Chat : MonoBehaviour
{
    /*  This file is part of the "Ultimate Unity networking project" by M2H (http://www.M2H.nl)
     *  This project is available on the Unity Store. You are only allowed to use these
     *  resources if you've bought them from the Unity Assets Store.
     */



    public bool usingChat = false;	//Can be used to determine if we need to stop player movement since we're chatting
    public GUISkin skin;						//Skin
    public bool showChat = false;			//Show/Hide the chat
    public static Chat SP;

    //Private vars used by the script
    private string inputField = "";

    private Vector2 scrollPosition;
    private int width = 500;
    private int height = 180;
    private string playerName;
    private float lastUnfocusTime = 0;
    private Rect window;

    //Server-only playerlist
    private ArrayList playerList = new ArrayList();
    public class PlayerNode
    {
        public string playerName;
        public NetworkPlayer networkPlayer;
    }

    private ArrayList chatEntries = new ArrayList();
    public class ChatEntry
    {
        public string name = "";
        public string text = "";
    }

    void Awake()
    {
        window = new Rect(Screen.width / 2 - width / 2, Screen.height - height + 5, width, height);
        SP = this;

        //We get the name from the masterserver example, if you entered your name there ;).
        playerName = PlayerPrefs.GetString("playerName", "");
        if (playerName == null || playerName == "")
        {
            playerName = "RandomName" + Random.Range(1, 999);
        }

    }


    //Client function
    void OnConnectedToServer()
    {
        ShowChatWindow();
        networkView.RPC("TellServerOurName", RPCMode.Server, playerName);
        // //We could also announce ourselves:
        // addGameChatMessage(playerName" joined the chat");
        // //But using "TellServer.." we build a list of active players which we can use for other stuff as well.
    }

    //Server function
    void OnServerInitialized()
    {
        ShowChatWindow();
        //I wish Unity supported sending an RPC on the server to the server itself :(
        // If so; we could use the same line as in "OnConnectedToServer();"
        PlayerNode newEntry = new PlayerNode();
        newEntry.playerName = playerName;
        newEntry.networkPlayer = Network.player;
        playerList.Add(newEntry);
        addGameChatMessage(playerName + " joined the chat");
    }

    //A handy wrapper function to get the PlayerNode by networkplayer
    PlayerNode GetPlayerNode(NetworkPlayer networkPlayer)
    {
        foreach (var entry in playerList)
        {
            if ((entry as PlayerNode).networkPlayer == networkPlayer)
            {
                return (entry as PlayerNode);
            }
        }
        Debug.LogError("GetPlayerNode: Requested a playernode of non-existing player!");
        return null;
    }


    //Server function
    void OnPlayerDisconnected(NetworkPlayer player)
    {
        addGameChatMessage("Player disconnected from: " + player.ipAddress + ":" + player.port);

        //Remove player from the server list
        playerList.Remove(GetPlayerNode(player));
    }

    void OnDisconnectedFromServer()
    {
        CloseChatWindow();
    }

    //Server function
    void OnPlayerConnected(NetworkPlayer player)
    {
        addGameChatMessage("Player connected from: " + player.ipAddress + ":" + player.port);
    }

    [RPC]
    //Sent by newly connected clients, recieved by server
    void TellServerOurName(string name, NetworkMessageInfo info)
    {
        PlayerNode newEntry = new PlayerNode();
        newEntry.playerName = name;
        newEntry.networkPlayer = info.sender;
        playerList.Add(newEntry);

        addGameChatMessage(name + " joined the chat");
    }




    public void CloseChatWindow()
    {
        showChat = false;
        inputField = "";
        chatEntries = new ArrayList();
    }

    public void ShowChatWindow()
    {
        showChat = true;
        inputField = "";
        chatEntries = new ArrayList();
    }

    void OnGUI()
    {
        if (!showChat)
        {
            return;
        }

        GUI.skin = skin;

        if (Event.current.type == EventType.keyDown && Event.current.character == '\n' && inputField.Length <= 0)
        {
            if (lastUnfocusTime + 0.25f < Time.time)
            {
                usingChat = true;
                GUI.FocusWindow(5);
                GUI.FocusControl("Chat input field");
            }
        }

        window = GUI.Window(5, window, GlobalChatWindow, "");
    }


    void GlobalChatWindow(int id)
    {

        GUILayout.BeginVertical();
        GUILayout.Space(10);
        GUILayout.EndVertical();

        // Begin a scroll view. All rects are calculated automatically - 
        // it will use up any available screen space and make sure contents flow correctly.
        // This is kept small with the last two parameters to force scrollbars to appear.
        scrollPosition = GUILayout.BeginScrollView(scrollPosition);

        foreach (var entry in chatEntries)
        {
            GUILayout.BeginHorizontal();
            if ((entry as ChatEntry).name == "")
            {//Game message
                GUILayout.Label((entry as ChatEntry).text);
            }
            else
            {
                GUILayout.Label((entry as ChatEntry).name + ": " + (entry as ChatEntry).text);
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(3);

        }
        // End the scrollview we began above.
        GUILayout.EndScrollView();

        if (Event.current.type == EventType.keyDown && Event.current.character == '\n' && inputField.Length > 0)
        {
            HitEnter(inputField);
        }
        GUI.SetNextControlName("Chat input field");
        inputField = GUILayout.TextField(inputField);


        if (Input.GetKeyDown("mouse 0"))
        {
            if (usingChat)
            {
                usingChat = false;
                GUI.UnfocusWindow();//Deselect chat
                lastUnfocusTime = Time.time;
            }
        }
    }

    void HitEnter(string msg)
    {
        msg = msg.Replace("\n", "");
        networkView.RPC("ApplyGlobalChatText", RPCMode.All, playerName, msg);
        inputField = ""; //Clear line
        GUI.UnfocusWindow();//Deselect chat
        lastUnfocusTime = Time.time;
        usingChat = false;
    }


    [RPC]
    public void ApplyGlobalChatText(string name, string msg)
    {
        ChatEntry entry = new ChatEntry();
        entry.name = name;
        entry.text = msg;

        chatEntries.Add(entry);

        //Remove old entries
        if (chatEntries.Count > 4)
        {
            chatEntries.RemoveAt(0);
        }

        scrollPosition.y = 1000000;
    }

    //Add game messages etc
    public void addGameChatMessage(string str)
    {
        ApplyGlobalChatText("", str);
        if (Network.connections.Length > 0)
        {
            networkView.RPC("ApplyGlobalChatText", RPCMode.Others, "", str);
        }
    }
}