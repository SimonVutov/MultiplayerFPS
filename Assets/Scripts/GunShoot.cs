using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class GunShoot : MonoBehaviourPunCallbacks
{
    PhotonView photonView;

    Target target;

    Controls controls;
    //public Animator animator;
    [HideInInspector]
    public int hitMarkerType;
    int damageDelt; //actual amount
    public int hitDamageAmount; //gets reset to 0 after (time)

    bool headShot = false;

    public bool shotGun = false;
    int amountOfShells = 10;
    private Vector3 shellOffset;

    [HideInInspector]
    public bool isBeingHeld = false;

    bool wantToShoot = false;

    public bool auto = false;
    public int damage = 10;
    private int defaultDamage;
    public float range = 100f;
    float impactForce = 4f;
    public float fireRate = 15f;
    public float recoilGunStrength = 80f; //how hard the gun pushes up and for how long
    private float defaultRecoilGunStrength;
    public float recoilMaxDist = 10f; //max distance the gun tries to face when recoiling
    public float recoilHandStrength = 3f; //how strong the player is holding the gun, stopping it from going up

    public float maxAmmo = 18f;
    private float currentAmmo;
    public float reloadTime = 0.5f;
    bool currentlyReloading = false;

    public string gunAmmoUI;

    private Vector3 velocity = Vector3.zero;

    [HideInInspector]
    public Camera fpsCam;
    public ParticleSystem muzzleFlash;
    public GameObject impactEffect;

    private float nextTimeToFire = 0f;

    bool shooting = false;

    float eulerXVelocit;
    float eulerYVelocit;
    float eulerZVelocit;
    Vector3 Velocit;

    private void Awake()
    {
        defaultDamage = damage;
        defaultRecoilGunStrength = recoilGunStrength;

        controls = new Controls();

        controls.Gameplay.Shoot.performed += ctx => wantToShoot = true;
        controls.Gameplay.Shoot.canceled += ctx => wantToShoot = false;

        controls.Gameplay.Reload.performed += ctx => WantToReload();
    }

    private void Start()
    {
        currentAmmo = maxAmmo;

        photonView = GetComponent<PhotonView>(); //every gun needs its own photonview, cant just use the player's
    }

    private void Update()
    {
        if (isBeingHeld) fpsCam = transform.root.GetChild(0).GetComponent<Camera>();

        //if (transform.parent != null) animator = transform.parent.GetComponent<Animator>();

        if (shotGun) damage = defaultDamage / amountOfShells * 2;
        else damage = defaultDamage;

        if (shotGun) recoilGunStrength = defaultRecoilGunStrength / amountOfShells;
        else recoilGunStrength = defaultRecoilGunStrength;

        //if (transform.root.GetComponent<Movement>().view.IsMine) isBeingHeld = true;
        if (transform.parent != null) isBeingHeld = true;
        else isBeingHeld = false;

        if (wantToShoot && isBeingHeld && Time.time >= nextTimeToFire && currentAmmo > 0f && !currentlyReloading && photonView.IsMine) //check if you are the person trying to shoots
        {
            currentAmmo--;
            if (!auto) wantToShoot = false;
            nextTimeToFire = Time.time + 1f / fireRate;
            if (!shotGun) Shoot(Vector3.zero);
            else ShotGunShoot();
            shooting = true;
            StartCoroutine(Recoil());
        }

        if (!currentlyReloading && currentAmmo <= 0f)
        {
            currentlyReloading = true;
            StartCoroutine(Reload());
        }

        float x = Mathf.SmoothDampAngle(transform.localEulerAngles.x, 0f, ref eulerXVelocit, 1f / recoilHandStrength);
        float y = Mathf.SmoothDampAngle(transform.localEulerAngles.y, 0f, ref eulerYVelocit, 1f / recoilHandStrength);
        float z = Mathf.SmoothDampAngle(transform.localEulerAngles.z, 0f, ref eulerZVelocit, 1f / recoilHandStrength);

        if (shooting)
        {
            transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(-recoilMaxDist, 0f, 0f), recoilGunStrength * Time.deltaTime);
            transform.localPosition = Vector3.Lerp(transform.localPosition, new Vector3(0f, 0f, -Mathf.Clamp(recoilMaxDist / 20f, 0, 0.3f)), recoilGunStrength * Time.deltaTime);
        }
        else if (isBeingHeld)
        {
            transform.localEulerAngles = new Vector3(x, y, z);
            transform.localPosition = Vector3.SmoothDamp(transform.localPosition, Vector3.zero, ref Velocit, 2f / recoilHandStrength);
        }

        if (isBeingHeld)
        {
            if (currentlyReloading) gunAmmoUI = "Reloading";
            else gunAmmoUI = currentAmmo.ToString();
        }
        else
        {
            gunAmmoUI = "E To Pickup";
        }
    }

    void Shoot(Vector3 offset)
    {
        StartCoroutine(HitMarker(1)); //shooting makes small marker
        //StartCoroutine(ShowDamageDelt()); //shows the amount of damage

        RaycastHit hit;
        if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward + offset, out hit, range))
        {
            target = hit.transform.GetComponent<Target>();

            if (target != null)
            {
                headShot = hit.point.y - target.transform.position.y > 1.4; //if the y value of the spot it hit is high enough, its a headshot

                if (headShot) DealDamage(damage * 2); //double damage for headshot
                else DealDamage(damage);

                if (target.health <= 0f) StartCoroutine(HitMarker(4)); //if the player dies, make the hitmarker number 4 (yellow)
                else if (headShot) StartCoroutine(HitMarker(3)); //red hitmarker for headshot
                else StartCoroutine(HitMarker(2)); //else white hit marker
            }

            if (hit.rigidbody != null) hit.rigidbody.AddForceAtPosition(transform.forward * impactForce * recoilGunStrength, hit.point);

            GameObject impactGO = PhotonNetwork.InstantiateRoomObject(impactEffect.name, hit.point, Quaternion.LookRotation(hit.normal));
            //impactGO.transform.parent = hit.transform;
            //Destroy(impactGO, 20f);
        }

        transform.root.GetComponent<Movement>().RecoilCamera();

        photonView.RPC("RPC_PlayMuzzleFlash", RpcTarget.All);
    }

    void DealDamage(int damageDealt)
    {
        target.TakeDamage(damageDealt);
    }

    [PunRPC]
    void RPC_PlayMuzzleFlash()
    {
        muzzleFlash.Play();
    }

    void ShotGunShoot()
    {
        for (int i = amountOfShells; i > 0; i--)
        {
            Vector2 direction = new Vector2(Random.Range(-50, 50) / 100f, Random.Range(-50, 50) / 100f);
            Vector2 Strength = direction * (Random.Range(-50, 50) / 100f);
            shellOffset = new Vector3(Strength.x, Strength.y, 1f);
            Vector3 sprayDir = fpsCam.transform.TransformVector(shellOffset);
            Shoot(sprayDir);
        }
    }

    private IEnumerator Recoil()
    {
        yield return new WaitForSeconds(recoilGunStrength / 3000f);
        shooting = false;
    }

    void WantToReload()
    {
        if (!currentlyReloading && currentAmmo != maxAmmo && !wantToShoot)
        {
            currentlyReloading = true;
            StartCoroutine(Reload());
        }
    }

    private IEnumerator Reload()
    {
        //animator.SetBool("Reloading", true);
        yield return new WaitForSeconds(reloadTime - 0.25f);
        //animator.SetBool("Reloading", false);
        yield return new WaitForSeconds(0.25f);
        currentAmmo = maxAmmo;
        currentlyReloading = false;
    }

    private IEnumerator HitMarker(int typeOfHit)
    {
        hitMarkerType = typeOfHit;
        yield return new WaitForSeconds(0.12f);
        hitMarkerType = 0;
    }

    //private IEnumerator ShowDamageDelt()
    //{
    //hitDamageAmount = 
    //}

    private void OnEnable() { controls.Gameplay.Enable(); }
    private void OnDisable() { controls.Gameplay.Disable(); }
}
