using System;
using Common;
using UnityEngine;
using CombatCommandType = Common.GameDefine.CombatCommandType;
using CombatState = Common.GameDefine.CombatState;
using BlendTreeType = Common.GameDefine.BlendTreeType;
using AvatarMaskType = Common.GameDefine.AvatarMaskType;

namespace InGame.Component.Command
{
    public class CharacterZoomCombatCommand : ICombatCommand
    {
        public CombatCommandType CommandType { get; } = CombatCommandType.Zoom;
        public bool IsCombatLock { get; } = false;
        public MoveCommandGroup[] LockedMoveGroups { get; } = null;

        public event Action<ICombatCommand> OnFinished;
        
        private ComponentBank _componentBank;
        public void Entry(ComponentBank componentBank)
        {
            _componentBank = componentBank;
            if (_componentBank.CharacterModel.CombatState.Value is not CombatState.Standard)
            {
                Debug.LogError("State is not Standard");
                return;
            }

            _componentBank.CharacterModel.CombatState.Value = CombatState.Zoom;
            _componentBank.AnimationPlayer.PlayBlendTree(BlendTreeType.AimMove2D, AvatarMaskType.Base);
        }

        public void Stay()
        {
        }

        public void FixedStay()
        {
        }

        public void Exit()
        {
            _componentBank.CharacterModel.CombatState.Value = CombatState.Standard;
            _componentBank.AnimationPlayer.PlayBlendTree(BlendTreeType.Move1D, AvatarMaskType.Base);
        }
    }
}
