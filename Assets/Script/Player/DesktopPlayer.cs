using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DesktopPlayer : Player
{
    [SerializeField] private DesktopChracterInfo characterModel;
    [SerializeField] private float mouseTurnSpeed = 4.0f;
    [SerializeField] private PhotonView photonView;
    
    float xRotate = 0f;
    public override void PlayerInit()
    {
        base.PlayerInit();
    }
    void Start()
    {
        if (!photonView.IsMine && PhotonNetwork.IsConnected)
            return;
        PlayerInit();
        Camera.main.transform.parent = characterModel.GetCameraTransform();
        Camera.main.transform.localPosition = Vector3.zero;
        Camera.main.transform.localEulerAngles = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void LateUpdate()
    {
        if (!photonView.IsMine && PhotonNetwork.IsConnected)
            return;
        MouseRotation();
        PlayerMove();
        currentPlayerMoveVector = Vector3.zero;
    }

    #region Player Move Function

    
    //Desktop Player의 마우스의 움직임을 인식하여 캐릭터와 카메라의 회전 방향을 조절하는 함수
    private void MouseRotation()
    {
        // 좌우로 움직인 마우스의 이동량 * 속도에 따라 카메라가 좌우로 회전할 양 계산
        float yRotateSize = Input.GetAxis("Mouse X") * mouseTurnSpeed;
        // 현재 y축 회전값에 더한 새로운 회전각도 계산
        float yRotate = characterModel.transform.eulerAngles.y + yRotateSize;

        // 위아래로 움직인 마우스의 이동량 * 속도에 따라 카메라가 회전할 양 계산(하늘, 바닥을 바라보는 동작)
        float xRotateSize = -Input.GetAxis("Mouse Y") * mouseTurnSpeed;
        // 위아래 회전량을 더해주지만 -45도 ~ 80도로 제한 (-45:하늘방향, 80:바닥방향)
        // Clamp 는 값의 범위를 제한하는 함수
        xRotate = Mathf.Clamp(xRotate + xRotateSize, -50, 35);
    
        // 카메라 회전량을 카메라에 반영(X, Y축만 회전)
        characterModel.transform.eulerAngles = new Vector3(0, yRotate, 0);
        characterModel.GetCameraTransform().localEulerAngles = new Vector3(xRotate, 0f, 0f);

    }


    private void PlayerMove()
    {
        Quaternion headYaw = Quaternion.Euler(0, characterModel.transform.eulerAngles.y, 0);
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
