using System;
using CombatCommandType = Common.GameDefine.CombatCommandType;
using WeaponType = Common.GameDefine.WeaponType;
using WeaponAnimationType = Common.GameDefine.WeaponAnimationType;
using AvatarMaskType = Common.GameDefine.AvatarMaskType;
using UniRx;

namespace InGame.Component.Command
{
    public class CharacterAirCombatCommand : ICombatCommand
    {
        public CombatCommandType CommandType { get; } = CombatCommandType.Air;
        public CombatCommandGroup? CombatCommandsGroup { get; } = CombatCommandGroup.Handle;
        private ComponentBank _componentBank;
        
        private Action<ICombatCommand> _onFinished;
        private IDisposable _isLandDisposable;

        public void Init(Action<ICombatCommand> onFinished, ICombatCommand transfer, ComponentBank componentBank)
        {
            _componentBank = componentBank;
            _onFinished = onFinished;
        }

        public void Entry()
        {
            _isLandDisposable = _componentBank.CharacterModel.IsLand.Subscribe(OnIsLandChanged);
            _componentBank.AnimationPlayer.PlayAnimation(WeaponType.Pistol, WeaponAnimationType.Air, AvatarMaskType.Upper);
        }

        public void Stay() { }

        public void FixedStay() { }

        public void Exit()
        {
            _isLandDisposable?.Dispose();
            _onFinished = null;
        }

        private void OnIsLandChanged(bool isLand)
        {
            if(isLand)
                _onFinished?.Invoke(this);
        }
    }
}
