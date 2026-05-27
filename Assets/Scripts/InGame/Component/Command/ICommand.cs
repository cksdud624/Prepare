using System;
using CombatCommandType = Common.GameDefine.CombatCommandType;
using MoveCommandType = Common.GameDefine.MoveCommandType;

namespace InGame.Component.Command
{
    public enum MoveCommandGroup { Locomotion }
    public enum CombatCommandGroup { Handle }

    public interface IMoveCommand
    {
        MoveCommandType CommandType { get; }
        //같은 그룹은 하나만 존재할 수 있음
        MoveCommandGroup? MoveCommandsGroup { get; }
        void Init(Action<IMoveCommand> onFinished, IMoveCommand transfer, ComponentBank componentBank);
        void Entry();
        void Stay();
        void FixedStay();
        void Exit();
    }

    public interface ICombatCommand
    {
        CombatCommandType CommandType { get; }
        CombatCommandGroup? CombatCommandsGroup { get; }
        void Init(Action<ICombatCommand> onFinished, ICombatCommand transfer, ComponentBank componentBank);
        void Entry();
        void Stay();
        void FixedStay();
        void Exit();
    }
}
