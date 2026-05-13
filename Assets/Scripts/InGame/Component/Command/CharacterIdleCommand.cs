using Common;
using AnimationType = Common.GameDefine.AnimationType;
using MoveCommandType = Common.GameDefine.MoveCommandType;

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
            _componentBank.AnimationPlayer.StopLayer(GameDefine.AvatarMaskType.Lower);
            if (_isLocked)
                return;
            _componentBank.AnimationPlayer.PlayAnimation(AnimationType.Idle, GameDefine.AvatarMaskType.Full);
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
            _componentBank.AnimationPlayer.PlayAnimation(AnimationType.Idle, GameDefine.AvatarMaskType.Full);
        }
    }
}
