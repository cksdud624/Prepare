namespace InGame.Component.Command
{
    public class CharacterFireCombatCommand : ICombatCommand
    {
        public bool IsLock { get; } = true;
        public bool IsFinished { get; private set; }
        
        private ComponentBank _componentBank;

        public void Entry(ComponentBank componentBank)
        {
            _componentBank = componentBank;
        }

        public void Stay()
        {
        }

        public void Exit()
        {
        }
    }
}
