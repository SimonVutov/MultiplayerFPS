using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Gun : MonoBehaviourPunCallbacks //THIS SCRIPT WAS FOR LEARNING HOW IT WORKS, delete if you want to
{
    public Transform gunTransform;
    public ParticleSystem ps;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine)
        {
            if (Input.GetMouseButtonDown(0))
            {
                //shoot
                photonView.RPC("RPC_Shoot", RpcTarget.All);
            }
        }
    }

    [PunRPC]
    void RPC_Shoot ()
    {
        ps.Play();
    }
}
