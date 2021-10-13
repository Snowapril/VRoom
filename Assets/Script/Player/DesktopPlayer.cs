using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DesktopPlayer : Player
{
    [SerializeField] private Transform characterModel;
    
    
    public override void PlayerInit()
    {
        base.PlayerInit();
    }
    void Start()
    {
        PlayerInit();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void LateUpdate()
    {
        PlayerMove();
        currentPlayerMoveVector = Vector3.zero;
    }

    #region Player Move Function

    private void PlayerMove()
    {
        Quaternion headYaw = Quaternion.Euler(0, characterModel.eulerAngles.y, 0);
        characterController.Move((headYaw * currentPlayerMoveVector.normalized) * (Time.deltaTime * playerMoveSpeed));

        if (isGround())
        {
            currentFallingSpeed = 0f;
        }
        else
        {
            currentFallingSpeed += playerGravity * Time.deltaTime;
            characterController.Move(Vector3.up * (currentFallingSpeed * Time.deltaTime));
        }
    }
    
    bool isGround()
    {
        Vector3 rayStart = transform.TransformPoint(characterController.center);
        float rayLength = characterController.center.y + 0.01f;
        bool hasHit = Physics.SphereCast(rayStart, 0.01f, Vector3.down, out RaycastHit hitInfo, rayLength, groundLayer);
        return hasHit;
    }

    #endregion

}
