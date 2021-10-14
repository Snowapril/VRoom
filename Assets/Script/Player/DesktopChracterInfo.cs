using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DesktopChracterInfo : MonoBehaviour
{
    [SerializeField] private Transform characterTransform;
    [SerializeField] private Transform cameraTransform;

    public Transform GetCharacterTransform() => characterTransform;
    public Transform GetCameraTransform() => cameraTransform;
}
