using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using WebXR;


public class VRoomInputSystem : MonoBehaviour
{
    private static WebXRController rightHandController, leftHandController;
    private static bool isVR;
    
    private void Start()
    {
        foreach (WebXRController controller in GameObject.FindObjectsOfType<WebXRController>())
        {
            if (controller.hand == WebXRControllerHand.RIGHT)
                rightHandController = controller;
            
            if (controller.hand == WebXRControllerHand.LEFT)
                leftHandController = controller;
        }
        
        Debug.Log("Right Hand Controller : " + rightHandController.name);
        Debug.Log("Left Hand Controller : " + leftHandController.name);
    }

    //Function For Test
    public static void ChangIsVR()
    {
        isVR = !isVR;
    }
    
    public static void SetIsVR(bool _isVR)
    {
        isVR = _isVR;
    }
    
    //Return (Vector2) Desktop & VR Joystick Axis or Arrow keys Input
    public static Vector2 GetAxis2D(WebXRControllerHand handType = WebXRControllerHand.RIGHT)
    {
        switch (handType)
        {
            case WebXRControllerHand.RIGHT:
                if (isVR)
                {
                    return rightHandController.GetAxis2D(WebXRController.Axis2DTypes.Thumbstick);
                }
                else
                {
                    return new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")).normalized;
                }
                break;
            case WebXRControllerHand.LEFT:
                if (isVR)
                {
                    return leftHandController.GetAxis2D(WebXRController.Axis2DTypes.Thumbstick);
                }
                else
                {
                    return Vector2.zero;
                }
                break;
        }

        return Vector2.zero;
    }

    //Return (Vector2) Desktop Arrow keys Input
    public static Vector2 GetDesktopAxis2D()
    {
        return new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")).normalized;
    }
    
    //Return (Vector2) VR Joystick Input
    public static Vector2 GetVRAxis2D(WebXRControllerHand handType = WebXRControllerHand.RIGHT)
    {
        switch (handType)
        {
            case WebXRControllerHand.RIGHT:
                return rightHandController.GetAxis2D(WebXRController.Axis2DTypes.Thumbstick);
                break;
            case WebXRControllerHand.LEFT:
                return leftHandController.GetAxis2D(WebXRController.Axis2DTypes.Thumbstick);
                break;
        }

        return Vector2.zero;
    }
    
    //Return (bool) Desktop & VR Button Input
    public static bool GetButton(WebXRController.ButtonTypes buttonType, WebXRControllerHand handType = WebXRControllerHand.RIGHT)
    {
        switch (buttonType)
        {
            case WebXRController.ButtonTypes.Grip:
                if (isVR)
                {
                    switch (handType)
                    {
                        case WebXRControllerHand.RIGHT:
                            return rightHandController.GetButton(buttonType);
                            break;
                        case WebXRControllerHand.LEFT:
                            return leftHandController.GetButton(buttonType);
                            break;
                        default:
                            return false;
                            break;
                    }
                }
                else
                {
                    return Input.GetButton("Fire2");
                }
                break;
            case WebXRController.ButtonTypes.Thumbstick:
                if (isVR)
                {
                    switch (handType)
                    {
                        case WebXRControllerHand.RIGHT:
                            return rightHandController.GetButton(buttonType);
                            break;
                        case WebXRControllerHand.LEFT:
                            return leftHandController.GetButton(buttonType);
                            break;
                        default:
                            return false;
                            break;
                    }
                    
                }
                else
                {
                    return Input.GetButton("Fire3");
                }
                break;
            case WebXRController.ButtonTypes.Touchpad:
                if (isVR)
                {
                    switch (handType)
                    {
                        case WebXRControllerHand.RIGHT:
                            return rightHandController.GetButton(buttonType);
                            break;
                        case WebXRControllerHand.LEFT:
                            return leftHandController.GetButton(buttonType);
                            break;
                        default:
                            return false;
                            break;
                    }
                    
                }
                else
                {
                    return false;
                }
                break;
            case WebXRController.ButtonTypes.Trigger:
                if (isVR)
                {
                    switch (handType)
                    {
                        case WebXRControllerHand.RIGHT:
                            return rightHandController.GetButton(buttonType);
                            break;
                        case WebXRControllerHand.LEFT:
                            return leftHandController.GetButton(buttonType);
                            break;
                        default:
                            return false;
                            break;
                    }
                    
                }
                else
                {
                    return Input.GetButton("Fire1");
                }
                break;
            case WebXRController.ButtonTypes.ButtonA:
                if (isVR)
                {
                    switch (handType)
                    {
                        case WebXRControllerHand.RIGHT:
                            return rightHandController.GetButton(buttonType);
                            break;
                        case WebXRControllerHand.LEFT:
                            return leftHandController.GetButton(buttonType);
                            break;
                        default:
                            return false;
                            break;
                    }
                }
                else
                {
                    //Undecided
                }
                break;
            case WebXRController.ButtonTypes.ButtonB:
                if (isVR)
                {
                    switch (handType)
                    {
                        case WebXRControllerHand.RIGHT:
                            return rightHandController.GetButton(buttonType);
                            break;
                        case WebXRControllerHand.LEFT:
                            return leftHandController.GetButton(buttonType);
                            break;
                        default:
                            return false;
                            break;
                    }
                    
                }
                else
                {
                    return Input.GetKey(KeyCode.Escape);
                }
                break;
        }
        return false;
    }
    
    //Return (bool) Desktop Button Input
    public static bool GetDesktopButton(WebXRController.ButtonTypes buttonType, WebXRControllerHand handType = WebXRControllerHand.RIGHT)
    {
        switch (buttonType)
        {
            case WebXRController.ButtonTypes.Grip:
                return Input.GetButton("Fire2");
                break;
            case WebXRController.ButtonTypes.Thumbstick:
                return Input.GetButton("Fire3");
                break;
            case WebXRController.ButtonTypes.Touchpad:
                return false;
                break;
            case WebXRController.ButtonTypes.Trigger:
                return Input.GetButton("Fire1");
                break;
            case WebXRController.ButtonTypes.ButtonA:
                //Undecided
                break;
            case WebXRController.ButtonTypes.ButtonB:
                return Input.GetKey(KeyCode.Escape);
                break;
        }
        return false;
    }
    
    //Return (bool) VR Button Input
    public static bool GetVRButton(WebXRController.ButtonTypes buttonType, WebXRControllerHand handType = WebXRControllerHand.RIGHT)
    {
        switch (handType)
        {
            case WebXRControllerHand.RIGHT:
                return rightHandController.GetButton(buttonType);
                break;
            case WebXRControllerHand.LEFT:
                return leftHandController.GetButton(buttonType);
                break;
            default:
                return false;
                break;
        }
        return false;
    }
    
}

