using System;
using Common;
using CombatCommandType = Common.GameDefine.CombatCommandType;
using CombatState = Common.GameDefine.CombatState;
using AnimationType = Common.GameDefine.AnimationType;

namespace InGame.Component.Command
{
    public class CharacterAimCombatCommand : ICombatCommand
    {
        public CombatCommandType CommandType { get; } = CombatCommandType.Aim;
        public bool IsCombatLock { get; } = false;
        public MoveCommandGroup[] LockedMoveGroups { get; } = null;
        public event Action<ICombatCommand> OnFinished;
        private ComponentBank _componentBank;
        public void Entry(ComponentBank componentBank)
        {
            _componentBank = componentBank;
            _componentBank.CharacterModel.CombatState = CombatState.Aim;
            _componentBank.AnimationPlayer.PlayAnimation(AnimationType.Aim, GameDefine.AvatarMaskType.Upper);
        }

        public void Stay()
        {
        }

        public void FixedStay()
        {
        }

        public void Exit()
        {
            _componentBank.AnimationPlayer.StopLayer(GameDefine.AvatarMaskType.Upper);
        }
    }
}
