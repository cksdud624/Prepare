using System;
using Common;
using CombatCommandType = Common.GameDefine.CombatCommandType;
using WeaponType = Common.GameDefine.WeaponType;
using WeaponAnimationType = Common.GameDefine.WeaponAnimationType;
using AvatarMaskType = Common.GameDefine.AvatarMaskType;

namespace InGame.Component.Command
{
    public class CharacterAirCombatCommand : ICombatCommand
    {
        public CombatCommandType CommandType { get; } = CombatCommandType.Air;
        public CombatCommandGroup? CombatCommandsGroup { get; } = CombatCommandGroup.Handle;
        private ComponentBank _componentBank;

        public void Init(Action<ICombatCommand> onFinished, ICombatCommand transfer, ComponentBank componentBank)
        {
            _componentBank = componentBank;
        }

        public void Entry()
        {
            _componentBank.AnimationPlayer.PlayAnimation(WeaponType.Pistol, WeaponAnimationType.Air, AvatarMaskType.Upper);
        }

        public void Stay() { }

        public void FixedStay() { }

        public void Exit() { }
    }
}
