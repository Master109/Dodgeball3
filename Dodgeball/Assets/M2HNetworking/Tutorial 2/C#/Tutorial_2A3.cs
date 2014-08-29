using UnityEngine;
using System.Collections;

public class Tutorial_2A3 : MonoBehaviour
{
    /*  This file is part of the "Ultimate Unity networking project" by M2H (http://www.M2H.nl)
     *  This project is available on the Unity Store. You are only allowed to use these
     *  resources if you've bought them from the Unity Assets Store.
     */


    private Vector3 lastPosition;

    void Update()
    {

        if (Network.isServer)
        {
            //Only the server can move the cube!			
            Vector3 moveDirection = new Vector3(-1 * Input.GetAxis("Vertical"), 0, Input.GetAxis("Horizontal"));
            float speed = 5;
            transform.Translate(speed * moveDirection * Time.deltaTime);

            //Save some network bandwidth; only send an rpc when the position has moved more than X
            if (Vector3.Distance(transform.position, lastPosition) >= 0.05f)
            {
                lastPosition = transform.position;

                //Send the position Vector3 over to the others; in this case all clients
                networkView.RPC("SetPosition", RPCMode.Others, transform.position);
            }
        }

    }
    [RPC]
    void SetPosition(Vector3 newPos)
    {
        //In this case, this function is always ran on the Clients
        //The server requested all clients to run this function (line 25).

        transform.position = newPos;
    }
}

