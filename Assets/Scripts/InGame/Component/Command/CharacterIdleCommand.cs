using AnimationType = Common.GameDefine.AnimationType;

namespace InGame.Component.Command
{
    public class CharacterIdleMoveCommand : IMoveCommand
    {
        private ComponentBank _componentBank;
        
        public MoveCommandGroup? ExclusiveGroup => MoveCommandGroup.Locomotion;

        public void Entry(ComponentBank componentBank)
        {
            _componentBank = componentBank;
            componentBank.AnimationPlayer.PlayAnimation(AnimationType.Idle);
        }

        public void Stay()
        {
        }

        public void Exit()
        {
        }
    }
}
