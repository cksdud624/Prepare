using System;
using Common;
using Cysharp.Threading.Tasks;
using InGame.Component.Hub;
using InGame.Component.Model;
using InGame.Model;
using UniRx;
using Unity.Cinemachine;
using UnityEngine;
using CombatState = Common.GameDefine.CombatState;

namespace InGame.Component.Module
{
    public class CameraController : MonoBehaviour
    {
        private InGameModel _inGameModel;
        private InputHub _inputHub;
        private CharacterModel _characterModel;
        private CinemachineCamera _cinemachineCamera;
        private GameObject _playerSight;
        private float _pitch;
        private float _yaw;
        
        private IDisposable _combatStateDisposable;

        public async UniTask Init(InGameModel inGameModel, InputHub inputHub, CharacterModel characterModel)
        {
            _inGameModel = inGameModel;
            _inputHub = inputHub;
            _characterModel = characterModel;
            _inputHub.OnDrag += OnDrag;
            _playerSight = new GameObject("PlayerSight");
            _playerSight.transform.rotation = Quaternion.identity;
            OnCombatStateChanged(_characterModel.CombatState.Value);
            _characterModel.CombatState.Subscribe(OnCombatStateChanged);
            await UniTask.CompletedTask;
        }
        
        #region Events
        private void OnDrag(Vector2 drag)
        {
            _yaw += drag.x * GameDefine.DefaultDragRotationSensitivity;
            _pitch -= drag.y * GameDefine.DefaultDragRotationSensitivity;
            _pitch = Mathf.Clamp(_pitch, GameDefine.DefaultCameraPitchMin, GameDefine.DefaultCameraPitchMax);
            _playerSight.transform.localRotation = Quaternion.Euler(_pitch, _yaw, 0f);
        }

        private void OnCombatStateChanged(CombatState combatState)
        {
            //카메라 줌
            switch(combatState)
            {
                case  CombatState.Standard:
                    _playerSight.transform.SetParent(transform);
                    _playerSight.transform.localPosition = GameDefine.DefaultPlayerSight;
                    break;
                case CombatState.Zoom:
                    if (_cinemachineCamera == null)
                        break;
                    break;
                default:
                    Debug.LogError($"CombatState is not implemented {combatState}");
                    break;
            }
        }
        #endregion

        public void AttachCamera(CinemachineCamera cinemachineCamera)
        {
            _cinemachineCamera = cinemachineCamera;
            _cinemachineCamera.Follow = _playerSight.transform;
        }

        public void DetachCamera()
        {
            _cinemachineCamera.Follow = null;
            _cinemachineCamera = null;
        }

        public Vector3 GetForward() => Vector3.ProjectOnPlane(_cinemachineCamera.transform.forward, Vector3.up).normalized;
        
        private void OnDestroy()
        {
            if (_inputHub != null)
                _inputHub.OnDrag -= OnDrag;
            _characterModel?.CombatState.Dispose();
        }
    }
}
