using System;
using System.Collections;
using UnityEngine;
using UnityEngine.XR;
using Photon.Pun;
using System.Linq;

public class NetworkPlayer : MonoBehaviour
{
    private PhotonView photonView;
    public Transform head;
    public Transform rightHand;
    public Transform leftHand;

    private Transform headSource;
    private Transform rightHandSource;
    private Transform leftHandSource;
    // Start is called before the first frame update
    void Start()
    {
        photonView = GetComponent<PhotonView>();
    }
    
    // Update is called once per frame
    void FixedUpdate()
    {
        if (photonView.IsMine)
        {
            if (headSource == null)
            {
                headSource = GameObject.FindGameObjectsWithTag("PlayerHead")[0].transform;
            }
            if (rightHandSource == null)
            {
                rightHandSource = GameObject.FindGameObjectsWithTag("PlayerRightHand")[0].transform;
            }
            if (leftHandSource == null)
            {
                leftHandSource = GameObject.FindGameObjectsWithTag("PlayerLeftHand")[0].transform;
            }

            MapPosition(head,headSource);
            MapPosition(rightHand,rightHandSource);
            MapPosition(leftHand,leftHandSource);
        }
    }

    void MapPosition(Transform target, Transform source)
    {
        target.position = source.position;
        target.rotation = source.rotation;
    }

    void MapPosition(Transform target, XRNode node)
    {
        InputDevices.GetDeviceAtXRNode(node).TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 position);
        InputDevices.GetDeviceAtXRNode(node).TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion rotation);

        target.position = position;
        target.rotation = rotation;
    }
}
