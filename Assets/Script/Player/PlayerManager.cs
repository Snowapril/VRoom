using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Must have Components to use this script
[RequireComponent (typeof(CharacterController))]
//[RequireComponent (typeof(Rigidbody))]
[RequireComponent (typeof(PlayerMoveManager))]
[RequireComponent (typeof(PlayerInputManager))]
public class PlayerManager : MonoBehaviour
{
    #region Enum Region
    
    private enum PlayerState
    {
        Idle, walk, Interacting, 
    }

    #endregion

    #region Serialize Private Value

    [NotNull] 
    [SerializeField] private Transform playerCamera;
    
    [SerializeField] private float playerMoveSpeed = 1f;
    #endregion
    
    #region Private Value
    
    private Vector3 tempPlayerMoveVector = Vector3.zero;
    //Move Vector by Controller/Keyboard Input
    private Vector3 currentPlayerMoveVector = Vector3.zero;
    
    //Components
    private CharacterController characterController;
    private PlayerMoveManager playerMoveManager;
    private PlayerInputManager playerInputManager;
    
    #endregion

    
    #region Public Value
    
    public LayerMask groundLayer;
    
    #endregion
    
    #region Public Lamda Function

    public void AddPlayerMoveVector(Vector3 addVector) => tempPlayerMoveVector += addVector;
    public Transform GetPlayerCamera() => playerCamera;

    #endregion
    
    // Start is called before the first frame update
    void Start()
    {
        characterController = transform.GetComponent<CharacterController>();
        playerMoveManager = transform.GetComponent<PlayerMoveManager>();
        playerInputManager = transform.GetComponent<PlayerInputManager>();
    }

    // Update is called once per frame
    void Update()
    {
        playerMoveManager.PlayerMove(currentPlayerMoveVector);
    }

    private void LateUpdate()
    {
        //Update Next Frame Player Move Vector
        currentPlayerMoveVector = tempPlayerMoveVector;
        tempPlayerMoveVector = Vector3.zero;
    }
    
}
