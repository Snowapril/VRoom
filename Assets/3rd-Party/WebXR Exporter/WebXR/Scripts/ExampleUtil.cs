using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

//Code For Replace XRDevice.isPresent
//Source : Unity Docs
internal static class ExampleUtil
{
    public static bool isPresent()
    {
        var xrDisplaySubsystems = new List<XRDisplaySubsystem>();
        SubsystemManager.GetInstances<XRDisplaySubsystem>(xrDisplaySubsystems);
        foreach (var xrDisplay in xrDisplaySubsystems)
        {
            if (xrDisplay.running)
            {
                return true;
            }
        }
        return false;
    }
}
public class CheckXRDisplay : MonoBehaviour
{
    void Awake()
    {
        Debug.Log("Do we have an Active Display? " + ExampleUtil.isPresent().ToString());
    }
}