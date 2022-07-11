using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiplayerIsMine : MonoBehaviour
{
    [HideInInspector]
    public bool isOnOtherTeam = false; //gets modified by spawner, not sued anymore

    public GameObject ArmatureRenderer;
    public Material opositeTeamMat;
    public GameObject CanvasUI; //disables it if its not yours

    void Start()
    {
        if (GetComponent<Movement>().view.IsMine) {
            gameObject.layer = 2;
            ArmatureRenderer.GetComponent<SkinnedMeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;

        } else {
            gameObject.layer = 0;
            ArmatureRenderer.GetComponent<SkinnedMeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            GetComponent<Rigidbody>().drag = 1000;
            transform.GetChild(0).GetComponent<Camera>().enabled = false;
            transform.GetChild(0).gameObject.SetActive(true);
            CanvasUI.SetActive(false);
        }
    }

    private void Update()
    {
        if (isOnOtherTeam) //not used
        {
            //ArmatureRenderer.GetComponent<SkinnedMeshRenderer>().material = opositeTeamMat;
        }
    }
}
