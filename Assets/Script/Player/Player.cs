using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;


//Must have Components to use this script
[RequireComponent (typeof(CharacterController))]
public class Player : MonoBehaviourPun
{
    #region Serialize Protected Variable
    
    [SerializeField] protected float playerMoveSpeed = 1f;
    [SerializeField] protected float playerGravity = -9.8f;
    [SerializeField] protected LayerMask groundLayer;
    
    #endregion
    
    #region Protected Variable
    
    protected CharacterController characterController;
    //Move Vector by Controller/Keyboard Input
    protected Vector3 currentPlayerMoveVector = Vector3.zero;
    protected float currentFallingSpeed;
    
    #endregion
    
    public void AddPlayerMoveVector(Vector3 addVector) => currentPlayerMoveVector += addVector;
    
    virtual public void PlayerInit()
    {
        characterController = transform.GetComponent<CharacterController>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void LateUpdate()
    {
        //Player Move
        
    }
}
