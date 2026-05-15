using Common;
using MoveCommandType = Common.GameDefine.MoveCommandType;
using BlendTreeType = Common.GameDefine.BlendTreeType;
using AvatarMaskType = Common.GameDefine.AvatarMaskType;

namespace InGame.Component.Command
{
    public class CharacterIdleMoveCommand : IMoveCommand
    {
        public MoveCommandType CommandType { get; } 
        public MoveCommandGroup? MoveCommandsGroup { get; } = MoveCommandGroup.Locomotion;
        private ComponentBank _componentBank;
        private bool _isLocked;

        public void Entry(ComponentBank componentBank, bool isLocked)
        {
            _componentBank = componentBank;
            _isLocked = isLocked;
            if (_isLocked)
                return;
            _componentBank.AnimationPlayer.PlayBlendTree(BlendTreeType.Move1D, AvatarMaskType.Base);
        }

        public void Stay()
        {
        }

        public void FixedStay()
        {
        }

        public void Exit()
        {
        }

        public void Lock() => _isLocked = true;

        public void UnLock()
        {
            _isLocked = false;
        }
    }
}
