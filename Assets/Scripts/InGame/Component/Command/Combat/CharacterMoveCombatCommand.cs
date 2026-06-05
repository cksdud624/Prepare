using System;
using UniRx;
using UnityEngine;
using CombatCommandType = Common.GameDefine.CombatCommandType;
using WeaponType = Common.GameDefine.WeaponType;
using WeaponBlendTreeType = Common.GameDefine.WeaponBlendTreeType;
using AvatarMaskType = Common.GameDefine.AvatarMaskType;

namespace InGame.Component.Command
{
    public class CharacterMoveCombatCommand : ICombatCommand
    {
        public CombatCommandType CommandType { get; } = CombatCommandType.Move;
        public CombatCommandGroup? CombatCommandsGroup { get; } = CombatCommandGroup.Handle;
        private ComponentBank _componentBank;

        private Action<ICombatCommand> _onFinished;
        private IDisposable _moveDirectionDisposable;
        private Vector2 _moveDirection;
        private IDisposable _sprintDisposable;
        private bool _isSprint;
        private IDisposable _isLandDisposable;

        public void Init(Action<ICombatCommand> onFinished, ICombatCommand transfer, ComponentBank componentBank)
        {
            _componentBank = componentBank;
            _onFinished = onFinished;
        }

        public void Entry()
        {
            var weaponType = (WeaponType)_componentBank.CharacterModel.CurrentWeapon.WeaponType;
            _componentBank.AnimationPlayer.PlayBlendTree(weaponType, WeaponBlendTreeType.Move1D, AvatarMaskType.Upper);

            _moveDirection = _componentBank.CharacterModel.MoveDirection.Value;
            _moveDirectionDisposable = _componentBank.CharacterModel.MoveDirection.Subscribe(OnMoveDirectionChanged);
            _isSprint = _componentBank.CharacterModel.IsSprint.Value;
            _sprintDisposable = _componentBank.CharacterModel.IsSprint.Subscribe(OnSprintChanged);
            _isLandDisposable = _componentBank.CharacterModel.IsLand.Subscribe(OnIsLandChanged);
            SetAnimationParameter();
        }

        public void Stay() { }

        public void FixedStay() { }

        public void Exit()
        {
            _moveDirectionDisposable?.Dispose();
            _sprintDisposable?.Dispose();
            _isLandDisposable?.Dispose();
            _onFinished = null;
        }

        private void OnMoveDirectionChanged(Vector2 direction)
        {
            bool wasZero = _moveDirection == Vector2.zero;
            _moveDirection = direction;
            if (wasZero != (_moveDirection == Vector2.zero))
                SetAnimationParameter();
        }

        private void OnSprintChanged(bool isSprint)
        {
            _isSprint = isSprint;
            if (_moveDirection != Vector2.zero)
                SetAnimationParameter();
        }

        private void OnIsLandChanged(bool isLand)
        {
            if(!isLand)
                _onFinished?.Invoke(this);
        }

        private void SetAnimationParameter(bool isLerp = true)
        {
            if (_moveDirection == Vector2.zero)
                _componentBank.AnimationPlayer.SetParameter(AvatarMaskType.Upper, 0f, isLerp);
            else if (!_isSprint)
                _componentBank.AnimationPlayer.SetParameter(AvatarMaskType.Upper, 0.5f, isLerp);
            else
                _componentBank.AnimationPlayer.SetParameter(AvatarMaskType.Upper, 1f, isLerp);
        }
    }
}
