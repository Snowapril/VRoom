using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class VRPlayerMove : MonoBehaviour
{
    public XRNode inputSource;
    public float speed = 1f;
    private Vector2 inputAxis;
    private CharacterController character;
    public Transform head;
    

    // Start is called before the first frame update
    void Start()
    {
        character = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        InputDevice device =InputDevices.GetDeviceAtXRNode(inputSource);
        device.TryGetFeatureValue(CommonUsages.primary2DAxis, out inputAxis);

    }

    private void FixedUpdate()
    {
        Vector3 direction = new Vector3(inputAxis.x, 0, inputAxis.y);
        Quaternion headYaw = Quaternion.Euler(0, head.eulerAngles.y, 0);
        direction = headYaw * direction.normalized;

        //character.Move(direction * Time.fixedDeltaTime);
        character.Move(direction * (Time.fixedDeltaTime * speed));
    }
}
