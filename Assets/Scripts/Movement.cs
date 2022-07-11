using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;

public class Movement : MonoBehaviour
{
    [HideInInspector]
    public PhotonView view;

    Camera cam;

    Controls controls;
    Rigidbody rb;

    public Vector3 camOffset = new Vector3(0f, 0f, 0f);

    float lookSpeed = 3f;
    float maxLookSpeed = 22f;
    Vector2 look;
    Vector3 rotation;
    public Vector2 smoothedRotation;

    [HideInInspector]
    public Vector3 gunRotation;
    [HideInInspector]
    public Vector3 smoothedGunRotation;

    Vector3 smoothedPos;

    float forward;
    float backward;
    float right;
    float left;

    Vector2 move;
    float moveSpeed = 4f;
    float airMoveSpeed = 1.5f;

    float recoilCameraMultiplier = 0.1f;

    [HideInInspector]
    public bool grounded;
    bool wantToJump;
    bool canJump;
    float jumpStrength = 5f;
    public RaycastHit hit;

    int counter = 0;
    bool crouching;
    bool wantToCrouch;
    bool canUnCrouch;
    float originalHeight;
    float crouchHeight = 0.5f;
    float crouchSpeedMultiplier = 0.5f;
    float crouchSmoothing = 2f;

    float sprintSpeedultiplier = 2f;
    float sprintSpeed;

    void Awake ()
    {
        originalHeight = GetComponent<CapsuleCollider>().height;
        sprintSpeed = sprintSpeedultiplier;
        smoothedRotation = Vector2.zero;

        controls = new Controls();

        controls.Gameplay.Forward.performed += ctx => forward = ctx.ReadValue<float>();
        controls.Gameplay.Forward.canceled += ctx => forward = 0f;

        controls.Gameplay.Backward.performed += ctx => backward = ctx.ReadValue<float>();
        controls.Gameplay.Backward.canceled += ctx => backward = 0f;

        controls.Gameplay.Right.performed += ctx => right = ctx.ReadValue<float>();
        controls.Gameplay.Right.canceled += ctx => right = 0f;

        controls.Gameplay.Left.performed += ctx => left = ctx.ReadValue<float>();
        controls.Gameplay.Left.canceled += ctx => left = 0f;

        controls.Gameplay.Look.performed += ctx => look = ctx.ReadValue<Vector2>();
        controls.Gameplay.Look.canceled += ctx => look = Vector2.zero;

        controls.Gameplay.Jump.performed += ctx => wantToJump = true;
        controls.Gameplay.Jump.canceled += ctx => wantToJump = false;

        controls.Gameplay.Crouch.performed += ctx => wantToCrouch = true;
        controls.Gameplay.Crouch.canceled += ctx => wantToCrouch = false;

        controls.Gameplay.Sprint.performed += ctx => StartSprint();
        controls.Gameplay.Sprint.canceled += ctx => EndSprint();
    }

    void Start ()
    {
        cam = transform.GetChild(0).GetComponent<Camera>();

        view = GetComponent<PhotonView>();

        if (!view.IsMine) cam.transform.gameObject.SetActive(false);

        rb = GetComponent<Rigidbody>();
        rotation = transform.eulerAngles;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        canJump = true;
    }

    void Update ()
    {
        if (view.IsMine)
        {
            Move();
            Look();
            if (wantToJump) Jump();
            if (wantToCrouch || counter != 0) { Crouch(); crouching = true; }
            if (!wantToCrouch && canUnCrouch) { UnCrouch(); crouching = false; }
        }

        GetComponent<SphereCollider>().center = new Vector3(0f, GetComponent<CapsuleCollider>().height * (1f - GetComponent<CapsuleCollider>().height / 2) + 0.1f, 0f);

        if (!view.IsMine && Physics.Raycast(transform.position, transform.TransformDirection(-Vector3.up), GetComponent<CapsuleCollider>().height / 2f + 2f)) GetComponent<Rigidbody>().useGravity = true;
        else if (!view.IsMine && !grounded) GetComponent<Rigidbody>().useGravity = false;
    }

    private void FixedUpdate ()
    {
        grounded = Physics.Raycast(transform.position, transform.TransformDirection(-Vector3.up), out hit, GetComponent<CapsuleCollider>().height / 2f + 0.2f);
        if (counter == 0) canUnCrouch = true;
        else if (counter != 0) canUnCrouch = false;
    }

    private void Move ()
    {
        move = new Vector2(right - left, forward - backward).normalized;
        if (grounded && !crouching) rb.velocity = transform.TransformDirection(new Vector3(move.x * moveSpeed * sprintSpeedultiplier, rb.velocity.y, move.y * moveSpeed * sprintSpeedultiplier));
        else if (grounded && crouching) rb.velocity = transform.TransformDirection(new Vector3(move.x * moveSpeed * crouchSpeedMultiplier, rb.velocity.y, move.y * moveSpeed * crouchSpeedMultiplier));
        else rb.AddForce(transform.TransformDirection(new Vector3(move.x * airMoveSpeed, 0f, move.y * airMoveSpeed * Mathf.Clamp(((30 - rb.velocity.magnitude)) / 30, 0, 1))));
    }

    public void RecoilCamera ()
    {
        float tempRecoilAmount;
        if (GetComponent<DownSights>().aiming) tempRecoilAmount = recoilCameraMultiplier / 2;
        else tempRecoilAmount = recoilCameraMultiplier;
        rotation.x += Random.Range(-recoilCameraMultiplier, recoilCameraMultiplier);
        rotation.y -= (GetComponent<GunManager>().gun.GetComponent<GunShoot>().recoilGunStrength / GetComponent<GunManager>().gun.GetComponent<GunShoot>().fireRate) * recoilCameraMultiplier * Random.Range(0.5f, 2f);
   
    }

    void Look () // Look rotation (UP down is Camera) (Left right is Transform rotation)
    {
        Vector3 oldRotation = rotation;
        
        //rotation = new Vector2(Mathf.Clamp((rotation.x - Mathf.Clamp(look.y, -maxLookSpeed, maxLookSpeed)), -30f, 30f), rotation.y + Mathf.Clamp(look.x, -maxLookSpeed, maxLookSpeed));
        rotation.x = rotation.x + Input.GetAxis("Mouse X");
        rotation.y = Mathf.Clamp( rotation.y - Input.GetAxis("Mouse Y"), -27, 27);

        rotation.z = rotation.x * 3; //just to role the gun when youre looking around

        gunRotation = rotation - oldRotation;
        smoothedGunRotation = Vector3.Lerp(smoothedGunRotation, gunRotation, 10f * Time.deltaTime); //this makes the gun look ahead of where your looking

        smoothedRotation = Vector3.Lerp(smoothedRotation, rotation, 800 * Time.deltaTime); //using a super high number to make it snappy

        transform.eulerAngles = new Vector2(0f, smoothedRotation.x) * lookSpeed;
        cam.transform.localRotation = Quaternion.Euler(smoothedRotation.y * lookSpeed, 0f, 0f);

        //smooth position of camera
        smoothedPos = Vector3.Lerp(smoothedPos, transform.position + camOffset, 20f * Time.deltaTime);
        cam.transform.position = smoothedPos + transform.up * (GetComponent<CapsuleCollider>().height / 3);
    }

    void Jump ()
    {
        if (grounded && canJump)
        {
            rb.velocity = new Vector3(rb.velocity.x, jumpStrength, rb.velocity.z);
            StartCoroutine(JumpTimer());
        }
    }

    void Crouch()
    {
        GetComponent<CapsuleCollider>().height = Mathf.Lerp(GetComponent<CapsuleCollider>().height, originalHeight * crouchHeight, crouchSmoothing * Time.deltaTime);

    }

    void UnCrouch()
    {
        GetComponent<CapsuleCollider>().height = Mathf.Lerp(GetComponent<CapsuleCollider>().height, originalHeight, crouchSmoothing * Time.deltaTime);
    }

    void StartSprint ()
    {
        sprintSpeedultiplier = 1f;
    }

    void EndSprint ()
    {
        sprintSpeedultiplier = sprintSpeed;
    }

    IEnumerator JumpTimer ()
    {
        yield return new WaitForSeconds(0.1f);
        canJump = true;
    }

    private void OnTriggerStay(Collider other)
    {
        counter++;
        StartCoroutine(RiseTimer());
    }

    private IEnumerator RiseTimer ()
    {
        int currentCounter = counter;
        yield return new WaitForSeconds(0.05f);
        if (counter == currentCounter) counter = 0;
    }

    private void OnEnable () { controls.Gameplay.Enable(); }
    private void OnDisable () { controls.Gameplay.Disable(); }
}
