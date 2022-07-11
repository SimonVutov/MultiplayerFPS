using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Target : MonoBehaviourPunCallbacks//, IPunObservable
{
    public int health = 100;
    int startHealth;

    private void Start()
    {
        startHealth = health;
    }

    private void Update()
    {
        if (health <= 0f)
        {
            Die();
        }
    }

    public void TakeDamage (int amount)
    {
        photonView.RPC("RPC_TakeDamage", RpcTarget.All, amount);
    }

    [PunRPC]
    void RPC_TakeDamage(int amount)
    {
        health -= amount;
    }

    void Die ()
    {
        health = startHealth;
        transform.position = new Vector3(0, 10, 0);
    }
}
