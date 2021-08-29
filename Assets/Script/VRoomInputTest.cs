using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WebXR;

//Class for test Input System
public class VRoomInputTest : MonoBehaviour
{
    //Text for Check Input State
    [SerializeField] private Text text;
    void Update()
    {
        //Button Test
        text.text = "right hand grip Button : " + VRoomInputSystem.GetButton(WebXRController.ButtonTypes.Grip) +
                    "\nright hand thumbstick Button : " + VRoomInputSystem.GetButton(WebXRController.ButtonTypes.Thumbstick) +
                    "\nright hand Trigger Button : " + VRoomInputSystem.GetButton(WebXRController.ButtonTypes.Trigger) +
                    "\nright hand ButtonA Button : " + VRoomInputSystem.GetButton(WebXRController.ButtonTypes.ButtonA) +
                    "\nright hand ButtonB Button : " + VRoomInputSystem.GetButton(WebXRController.ButtonTypes.ButtonB) +
                    "\nleft hand grip Button : " + VRoomInputSystem.GetButton(WebXRController.ButtonTypes.Grip, WebXRControllerHand.LEFT) +
                    "\nleft hand thumbstick Button : " + VRoomInputSystem.GetButton(WebXRController.ButtonTypes.Thumbstick, WebXRControllerHand.LEFT) +
                    "\nleft hand Trigger Button : " + VRoomInputSystem.GetButton(WebXRController.ButtonTypes.Trigger, WebXRControllerHand.LEFT) +
                    "\nleft hand ButtonA Button : " + VRoomInputSystem.GetButton(WebXRController.ButtonTypes.ButtonA, WebXRControllerHand.LEFT) +
                    "\nleft hand ButtonB Button : " + VRoomInputSystem.GetButton(WebXRController.ButtonTypes.ButtonB, WebXRControllerHand.LEFT);
        //Axis TEst
        text.text += "\nright Hand Axis : " + VRoomInputSystem.GetAxis2D(WebXRControllerHand.RIGHT) +
                     "\nleft Hand Axis : " + VRoomInputSystem.GetAxis2D(WebXRControllerHand.LEFT);
    }
}
