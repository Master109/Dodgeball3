using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FPSPlayerNode
{
    public string playerName;
    public NetworkPlayer networkPlayer;

    public int kills = 0;
    public int deaths = 0;

}
public class GameSetup : MonoBehaviour
{
    /*  This file is part of the "Ultimate Unity networking project" by M2H (http://www.M2H.nl)
     *  This project is available on the Unity Store. You are only allowed to use these
     *  resources if you've bought them from the Unity Assets Store.
     */

    public Transform playerPref;
    public FPSChat4 chatScript;
    public string playerName = "";

    //Server-only playerlist
    public List<FPSPlayerNode> playerList = new List<FPSPlayerNode>();

    void Awake()
    {
        playerName = PlayerPrefs.GetString("playerName");

        chatScript = GetComponent<FPSChat4>() as FPSChat4;
        Network.isMessageQueueRunning = true;
        Screen.lockCursor = true;

        if (Network.isServer)
        {

            MultiplayerFunctions.SP.RegisterHost(playerName + "s game", "No description");

            chatScript.ShowChatWindow();

            networkView.RPC("TellOurName", RPCMode.AllBuffered, playerName, Network.player);

            foreach (GameObject go in FindObjectsOfType(typeof(GameObject)) as GameObject[])
            {
                go.SendMessage("OnNetworkLoadedLevel", SendMessageOptions.DontRequireReceiver);
            }

        }
        else if (Network.isClient)
        {

            networkView.RPC("TellOurName", RPCMode.AllBuffered, playerName, Network.player);
            chatScript.ShowChatWindow();

            foreach (GameObject go in FindObjectsOfType(typeof(GameObject)) as GameObject[])
            {
                go.SendMessage("OnNetworkLoadedLevel", SendMessageOptions.DontRequireReceiver);
            }



        }
        else
        {
            //How did we even get here without connection?
            Screen.lockCursor = false;
            Application.LoadLevel((Application.loadedLevel - 1));
        }
    }
    //Server function
    void OnPlayerDisconnected(NetworkPlayer player)
    {
        Network.RemoveRPCs(player, 0);
        Network.DestroyPlayerObjects(player);

        //Remove player from the server list
        foreach (FPSPlayerNode entry in playerList as List<FPSPlayerNode>)
        {
            if (entry.networkPlayer == player)
            {
                chatScript.addGameChatMessage(entry.playerName + " disconnected from: " + player.ipAddress + ":" + player.port);
                playerList.Remove(entry);
                break;
            }
        }
    }

    //Server function
    void OnPlayerConnected(NetworkPlayer player)
    {
        chatScript.addGameChatMessage("Player connected from: " + player.ipAddress + ":" + player.port);
    }

    [RPC]
    //Sent by newly connected clients, recieved by server
    void TellOurName(string name, NetworkPlayer fromPlayer, NetworkMessageInfo info)
    {
        NetworkPlayer netPlayer = fromPlayer;
        if (netPlayer + "" == "-1")
        {
            //This hack is required to fix the local players networkplayer when the RPC is sent to itself.
            netPlayer = Network.player;
        }

        FPSPlayerNode newEntry = new FPSPlayerNode();
        newEntry.playerName = name;
        newEntry.networkPlayer = netPlayer;
        playerList.Add(newEntry);

        if (Network.isServer)
        {
            chatScript.addGameChatMessage(name + " joined the game");
        }
    }

    //Called via Awake()
    void OnNetworkLoadedLevel()
    {
        // Randomize starting location
        GameObject[] spawnpoints = GameObject.FindGameObjectsWithTag("Spawnpoint");
        Debug.Log("spawns: " + spawnpoints.Length);

        Transform spawnpoint = spawnpoints[Random.Range(0, spawnpoints.Length)].transform;
        Network.Instantiate(playerPref, spawnpoint.position, spawnpoint.rotation, 0);
    }
    void OnDisconnectedFromServer()
    {
        //Load main menu
        Screen.lockCursor = false;
        Application.LoadLevel((Application.loadedLevel - 1));
    }



}