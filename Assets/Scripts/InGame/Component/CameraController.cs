using Common;
using Cysharp.Threading.Tasks;
using Generated.Table;
using InGame.Model;
using Unity.Cinemachine;
using UnityEngine;

namespace InGame.Component
{
    public class CameraController : MonoBehaviour
    {
        private InGameModel _inGameModel;
        private CinemachineCamera _cinemachineCamera;
        private GameObject _playerSight;
        
        public async UniTask Init(InGameModel inGameModel)
        {
            _inGameModel = inGameModel;
            
            _playerSight = new GameObject("PlayerSight");
            _playerSight.transform.SetParent(transform);
            _playerSight.transform.localPosition = GameDefine.DefaultPlayerSight;
            _playerSight.transform.localRotation = Quaternion.identity;
            
            await UniTask.CompletedTask;
        }

        public void AttachCamera(CinemachineCamera cinemachineCamera)
        {
            _cinemachineCamera = cinemachineCamera;
            _cinemachineCamera.Follow = _playerSight.transform;
        }

        public void DetachCamera()
        {
            _cinemachineCamera.Follow = null;
        }
    }
}
