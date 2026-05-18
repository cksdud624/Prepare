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
        private ComponentBank _componentBank;
        
        private IDisposable _moveDirectionDisposable;
        private Vector2 _moveDirection;
        private IDisposable _runDisposable;
        private bool _isRun;
        
        public void Entry(ComponentBank componentBank, bool isLocked)
        {
            _componentBank = componentBank;
            _componentBank.AnimationPlayer.PlayBlendTree(BlendTreeType.Move1D, AvatarMaskType.Base);
            _moveDirection = _componentBank.CharacterModel.MoveDirection.Value;
            _moveDirectionDisposable = _componentBank.CharacterModel.MoveDirection.Subscribe(OnMoveDirectionChanged);
            _isRun = _componentBank.CharacterModel.IsRun.Value;
            _runDisposable = _componentBank.CharacterModel.IsRun.Subscribe(OnRunChanged);
            SetAnimationParameter();
        }

        public void Stay()
        {
            if (_moveDirection == Vector2.zero)
                return;
            
            var camForward = _componentBank.CameraController.GetForward();
            var camRight = Vector3.Cross(Vector3.up, camForward);
            var worldDirection = camForward * _moveDirection.y + camRight * _moveDirection.x;
            if (_isRun)
                worldDirection *= GameDefine.DefaultRunSpeed;
            else
                worldDirection *= GameDefine.DefaultMoveSpeed;

            var targetRotation = Quaternion.LookRotation(worldDirection);
            _componentBank.Model.transform.rotation = Quaternion.Slerp(
                _componentBank.Model.transform.rotation,
                targetRotation,
                GameDefine.DefaultRotationSpeed * Time.deltaTime
            );

            _componentBank.Rigidbody.linearVelocity = worldDirection;
        }

        public void FixedStay()
        {
        }

        public void Exit()
        { 
            _moveDirectionDisposable?.Dispose();
            _runDisposable?.Dispose();
        }

        public void Lock()
        {
            throw new System.NotImplementedException();
        }

        public void UnLock()
        {
            throw new System.NotImplementedException();
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

        private void SetAnimationParameter()
        {
            if(_moveDirection == Vector2.zero)
                _componentBank.AnimationPlayer.SetParameter(AvatarMaskType.Base, 0);
            else if(!_isRun)
                _componentBank.AnimationPlayer.SetParameter(AvatarMaskType.Base, 0.5f);
            else
                _componentBank.AnimationPlayer.SetParameter(AvatarMaskType.Base, 1f);
        }
    }
}
