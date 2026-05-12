namespace InGame.Component.Command
{
    public enum MoveCommandGroup { Locomotion }

    public interface IMoveCommand
    {
        MoveCommandGroup? ExclusiveGroup { get; }
        void Entry(ComponentBank componentBank);
        void Stay();
        void Exit();
    }
    
    public interface ICombatCommand
    {
        bool IsLock { get; }
        bool IsFinished { get; }
        void Entry(ComponentBank componentBank);
        void Stay();
        void Exit();
    }
}
