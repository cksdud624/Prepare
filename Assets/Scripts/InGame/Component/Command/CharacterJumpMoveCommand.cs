using UnityEngine;
using MoveCommandType = Common.GameDefine.MoveCommandType;

namespace InGame.Component.Command
{
    public class CharacterJumpMoveCommand : IMoveCommand
    {
        public MoveCommandType CommandType { get; } = MoveCommandType.Jump;
        public MoveCommandGroup? MoveCommandsGroup { get; } = MoveCommandGroup.Locomotion;

        private ComponentBank _componentBank;
        public void Entry(ComponentBank componentBank, bool isLocked)
        {
            _componentBank = componentBank;
            
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

        public void Lock()
        {
        }

        public void UnLock()
        {
        }
    }
}
