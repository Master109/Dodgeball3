using UnityEngine;
using System.Collections;

public class Tutorial_2B_Spawnscript : MonoBehaviour
{
    /*  This file is part of the "Ultimate Unity networking project" by M2H (http://www.M2H.nl)
     *  This project is available on the Unity Store. You are only allowed to use these
     *  resources if you've bought them from the Unity Assets Store.
     */

    public Transform playerPrefab;

    void OnServerInitialized()
    {
        Spawnplayer();
    }

    void OnConnectedToServer()
    {
        Spawnplayer();
    }

    void Spawnplayer()
    {
        Network.Instantiate(playerPrefab, transform.position, transform.rotation, 0);
    }


    void OnPlayerDisconnected(NetworkPlayer player)
    {
        Debug.Log("Clean up after player " + player);
        Network.RemoveRPCs(player);
        Network.DestroyPlayerObjects(player);
    }

    void OnDisconnectedFromServer(NetworkDisconnection info)
    {
        Debug.Log("Clean up a bit after server quit");
        Network.RemoveRPCs(Network.player);
        Network.DestroyPlayerObjects(Network.player);

        /* 
        * Note that we only remove our own objects, but we cannot remove the other players 
        * objects since we don't know what they are; we didn't keep track of them. 
        * In a game you would usually reload the level or load the main menu level anyway ;).
        * 
        * To reset the scene we'll just reload it:
        */
        Application.LoadLevel(Application.loadedLevel);
    }
}