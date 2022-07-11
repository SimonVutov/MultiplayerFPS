using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using Photon.Pun;
using Photon.Realtime;

public class GunManager : MonoBehaviourPunCallbacks
{
    public GameObject rig;

    PhotonView photonView;

    float gunRotationAheadStrength = 2f;

    public GameObject[] hitMarkerUI;

    Vector3 handOffset;

    Controls controls;

    [HideInInspector]
    public GameObject gun;

    public bool holdingGun = false;

    public float maxPickupDistance = 4f;
    public GameObject gunHolder;

    void Awake()
    {
        handOffset = gunHolder.transform.GetChild(0).transform.position;

        photonView = GetComponent<PhotonView>();

        controls = new Controls();
        if (photonView.IsMine)
        {
            controls.Gameplay.Pickup.performed += ctx => WantToPickup();
            controls.Gameplay.Drop.performed += ctx => WantToDrop();
        }

        rig.GetComponent<TwoBoneIKConstraint>().weight = 0f;   
    }

    private void Update()
    {
        if (holdingGun)
        {
            gunHolder.transform.GetChild(0).transform.position = gun.transform.position;// + handOffset;
            gunHolder.transform.localEulerAngles = new Vector3(GetComponent<Movement>().smoothedGunRotation.y, GetComponent<Movement>().smoothedGunRotation.x, GetComponent<Movement>().smoothedGunRotation.z) * gunRotationAheadStrength;

            if (gun.GetComponent<GunShoot>().hitMarkerType != 0 && holdingGun) hitMarkerUI[gun.GetComponent<GunShoot>().hitMarkerType - 1].SetActive(true);
            else
            {
                foreach (GameObject go in hitMarkerUI)
                {
                    go.SetActive(false);
                }
            }
        }
    }

    void WantToPickup() //so it does it on all instances
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.root.GetChild(0).position, transform.root.GetChild(0).forward, out hit, maxPickupDistance));
        {//camera is the first thing on person child list
            if (hit.transform.tag == "Gun" && !holdingGun)
            {
                photonView.RPC("Pickup", RpcTarget.All, hit.transform.root.transform.gameObject.name);
            }
            else if (hit.transform.tag == "Gun")
            {
                WantToDrop();
                WantToPickup();
            }
        }
    }

    void WantToDrop() //so it does it on all instances
    {
        photonView.RPC("Drop", RpcTarget.All);
    }
    
    [PunRPC] //RPC_Pickup
    void Pickup(string gunToPickup)
    {
        gun = GameObject.Find(gunToPickup);

        gun.GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.LocalPlayer.ActorNumber);

        holdingGun = true;

        foreach (var c in gun.transform.GetComponentsInChildren<Collider>()) c.enabled = false;
            
        gun.transform.parent = gunHolder.transform;
        gun.transform.localPosition = Vector3.zero;
        gun.transform.localEulerAngles = Vector3.zero;
        rig.GetComponent<TwoBoneIKConstraint>().weight = 1f;
    }

    [PunRPC] //RPC_Drop
    void Drop()
    {
        if (holdingGun) {
            RaycastHit hitDown;
            Physics.Raycast(transform.position, -transform.up, out hitDown);
            if (holdingGun) gun.transform.position = hitDown.point;
            gun.transform.parent = null;
            gun.transform.eulerAngles = new Vector3(0f, gun.transform.eulerAngles.y, 0f);
            foreach (var c in gun.transform.GetComponentsInChildren<Collider>()) c.enabled = true;

            rig.GetComponent<TwoBoneIKConstraint>().weight = 0f;

            gunHolder.transform.GetChild(0).transform.position = Vector3.zero;

            holdingGun = false;

            gun.GetComponent<GunShoot>().hitMarkerType = 0;
        }
    }

    private void OnEnable() { controls.Gameplay.Enable(); }
    private void OnDisable() { controls.Gameplay.Disable(); }
}
