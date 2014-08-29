using UnityEngine;
using System.Collections;

public class gameGUI : MonoBehaviour
{
    /*  This file is part of the "Ultimate Unity networking project" by M2H (http://www.M2H.nl)
     *  This project is available on the Unity Store. You are only allowed to use these
     *  resources if you've bought them from the Unity Assets Store.
     */

    public string gameName = "Example1";

    void Awake()
    {
        //RE-enable the network messages now we've loaded the right level
        Network.isMessageQueueRunning = true;

    }
    void OnGUI()
    {

        if (Network.peerType == NetworkPeerType.Disconnected)
        {
            //We are currently disconnected: Not a client or host
            GUILayout.Label("Connection status: We've (been) disconnected");
            if (GUILayout.Button("Back to main menu"))
            {
                Application.LoadLevel(Application.loadedLevel-1);
            }

        }
        else
        {
            //We've got a connection(s)!


            if (Network.peerType == NetworkPeerType.Connecting)
            {

                GUILayout.Label("Connection status: Connecting");

            }
            else if (Network.peerType == NetworkPeerType.Client)
            {

                GUILayout.Label("Connection status: Client!");
                GUILayout.Label("Ping to server: " + Network.GetAveragePing(Network.connections[0]));

            }
            else if (Network.peerType == NetworkPeerType.Server)
            {

                GUILayout.Label("Connection status: Server!");
                GUILayout.Label("Connections: " + Network.connections.Length);
                if (Network.connections.Length >= 1)
                {
                    GUILayout.Label("Ping to first player: " + Network.GetAveragePing(Network.connections[0]));
                }
            }

            if (GUILayout.Button("Disconnect"))
            {
                Network.Disconnect(200);
            }
        }


    }

    //CLient function
    void OnDisconnectedFromServer(NetworkDisconnection info)
    {
        Debug.Log("This CLIENT has disconnected from a server");
    }
    //Server functions called by Unity
    void OnPlayerConnected(NetworkPlayer player)
    {
        Debug.Log("Player connected from: " + player.ipAddress + ":" + player.port);
    }

    void OnPlayerDisconnected(NetworkPlayer player)
    {
        Debug.Log("Player disconnected from: " + player.ipAddress + ":" + player.port);

    }


}