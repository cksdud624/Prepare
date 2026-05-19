using System;
using System.Threading;
using Common;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using MoveCommandType = Common.GameDefine.MoveCommandType;
using BlendTreeType = Common.GameDefine.BlendTreeType;
using AvatarMaskType = Common.GameDefine.AvatarMaskType;

namespace InGame.Component.Command
{
    public class CharacterAimMoveMoveCommand : IMoveCommand
    {
public MoveCommandType CommandType { get; } = MoveCommandType.Move;
        public MoveCommandGroup? MoveCommandsGroup { get; } = MoveCommandGroup.Locomotion;
        private ComponentBank _componentBank;
        
        private IDisposable _moveDirectionDisposable;
        private Vector2 _moveDirection;
        private IDisposable _runDisposable;
        private bool _isRun;
        private CancellationTokenSource _entryCancelToken;
        private bool _isEntryRotating;
        
        public void Entry(ComponentBank componentBank, bool isLocked)
        {
            _componentBank = componentBank;
            _componentBank.AnimationPlayer.PlayBlendTree(BlendTreeType.AimMove2D, AvatarMaskType.Base);
            _moveDirection = _componentBank.CharacterModel.MoveDirection.Value;
            _moveDirectionDisposable = _componentBank.CharacterModel.MoveDirection.Subscribe(OnMoveDirectionChanged);
            _isRun = _componentBank.CharacterModel.IsRun.Value;
            _runDisposable = _componentBank.CharacterModel.IsRun.Subscribe(OnRunChanged);
            SetAnimationParameter();
            SlerpEntryRotation(GameDefine.DefaultEntryRotationTime).Forget();
        }

        public void Stay()
        {
            var camForward = _componentBank.CameraController.GetForward();

            if (!_isEntryRotating)
            {
                _componentBank.Model.transform.rotation = Quaternion.LookRotation(camForward);
            }

            var camRight = Vector3.Cross(Vector3.up, camForward);
            var worldDirection = camForward * _moveDirection.y + camRight * _moveDirection.x;
            _componentBank.Rigidbody.linearVelocity = worldDirection;
            SetAnimationParameter();
        }

        public void FixedStay()
        {
        }

        public void Exit()
        { 
            _moveDirectionDisposable?.Dispose();
            _runDisposable?.Dispose();
            _entryCancelToken?.Cancel();
        }

        public void Lock()
        {
        }

        public void UnLock()
        {
        }

        #region Events
        private void OnMoveDirectionChanged(Vector2 direction)
        {
            bool isZero = _moveDirection == Vector2.zero;
            _moveDirection = direction;

            if (isZero != (_moveDirection == Vector2.zero))
                SetAnimationParameter();
        }

        private void OnRunChanged(bool isRun)
        {
            _isRun = isRun;
            if(_moveDirection != Vector2.zero)
                SetAnimationParameter();
        }

        #endregion
        
        #region Async

        private async UniTask SlerpEntryRotation(float duration)
        {
            _entryCancelToken?.Cancel();
            _entryCancelToken = new CancellationTokenSource();
            _isEntryRotating = true;
            float elapsed = 0f;
            var startRotation = _componentBank.Model.transform.rotation;
            try
            {
                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float progress = Mathf.Clamp01(elapsed / duration);
                    var camForward = _componentBank.CameraController.GetForward();
                    var targetRotation = Quaternion.LookRotation(camForward);
                    _componentBank.Model.transform.rotation = Quaternion.Slerp(
                        startRotation,
                        targetRotation,
                        progress
                    );
                    await UniTask.Yield(PlayerLoopTiming.Update, _entryCancelToken.Token);
                }
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                _isEntryRotating = false;
            }
        }
        #endregion

        private void SetAnimationParameter(bool isLerp = true)
        {
            float balancer = _isRun ? 1f : 0.5f;
            _componentBank.AnimationPlayer.SetParameter(AvatarMaskType.Base, _moveDirection.normalized * balancer, isLerp);
        }
    }
}
