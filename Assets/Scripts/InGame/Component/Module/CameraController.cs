using Common;
using Cysharp.Threading.Tasks;
using InGame.Component.Hub;
using InGame.Model;
using Unity.Cinemachine;
using UnityEngine;

namespace InGame.Component.Module
{
    public class CameraController : MonoBehaviour
    {
        private InGameModel _inGameModel;
        private InputHub _inputHub;
        private CinemachineCamera _cinemachineCamera;
        private GameObject _playerSight;
        private float _pitch;
        private float _yaw;

        public async UniTask Init(InGameModel inGameModel, InputHub inputHub)
        {
            _inGameModel = inGameModel;
            _inputHub = inputHub;
            _inputHub.OnDrag += OnDrag;

            _playerSight = new GameObject("PlayerSight");
            _playerSight.transform.SetParent(transform);
            _playerSight.transform.localPosition = GameDefine.DefaultPlayerSight;
            _playerSight.transform.localRotation = Quaternion.identity;
            await UniTask.CompletedTask;
        }

        private void OnDrag(Vector2 drag)
        {
            _yaw += drag.x * GameDefine.DefaultDragRotationSensitivity;
            _pitch -= drag.y * GameDefine.DefaultDragRotationSensitivity;
            _pitch = Mathf.Clamp(_pitch, GameDefine.DefaultCameraPitchMin, GameDefine.DefaultCameraPitchMax);
            _playerSight.transform.localRotation = Quaternion.Euler(_pitch, _yaw, 0f);
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

        public Vector3 GetForward() => Vector3.ProjectOnPlane(_cinemachineCamera.transform.forward, Vector3.up).normalized;
        
        private void OnDestroy()
        {
            if (_inputHub != null)
                _inputHub.OnDrag -= OnDrag;
        }
    }
}
