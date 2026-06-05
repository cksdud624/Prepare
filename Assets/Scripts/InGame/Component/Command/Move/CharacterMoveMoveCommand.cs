using System;
using Common;
using UniRx;
using UnityEngine;
using MoveCommandType = Common.GameDefine.MoveCommandType;
using AvatarMaskType = Common.GameDefine.AvatarMaskType;
using WeaponType = Common.GameDefine.WeaponType;
using WeaponBlendTreeType = Common.GameDefine.WeaponBlendTreeType;

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
        private IDisposable _sprintDisposable;
        private bool _isSprint;
        private IDisposable _isLandDisposable;
        private bool _isLand;
        private float _fallingElapsed;
        
        public void Entry()
        {
            var weaponType = (WeaponType)_componentBank.CharacterModel.CurrentWeapon.WeaponType;
            _componentBank.AnimationPlayer.PlayBlendTree(weaponType, WeaponBlendTreeType.Move1D, AvatarMaskType.Base);
            _moveDirection = _componentBank.CharacterModel.MoveDirection.Value;
            _moveDirectionDisposable = _componentBank.CharacterModel.MoveDirection.Subscribe(OnMoveDirectionChanged);
            _isSprint = _componentBank.CharacterModel.IsSprint.Value;
            _sprintDisposable = _componentBank.CharacterModel.IsSprint.Subscribe(OnRunChanged);
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
            worldDirection *= _isSprint ? GameDefine.DefaultRunSpeed : GameDefine.DefaultMoveSpeed;

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
            worldDirection *= _isSprint ? GameDefine.DefaultRunSpeed : GameDefine.DefaultMoveSpeed;
            _componentBank.Rigidbody.linearVelocity = new Vector3(
                worldDirection.x,
                _componentBank.Rigidbody.linearVelocity.y,
                worldDirection.z
            );
        }

        public void Exit()
        {
            _moveDirectionDisposable?.Dispose();
            _sprintDisposable?.Dispose();
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

        private void OnRunChanged(bool isSprint)
        {
            _isSprint = isSprint;
            if(_moveDirection != Vector2.zero)
                SetAnimationParameter();
        }

        #endregion

        private void SetAnimationParameter(bool isLerp = true)
        {
            Debug.Log("아잇!");
            if(_moveDirection == Vector2.zero)
                _componentBank.AnimationPlayer.SetParameter(AvatarMaskType.Base, 0, isLerp);
            else if(!_isSprint)
                _componentBank.AnimationPlayer.SetParameter(AvatarMaskType.Base, 0.5f, isLerp);
            else
                _componentBank.AnimationPlayer.SetParameter(AvatarMaskType.Base, 1f, isLerp);
        }
    }
}
