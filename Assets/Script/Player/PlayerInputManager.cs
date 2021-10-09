using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebXR;


[RequireComponent (typeof(PlayerManager))]
public class PlayerInputManager : MonoBehaviour
{

    #region Private Value
    
    private PlayerManager player;
    private int leftControllerState = 0;

    private Vector2 rightMoveVectorInput, leftMoveVectorInput;
    #endregion
    // Start is called before the first frame update
    void Start()
    {
        player = transform.GetComponent<PlayerManager>();
    }

    // Update is called once per frame
    void Update()
    {
        player.AddPlayerMoveVector(GetPlayerMoveInput());
    }

    private Vector3 GetPlayerMoveInput()
    {
        rightMoveVectorInput = VRoomInputSystem.GetAxis2D(WebXRControllerHand.RIGHT);
        leftMoveVectorInput = VRoomInputSystem.GetAxis2D(WebXRControllerHand.LEFT);

        //left joystick input
        if (Vector2.Dot(leftMoveVectorInput, Vector2.left) > 0.7f)
        {
            if (leftControllerState != 1)
            {
                transform.Rotate(0f, -30f, 0f);
                //Player Rotation으로 바꿔야함
                
                leftControllerState = 1;
            }

            leftMoveVectorInput = Vector2.zero;
        }
        else if (Vector2.Dot(leftMoveVectorInput, Vector2.right) > 0.7f)
        {
            if (leftControllerState != 2)
            {
                transform.Rotate(0f, 30f, 0f);
                //Player Rotation으로 바꿔야함
                
                leftControllerState = 2;
            }

            leftMoveVectorInput = Vector2.zero;
        }
        else
        {
            leftControllerState = 0;
            if (leftMoveVectorInput.y > 0)
            {
                leftMoveVectorInput = Vector2.up * Vector2.Dot(Vector2.up, leftMoveVectorInput);
            }
            else
            {
                leftMoveVectorInput = Vector2.down * Vector2.Dot(Vector2.down, leftMoveVectorInput);
            }
        }

        return new Vector3(rightMoveVectorInput.x + leftMoveVectorInput.x, 0f,
            rightMoveVectorInput.y + leftMoveVectorInput.y);
    }
    
}
