using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent (typeof(PlayerManager))]
[RequireComponent (typeof(CharacterController))]
public class PlayerMoveManager : MonoBehaviour
{
    #region Serialize Private Value
    [SerializeField] private float playerMoveSpeed = 1f;
    [SerializeField] private float playerGravity = -9.8f;
    [SerializeField] private LayerMask groundLayer;
    #endregion
    
    #region Private Value
    
    private CharacterController characterController;
    private float additionalPlayerHeight = 0f;
    private float currentFallingSpeed;
    private PlayerManager player;
    private Transform playerCamera;
    
    #endregion

    
    #region Public Value
    
    #endregion
    
    #region Public Lamda Function

    #endregion
    
    // Start is called before the first frame update
    void Start()
    {
        player = transform.GetComponent<PlayerManager>();
        playerCamera = player.GetPlayerCamera();
        characterController = transform.GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayerMove(Vector3 playerMoveVector)
    {
        Quaternion headYaw = Quaternion.Euler(0, playerCamera.eulerAngles.y, 0);
        playerMoveVector = headYaw * playerMoveVector.normalized;
        
        characterController.Move(playerMoveVector * (Time.fixedDeltaTime * playerMoveSpeed));
        
        //gravity
        if (isGround())
            currentFallingSpeed = 0;
        else
        {
            currentFallingSpeed += playerGravity * Time.fixedDeltaTime;
            characterController.Move(Vector3.up * (currentFallingSpeed * Time.fixedDeltaTime));
        }
        
    }
    
    void CharacterCapsuleHeightAddjust()
    {
        characterController.height = playerCamera.localPosition.y + additionalPlayerHeight;
        Vector3 capsuleCenter = transform.InverseTransformPoint(playerCamera.position);
        capsuleCenter.y = -(characterController.height/2 + characterController.skinWidth);
        characterController.center = capsuleCenter;
    }
    
    bool isGround()
    {
        Vector3 rayStart = transform.TransformPoint(characterController.center);
        float rayLength = characterController.center.y + 0.01f;
        bool hasHit = Physics.SphereCast(rayStart, characterController.radius, Vector3.down, out RaycastHit hitInfo, rayLength, groundLayer);
        return hasHit;
    }
    
}
