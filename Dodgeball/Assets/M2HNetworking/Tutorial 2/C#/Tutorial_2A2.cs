using UnityEngine;
using System.Collections;

public class Tutorial_2A2 : MonoBehaviour
{
    /*  This file is part of the "Ultimate Unity networking project" by M2H (http://www.M2H.nl)
     *  This project is available on the Unity Store. You are only allowed to use these
     *  resources if you've bought them from the Unity Assets Store.
     */

    void Update()
    {

        if (Network.isServer)
        {
            //Only the server can move the cube!			
            Vector3 moveDirection = new Vector3(-1 * Input.GetAxis("Vertical"), 0, Input.GetAxis("Horizontal"));
            float speed = 5;
            transform.Translate(speed * moveDirection * Time.deltaTime);
        }

    }
    void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
    {
        if (stream.isWriting)
        {
            //Executed on the owner of the networkview; in this case the Server
            //The server sends it's position over the network

            Vector3 pos = transform.position;
            stream.Serialize(ref pos);//"Encode" it, and send it

            /*
            bool jumpBoolean= Input.GetButton ("Jump");
            stream.Serialize(ref jumpBoolean);
            */

        }
        else
        {
            //Executed on the others; in this case the Clients
            //The clients receive a position and use it

            Vector3 posReceive = Vector3.zero;
            stream.Serialize(ref posReceive); //"Decode" it and receive it
            transform.position = posReceive;

            /*
            bool jumpBoolean = false;
            stream.Serialize(ref jumpBoolean);
            if(jumpBoolean){
                Debug.Log(We are jumping");
            }
            */
        }
    }
}

