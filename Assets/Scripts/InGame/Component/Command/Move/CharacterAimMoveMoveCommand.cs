using System;
using System.Threading;
using Common;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using MoveCommandType = Common.GameDefine.MoveCommandType;
using AvatarMaskType = Common.GameDefine.AvatarMaskType;
using WeaponType = Common.GameDefine.WeaponType;
using WeaponBlendTreeType = Common.GameDefine.WeaponBlendTreeType;

namespace InGame.Component.Command
{
    public class CharacterAimMoveMoveCommand : IMoveCommand
    {
        public MoveCommandType CommandType { get; } = MoveCommandType.AimMove;
        public MoveCommandGroup? MoveCommandsGroup { get; } = MoveCommandGroup.Locomotion;

        public void Init(Action<IMoveCommand> onFinished, IMoveCommand transfer, ComponentBank componentBank)
        {
            _onMoveCommandFinished = onFinished;
            _componentBank = componentBank;
        }

        private ComponentBank _componentBank;
        private Action<IMoveCommand> _onMoveCommandFinished;

        private IDisposable _moveDirectionDisposable;
        private Vector2 _moveDirection;
        private IDisposable _isLandDisposable;
        private bool _isLand;
        private float _fallingElapsed;
        private CancellationTokenSource _entryCancelToken;
        private bool _isEntryRotating;
        
        public void Entry()
        {
            var weaponType = (WeaponType)_componentBank.CharacterModel.CurrentWeapon.WeaponType;
            _componentBank.AnimationPlayer.PlayBlendTree(weaponType, WeaponBlendTreeType.AimMove2D, AvatarMaskType.Base);
            _moveDirection = _componentBank.CharacterModel.MoveDirection.Value;
            _moveDirectionDisposable = _componentBank.CharacterModel.MoveDirection.Subscribe(OnMoveDirectionChanged);
            _isLand = _componentBank.CharacterModel.IsLand.Value;
            _isLandDisposable = _componentBank.CharacterModel.IsLand.Subscribe(v => _isLand = v);
            SetAnimationParameter(false);
            SlerpEntryRotation(GameDefine.DefaultEntryRotationTime).Forget();
        }

        public void Stay()
        {
            if (!_isLand)
            {
                _fallingElapsed += Time.deltaTime;
                if (_fallingElapsed >= GameDefine.DefaultFallToFlyTime)
                    _onMoveCommandFinished?.Invoke(this);
            }
            else
            {
                _fallingElapsed = 0f;
            }

            var camForward = _componentBank.CameraController.GetForward();

            if (!_isEntryRotating)
                _componentBank.Model.transform.rotation = Quaternion.LookRotation(camForward);

            SetAnimationParameter();
        }

        public void FixedStay()
        {
            var camForward = _componentBank.CameraController.GetForward();
            var camRight = Vector3.Cross(Vector3.up, camForward);
            var worldDirection = camForward * _moveDirection.y + camRight * _moveDirection.x;
            worldDirection *= GameDefine.DefaultMoveSpeed;
            _componentBank.Rigidbody.linearVelocity = new Vector3(
                worldDirection.x,
                _componentBank.Rigidbody.linearVelocity.y,
                worldDirection.z
            );
        }

        public void Exit()
        {
            _moveDirectionDisposable?.Dispose();
            _isLandDisposable?.Dispose();
            _entryCancelToken?.Cancel();
            _onMoveCommandFinished = null;
            _componentBank.AnimationPlayer.SetUpperBodyOffset(0);
        }

        #region Events
        private void OnMoveDirectionChanged(Vector2 direction)
        {
            bool isZero = _moveDirection == Vector2.zero;
            _moveDirection = direction;

            if (isZero != (_moveDirection == Vector2.zero))
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
            var param = new Vector2(
                _moveDirection.x > 0f ? 1f : _moveDirection.x < 0f ? -1f : 0f,
                _moveDirection.y > 0f ? 1f : _moveDirection.y < 0f ? -1f : 0f
            );
            if(_moveDirection == new Vector2(0f, 1f))
                _componentBank.AnimationPlayer.SetUpperBodyOffset(20);
            else
                _componentBank.AnimationPlayer.SetUpperBodyOffset(0);
            _componentBank.AnimationPlayer.SetParameter(AvatarMaskType.Base, param, isLerp);
        }
    }
}
