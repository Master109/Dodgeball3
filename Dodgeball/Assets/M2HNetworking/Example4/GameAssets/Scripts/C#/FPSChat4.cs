using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FPSChat4 : MonoBehaviour
{
    /*  This file is part of the "Ultimate Unity networking project" by M2H (http://www.M2H.nl)
     *  This project is available on the Unity Store. You are only allowed to use these
     *  resources if you've bought them from the Unity Assets Store.
     */

    public static bool usingChat = false;	//Can be used to determine if we need to stop player movement since we're chatting
    public GUISkin skin;						//Skin
    public bool showChat = false;			//Show/Hide the chat

    //Private vars used by the script
    private string inputField = "";

    private Vector2 scrollPosition;
    private int width = 500;
    private int height = 180;
    private string playerName;
    private float lastUnfocus = 0;
    private Rect window;

    private List<FPSChatEntry> chatEntries = new List<FPSChatEntry>();
    public class FPSChatEntry
    {
        public string name = "";
        public string text = "";
    }
    void Awake()
    {
        usingChat = false;

        window = new Rect(Screen.width / 2 - width / 2, Screen.height - height + 5, width, height);
    }

    public void CloseChatWindow()
    {
        showChat = false;
        inputField = "";
        chatEntries = new List<FPSChatEntry>();
    }

    public void ShowChatWindow()
    {
        playerName = PlayerPrefs.GetString("playerName", "");
        if (playerName == "")
        {
            playerName = "RandomName" + Random.Range(1, 999);
        }

        showChat = true;
        inputField = "";
        chatEntries = new List<FPSChatEntry>();
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
            if (lastUnfocus + 0.25f < Time.time)
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

        foreach (FPSChatEntry entry in chatEntries as List<FPSChatEntry>)
        {
            GUILayout.BeginHorizontal();
            if (entry.name == "")
            {//Game message
                GUILayout.Label(entry.text);
            }
            else
            {
                GUILayout.Label(entry.name + ": " + entry.text);
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
                lastUnfocus = Time.time;
            }
        }
    }

    void HitEnter(string msg)
    {
        msg = msg.Replace("\n", "");
        networkView.RPC("ApplyGlobalChatText", RPCMode.All, playerName, msg);
        inputField = ""; //Clear line
        GUI.UnfocusWindow();//Deselect chat
        lastUnfocus = Time.time;
        usingChat = false;
    }
    [RPC]
    public void ApplyGlobalChatText(string name, string msg)
    {
        FPSChatEntry entry = new FPSChatEntry();
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
