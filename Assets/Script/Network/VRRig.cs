using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;


[System.Serializable]
public class VRMap
{
    public Transform vrTarget;
    public Transform rigTarget;
    public Vector3 trackingPositionOffset;
    public Vector3 trackingRotationOffset;
    
    public void Map()
    {
        if (vrTarget == null || rigTarget == null)
            return;
        rigTarget.position = vrTarget.TransformPoint(trackingPositionOffset);
        rigTarget.rotation = vrTarget.rotation * Quaternion.Euler(trackingRotationOffset);
    }
    
    
}

public class VRRig : MonoBehaviourPunCallbacks
{
    public VRMap head;
    public VRMap leftHand;
    public VRMap rightHand;
    
    public Transform headConstraint;

    public Vector3 headBodyOffest;

    public Vector3 headBodyRotationOffset;

    private bool isPlayerSet = false;
    private PhotonView photonView;

        public float turnSmoothness = 1f;
    // Start is called before the first frame update
    void Start()
    {
        headBodyOffest = transform.position - headConstraint.position;
        headBodyRotationOffset = transform.forward - Vector3.ProjectOnPlane(headConstraint.up, Vector3.up).normalized;
        photonView = transform.GetComponent<PhotonView>();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (!isPlayerSet)
        {
            foreach (var userRoot in GameObject.FindGameObjectsWithTag("UserRoot"))
            {
                PhotonView rootPhotonView = userRoot.GetComponent<PhotonView>();
                if (rootPhotonView.Owner.UserId == photonView.Owner.UserId)
                {
                    this.head.vrTarget = userRoot.transform.GetChild(0);
                    this.rightHand.vrTarget = userRoot.transform.GetChild(1);
                    this.leftHand.vrTarget = userRoot.transform.GetChild(2);
                    transform.SetParent(userRoot.transform);
                    isPlayerSet = true;
                    
                    break;
                }
            }
            return;
        }
        
        transform.position = headConstraint.position + headBodyOffest;
        
        transform.forward = Vector3.Lerp(transform.forward,
            Vector3.ProjectOnPlane(headConstraint.forward, Vector3.up).normalized,
            Time.deltaTime * turnSmoothness);
        
        //transform.forward = Vector3.ProjectOnPlane(headConstraint.up, Vector3.up).normalized;
            
            //Vector3.ProjectOnPlane(headConstraint.up, Vector3.up).normalized + headBodyRotationOffset;
        head.Map();
        rightHand.Map();
        leftHand.Map();
    }
}
