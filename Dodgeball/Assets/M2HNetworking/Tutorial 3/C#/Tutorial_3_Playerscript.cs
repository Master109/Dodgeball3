using UnityEngine;
using System.Collections;

public class Tutorial_3_Playerscript : MonoBehaviour
{
    /*  This file is part of the "Ultimate Unity networking project" by M2H (http://www.M2H.nl)
     *  This project is available on the Unity Store. You are only allowed to use these
     *  resources if you've bought them from the Unity Assets Store.
     */
    public NetworkPlayer owner;

    //Last input value, we're saving this to be able to save network messages/bandwidth.
    private float lastClientHInput = 0;
    private float lastClientVInput = 0;

    //The input values the server will execute on this object
    private float serverCurrentHInput = 0;
    private float serverCurrentVInput = 0;
    void Awake()
    {
        if (Network.isClient)
        {
            // We are probably not the owner of this object: disable this script.
            // RPC's and OnSerializeNetworkView will STILL get trough!
            // The server ALWAYS run this script though
            enabled = false;	 // disable this script (this disables Update());	
        }
    }
    [RPC]
    void SetPlayer(NetworkPlayer player)
    {
        owner = player;
        if (player == Network.player)
        {
            //Hey thats us! We can control this player: enable this script (this enables Update());
            enabled = true;
        }
    }

    void Update()
    {

        //Client code
        if (Network.player == owner)
        {
            //Only the client that owns this object executes this code
            float HInput = Input.GetAxis("Horizontal");
            float VInput = Input.GetAxis("Vertical");

            //Is our input different? Do we need to update the server?
            if (lastClientHInput != HInput || lastClientVInput != VInput)
            {
                lastClientHInput = HInput;
                lastClientVInput = VInput;

                if (Network.isServer)
                {
                    //Too bad a server can't send an rpc to itself using "RPCMode.Server"!
                    //This is a Unity "feature", see `Tips`
                    SendMovementInput(HInput, VInput);
                }
                else if (Network.isClient)
                {
                    //SendMovementInput(HInput, VInput); //Use this (and line 64) for simple "prediction"
                    networkView.RPC("SendMovementInput", RPCMode.Server, HInput, VInput);
                }

            }
        }

        //Server movement code
        if (Network.isServer)
        {//To also enable this on the client itself, use: "|| Network.player==owner){|"
            //Actually move the player using his/her input
            Vector3 moveDirection = new Vector3(serverCurrentHInput, 0, serverCurrentVInput);
            float speed = 5;
            transform.Translate(speed * moveDirection * Time.deltaTime);
        }

    }


    [RPC]
    void SendMovementInput(float HInput, float VInput)
    {
        //Called on the server
        serverCurrentHInput = HInput;
        serverCurrentVInput = VInput;
    }
    void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
    {
        if (stream.isWriting)
        {
            //This is executed on the owner of the networkview
            //The owner sends it's position over the network

            Vector3 pos = transform.position;
            stream.Serialize(ref pos);//"Encode" it, and send it

        }
        else
        {
            //Executed on all non-owners
            //receive a position and set the object to it

            Vector3 posReceive = Vector3.zero;
            stream.Serialize(ref posReceive); //"Decode" it and receive it

            //We've just recieved the current servers position of this object in 'posReceive'.

            transform.position = posReceive;
            //To reduce laggy movement a bit you could comment the line above and use position lerping below instead:	
            //transform.position = Vector3.Lerp(transform.position, posReceive, 0.9f); //"lerp" to the posReceive by 90%
            //It would be even better to save the last received server position and lerp to it in Update because it is executed more often than OnSerializeNetworkView

        }
    }
}