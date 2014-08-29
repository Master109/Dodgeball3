using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Tutorial_3_Spawnscript : MonoBehaviour
{
    /*  This file is part of the "Ultimate Unity networking project" by M2H (http://www.M2H.nl)
     *  This project is available on the Unity Store. You are only allowed to use these
     *  resources if you've bought them from the Unity Assets Store.
     */

    public Transform playerPrefab;
    public List<Tutorial_3_Playerscript> playerScripts = new List<Tutorial_3_Playerscript>();

    void OnServerInitialized()
    {
        //Spawn a player for the server itself
        Spawnplayer(Network.player);
    }

    void OnPlayerConnected(NetworkPlayer newPlayer)
    {
        //A player connected to me(the server), spawn a player for it:
        Spawnplayer(newPlayer);
    }


    void Spawnplayer(NetworkPlayer newPlayer)
    {
        //Called on the server only

        //Instantiate a new object for this player, remember; the server is therefore the owner.
        Transform myNewTrans = Network.Instantiate(playerPrefab, transform.position, transform.rotation, 0) as Transform;

        //Get the networkview of this new transform
        NetworkView newObjectsNetworkview = myNewTrans.networkView;

        Tutorial_3_Playerscript playerScript = myNewTrans.GetComponent<Tutorial_3_Playerscript>();
        //Keep track of this new player so we can properly destroy it when required.
        playerScripts.Add(playerScript);
        playerScript.owner = newPlayer;

        //Call an RPC on this new networkview, set the NetworkPlayer who controls this new player
        newObjectsNetworkview.RPC("SetPlayer", RPCMode.AllBuffered, newPlayer);//Set it on the owner
    }

    void OnPlayerDisconnected(NetworkPlayer player)
    {
        Debug.Log("Clean up after player " + player);

        for(int i =playerScripts.Count-1;i>=0;i--){
            Tutorial_3_Playerscript script = playerScripts[i];        
            if (player == script.owner)
            {//We found the players object
                Network.RemoveRPCs(script.gameObject.networkView.viewID);//remove the bufferd SetPlayer call
                Network.Destroy(script.gameObject);//Destroying the GO will destroy everything
                playerScripts.Remove(script);//Remove this player from the list
                break;
            }
        }

        //Remove the buffered RPC call for this player (SetPlayer, line 37)
        int playerNumber = int.Parse(player + "");
        Network.RemoveRPCs(Network.player, playerNumber);

        // The next destroys will not destroy anything since the players never
        // instantiated anything nor buffered RPCs
        Network.RemoveRPCs(player);
        Network.DestroyPlayerObjects(player);
    }

    void OnDisconnectedFromServer(NetworkDisconnection info)
    {
        Debug.Log("Resetting the scene the easy way.");
        Application.LoadLevel(Application.loadedLevel);
    }

}