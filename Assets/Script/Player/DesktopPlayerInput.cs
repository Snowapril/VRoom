using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(DesktopPlayer))]
public class DesktopPlayerInput : MonoBehaviour
{
    #region Private Variable
    private DesktopPlayer player;
    private Vector3 keyboardInput;
    #endregion
    
    // Start is called before the first frame update
    void Start()
    {
        player = transform.GetComponent<DesktopPlayer>();
    }

    // Update is called once per frame
    void Update()
    {
        GetPlayerMoveInput();
        player.AddPlayerMoveVector(keyboardInput);
    }

    private Vector3 GetPlayerMoveInput()
    {
        keyboardInput.x = Input.GetAxis("Horizontal");
        keyboardInput.y = 0;
        keyboardInput.z = Input.GetAxis("Vertical");
        return keyboardInput;
    }
}
