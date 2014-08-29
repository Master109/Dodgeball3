using UnityEngine;
using System.Collections;

public class Tutorial_2B_Playerscript : MonoBehaviour
{
    /*  This file is part of the "Ultimate Unity networking project" by M2H (http://www.M2H.nl)
     *  This project is available on the Unity Store. You are only allowed to use these
     *  resources if you've bought them from the Unity Assets Store.
     */

    void Awake()
    {
        if (!networkView.isMine)
        {
            //We aren't the networkView owner, disable this script
            //RPC's and OnSerializeNetworkView will STILL get trough but we prevent Update from running
            enabled = false;
        }
    }

    void Update()
    {

        if (networkView.isMine)
        {
            //Only the owner can move the cube!	
            //(Ok this check is a bit overkill since we did already disable the script in Awake)		
            Vector3 moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            float speed = 5;
            transform.Translate(speed * moveDirection * Time.deltaTime);
        }

    }
    void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
    {
        if (stream.isWriting)
        {
            //Executed on the owner of this networkview; 
            //The server sends it's position over the network

            Vector3 pos = transform.position;
            stream.Serialize(ref pos);//"Encode" it, and send it

        }
        else
        {
            //Executed on the others; 
            //receive a position and set the object to it

            Vector3 posReceive = Vector3.zero;
            stream.Serialize(ref posReceive); //"Decode" it and receive it
            transform.position = posReceive;

        }
    }
}
