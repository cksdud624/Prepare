using System;
using Common;
using Generated.Table;
using UnityEngine;
using AnimationType = Common.GameDefine.AnimationType;
using CombatCommandType = Common.GameDefine.CombatCommandType;

namespace InGame.Component.Command
{
    public class CharacterFireCombatCommand : ICombatCommand
    {
        public CombatCommandType CommandType { get; } = CombatCommandType.Fire;
        public bool IsCombatLock { get; } = true;
        public MoveCommandGroup[] LockedMoveGroups { get; } = null;
        public event Action<ICombatCommand> OnFinished;

        private ComponentBank _componentBank;
        private WeaponStatusData _weaponStatusData;
        private float _elapsed;
        private float _attackSpeed;
        private Quaternion _targetRotation;

        public void Entry(ComponentBank componentBank)
        {
            _componentBank = componentBank;
            _weaponStatusData = _componentBank.CharacterModel.CurrentWeapon;
            _attackSpeed = _weaponStatusData.AttackSpeed;
            _elapsed = 0f;
            _componentBank.AnimationPlayer.PlayAnimation(AnimationType.Fire, GameDefine.AvatarMaskType.Upper ,playDuration: _attackSpeed);
            
            var forward = _componentBank.CameraController.GetForward();
            if (forward != Vector3.zero)
            {
                _targetRotation = Quaternion.LookRotation(forward);
            }
        }

        public void Stay()
        {
            _elapsed += Time.deltaTime;
            if (_elapsed >= _attackSpeed)
                Finish();

            _componentBank.Model.transform.rotation = Quaternion.Slerp(
                _componentBank.Model.transform.rotation,
                _targetRotation,
                Time.deltaTime * GameDefine.DefaultRotationSpeed);
        }

        public void FixedStay()
        {
        }

        public void Exit()
        {
            //_componentBank.AnimationPlayer.StopAnimation(GameDefine.AvatarMaskType.Upper);
        }

        private void Finish()
        {
            OnFinished?.Invoke(this);
        }
    }
}
