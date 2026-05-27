using System;
using System.Threading;
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
        private CinemachineThirdPersonFollow _thirdPersonFollow;
        private GameObject _playerSight;
        private GameObject _aimTarget;
        private float _pitch;
        private float _yaw;

        private CancellationTokenSource _cameraLerpCts;

        public async UniTask Init(InGameModel inGameModel, InputHub inputHub, CharacterModel characterModel)
        {
            _inGameModel = inGameModel;
            _inputHub = inputHub;
            _characterModel = characterModel;
            _inputHub.OnDrag += OnDrag;

            _playerSight = new GameObject("PlayerSight");
            _playerSight.transform.SetParent(transform);
            _playerSight.transform.localPosition = GameDefine.DefaultPlayerSight;
            _playerSight.transform.rotation = Quaternion.identity;

            _aimTarget = new GameObject("AimTarget");
            _aimTarget.transform.SetParent(_playerSight.transform);
            _aimTarget.transform.localPosition = new Vector3(0f, 0f, 100f);
            _aimTarget.transform.localRotation = Quaternion.identity;

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
            switch (combatState)
            {
                case CombatState.Standard:
                    LerpCamera(GameDefine.DefaultCameraDistance, 0f).Forget();
                    break;
                case CombatState.Aim:
                    LerpCamera(GameDefine.DefaultZoomCameraDistance, GameDefine.DefaultZoomCameraShoulderOffsetX).Forget();
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
            _cinemachineCamera.LookAt = _aimTarget.transform;
            _thirdPersonFollow = _cinemachineCamera.GetComponent<CinemachineThirdPersonFollow>();
        }

        public void DetachCamera()
        {
            _cinemachineCamera.Follow = null;
            _cinemachineCamera.LookAt = null;
            _cinemachineCamera = null;
        }

        public Vector3 GetForward() => Vector3.ProjectOnPlane(_playerSight.transform.forward, Vector3.up).normalized;

        private async UniTaskVoid LerpCamera(float targetDistance, float targetShoulderX)
        {
            _cameraLerpCts?.Cancel();
            _cameraLerpCts = new CancellationTokenSource();
            var token = _cameraLerpCts.Token;

            if (_thirdPersonFollow == null)
                return;

            float startDistance = _thirdPersonFollow.CameraDistance;
            float startShoulderX = _thirdPersonFollow.ShoulderOffset.x;

            float elapsed = 0f;
            try
            {
                while (elapsed < GameDefine.DefaultCameraLerpTime)
                {
                    elapsed += Time.deltaTime;
                    float t = Mathf.Clamp01(elapsed / GameDefine.DefaultCameraLerpTime);

                    _thirdPersonFollow.CameraDistance = Mathf.Lerp(startDistance, targetDistance, t);
                    var offset = _thirdPersonFollow.ShoulderOffset;
                    offset.x = Mathf.Lerp(startShoulderX, targetShoulderX, t);
                    _thirdPersonFollow.ShoulderOffset = offset;

                    await UniTask.Yield(PlayerLoopTiming.Update, token);
                }
            }
            catch (OperationCanceledException) { }
        }

        private void OnDestroy()
        {
            _cameraLerpCts?.Cancel();
            if (_inputHub != null)
                _inputHub.OnDrag -= OnDrag;
            _characterModel?.CombatState.Dispose();
        }
    }
}
