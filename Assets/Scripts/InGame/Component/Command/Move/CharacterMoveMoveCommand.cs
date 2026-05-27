using System;
using Common;
using UniRx;
using UnityEngine;
using MoveCommandType = Common.GameDefine.MoveCommandType;
using BlendTreeType = Common.GameDefine.BlendTreeType;
using AvatarMaskType = Common.GameDefine.AvatarMaskType;

namespace InGame.Component.Command
{
    public class CharacterMoveMoveCommand : IMoveCommand
    {
        public MoveCommandType CommandType { get; } = MoveCommandType.Move;
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
        private IDisposable _runDisposable;
        private bool _isRun;
        private IDisposable _isLandDisposable;
        private bool _isLand;
        private float _fallingElapsed;
        
        public void Entry()
        {
            _componentBank.AnimationPlayer.PlayBlendTree(BlendTreeType.Move1D, AvatarMaskType.Base);
            _moveDirection = _componentBank.CharacterModel.MoveDirection.Value;
            _moveDirectionDisposable = _componentBank.CharacterModel.MoveDirection.Subscribe(OnMoveDirectionChanged);
            _isRun = _componentBank.CharacterModel.IsRun.Value;
            _runDisposable = _componentBank.CharacterModel.IsRun.Subscribe(OnRunChanged);
            _isLand = _componentBank.CharacterModel.IsLand.Value;
            _isLandDisposable = _componentBank.CharacterModel.IsLand.Subscribe(v => _isLand = v);
            SetAnimationParameter();
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

            if (_moveDirection == Vector2.zero)
                return;

            var camForward = _componentBank.CameraController.GetForward();
            var camRight = Vector3.Cross(Vector3.up, camForward);
            var worldDirection = camForward * _moveDirection.y + camRight * _moveDirection.x;
            worldDirection *= _isRun ? GameDefine.DefaultRunSpeed : GameDefine.DefaultMoveSpeed;

            _componentBank.Model.transform.rotation = Quaternion.Slerp(
                _componentBank.Model.transform.rotation,
                Quaternion.LookRotation(worldDirection),
                GameDefine.DefaultRotationSpeed * Time.deltaTime
            );
        }

        public void FixedStay()
        {
            if (_moveDirection == Vector2.zero)
                return;

            var camForward = _componentBank.CameraController.GetForward();
            var camRight = Vector3.Cross(Vector3.up, camForward);
            var worldDirection = camForward * _moveDirection.y + camRight * _moveDirection.x;
            worldDirection *= _isRun ? GameDefine.DefaultRunSpeed : GameDefine.DefaultMoveSpeed;
            _componentBank.Rigidbody.linearVelocity = new Vector3(
                worldDirection.x,
                _componentBank.Rigidbody.linearVelocity.y,
                worldDirection.z
            );
        }

        public void Exit()
        {
            _moveDirectionDisposable?.Dispose();
            _runDisposable?.Dispose();
            _isLandDisposable?.Dispose();
            _onMoveCommandFinished = null;
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

        private void SetAnimationParameter(bool isLerp = true)
        {
            if(_moveDirection == Vector2.zero)
                _componentBank.AnimationPlayer.SetParameter(AvatarMaskType.Base, 0, isLerp);
            else if(!_isRun)
                _componentBank.AnimationPlayer.SetParameter(AvatarMaskType.Base, 0.5f, isLerp);
            else
                _componentBank.AnimationPlayer.SetParameter(AvatarMaskType.Base, 1f, isLerp);
        }
    }
}
