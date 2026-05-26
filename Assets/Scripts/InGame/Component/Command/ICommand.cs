using System;
using System.Collections.Generic;
using Common;
using CombatCommandType = Common.GameDefine.CombatCommandType;
using MoveCommandType = Common.GameDefine.MoveCommandType;

namespace InGame.Component.Command
{
    public enum MoveCommandGroup { Locomotion }

    public interface IMoveCommand
    {
        MoveCommandType CommandType { get; }
        //같은 그룹은 하나만 존재할 수 있음
        MoveCommandGroup? MoveCommandsGroup { get; }
        void Init(Action<IMoveCommand> onFinished, IMoveCommand transfer);
        //isLocked의 역할은 커맨드 자체는 적용되고 있지만 내부 동작은 하지 않는 방향임
        void Entry(ComponentBank componentBank, bool isLocked);
        void Stay();
        void FixedStay();
        void Exit();
        void Lock();
        void UnLock();
    }

    public interface ICombatCommand
    {
        CombatCommandType CommandType { get; }
        bool IsCombatLock { get; }
        MoveCommandGroup[] LockedMoveGroups { get; }
        event Action<ICombatCommand> OnFinished;
        void Entry(ComponentBank componentBank);
        void Stay();
        void FixedStay();
        void Exit();
    }
}
