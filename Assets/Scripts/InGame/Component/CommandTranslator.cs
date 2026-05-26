using System.Collections.Generic;
using Common;
using Common.Template.Interface;
using Cysharp.Threading.Tasks;
using InGame.Component.Command;
using InGame.Component.Hub;
using InGame.Model;
using UnityEngine;
using MoveCommandType = Common.GameDefine.MoveCommandType;
using CombatCommandType = Common.GameDefine.CombatCommandType;
using CombatState = Common.GameDefine.CombatState;
using UniRx;

namespace InGame.Component
{
    public class CommandTranslator : MonoBehaviour, IUpdateable, IFixedUpdateable
    {
        private InGameModel _inGameModel;
        private ComponentBank _componentBank;
        private InputHub _inputHub;

        private readonly List<IMoveCommand> _moveCommands = new();
        private readonly List<IMoveCommand> _removePendingMove = new();
        
        private readonly List<ICombatCommand> _combatCommands = new();
        private readonly List<ICombatCommand> _removePendingCombat = new();
        private readonly Dictionary<MoveCommandGroup, int> _lockedMoveGroups = new();
        private bool _isCombatLock;
        
        #region Command Generate Variables
        private float _fallingElapse;
        #endregion

        public async UniTask Init(InGameModel inGameModel, InputHub inputHub, ComponentBank componentBank)
        {
            _inGameModel = inGameModel;
            _componentBank = componentBank;
            _inputHub = inputHub;
            _inputHub.OnMove += OnMove;
            _inputHub.OnLeftClick += OnLeftClick;
            _inputHub.OnRightClick += OnRightClick;
            _inputHub.OnShiftClick += OnShiftClick;
            _inputHub.OnSpaceClick += OnSpaceClick;
            
            Global.Instance.BindUpdate(this);
            Global.Instance.BindFixedUpdate(this);

            PlayCommand(MoveCommandType.Move);
            await UniTask.CompletedTask;
        }

        #region Events
        private void OnMove(Vector2 direction)
        {
            _componentBank.CharacterModel.MoveDirection.Value = direction;
            foreach (var moveCommand in _moveCommands)
            {
                if (moveCommand.MoveCommandsGroup is MoveCommandGroup.Locomotion)
                    return;
            }
            PlayCommand(MoveCommandType.Move);
        }

        private void OnLeftClick(bool isClick)
        {
            return;
        }

        private void OnRightClick(bool isClick)
        {
            if (!isClick)
                return;

            switch (_componentBank.CharacterModel.CombatState.Value)
            {
                case CombatState.Standard:
                    _componentBank.CharacterModel.CombatState.Value = CombatState.Zoom;
                    break;
                case CombatState.Zoom:
                    _componentBank.CharacterModel.CombatState.Value = CombatState.Standard;
                    break;
            }

            foreach (var command in _moveCommands)
            {
                if (command.MoveCommandsGroup == MoveCommandGroup.Locomotion)
                {
                    switch (command.CommandType)
                    {
                        case MoveCommandType.Move:
                            PlayCommand(MoveCommandType.AimMove, command);
                            break;
                        case MoveCommandType.AimMove:
                            PlayCommand(MoveCommandType.Move, command);
                            break;
                        case MoveCommandType.Jump:
                            PlayCommand(MoveCommandType.AimJump, command);
                            break;
                        case MoveCommandType.AimJump:
                            PlayCommand(MoveCommandType.Jump, command);
                            break;
                        case MoveCommandType.Fly:
                            PlayCommand(MoveCommandType.AimFly, command);
                            break;
                        case MoveCommandType.AimFly:
                            PlayCommand(MoveCommandType.Fly, command);
                            break;
                    }
                    break;
                }
            }
        }

        private void OnShiftClick(bool isClick)
        {
            _componentBank.CharacterModel.IsRun.Value = isClick;
        }

        private void OnSpaceClick(bool isClick)
        {
            _componentBank.CharacterModel.IsFlyHolding.Value = isClick;
            
            if (!isClick)
                return;
            
            IMoveCommand jumpCommand = null;
            
            foreach (var command in _moveCommands)
            {
                if (command.CommandType is MoveCommandType.Jump or MoveCommandType.AimJump)
                    jumpCommand = command;
            }
            
            //case 1: 바닥에 붙어있을 때
            if (_componentBank.CharacterModel.IsLand.Value)
            {
                switch (_componentBank.CharacterModel.CombatState.Value)
                {
                    case CombatState.Zoom:
                        PlayCommand(MoveCommandType.AimJump);
                        break;
                    case CombatState.Standard:
                        PlayCommand(MoveCommandType.Jump);
                        break;
                    default:
                        Debug.LogError("CombatState is not valid");
                        break;
                }
            }
            //case 2: 점프 중일 때
            else if (jumpCommand != null)
            {
                switch (_componentBank.CharacterModel.CombatState.Value)
                {
                    case CombatState.Zoom:
                        PlayCommand(MoveCommandType.AimFly);
                        break;
                    case CombatState.Standard:
                        PlayCommand(MoveCommandType.Fly);
                        break;
                    default:
                        Debug.LogError("CombatState is not valid");
                        break;
                }
            }
            //case 3 : 이미 공중에 있을 때
            // => 이 부분은 Fly 커맨드 단에서 구현하는게 맞을 듯
        }

        public void OnUpdate()
        {
            foreach (var moveCommand in _moveCommands)
                moveCommand.Stay();
            foreach (var combatCommand in _combatCommands)
                combatCommand.Stay();

            if (_removePendingMove.Count > 0)
            {
                foreach (var command in _removePendingMove)
                {
                    command.Exit();
                    _moveCommands.Remove(command);
                    FinishCommand(command);
                }
                _removePendingMove.Clear();
            }

            if (_removePendingCombat.Count > 0)
            {
                foreach (var command in _removePendingCombat)
                {
                    if (command.IsCombatLock)
                        _isCombatLock = false;
                    command.Exit();
                    _combatCommands.Remove(command);
                }
                _removePendingCombat.Clear();
            }
        }

        public void OnFixedUpdate()
        {
            foreach (var moveCommand in _moveCommands)
                moveCommand.FixedStay();
            foreach (var combatCommand in _combatCommands)
                combatCommand.FixedStay();
            
            if (_removePendingMove.Count > 0)
            {
                foreach (var command in _removePendingMove)
                {
                    command.Exit();
                    _moveCommands.Remove(command);
                    FinishCommand(command);
                }
                _removePendingMove.Clear();
            }

            if (_removePendingCombat.Count > 0)
            {
                foreach (var command in _removePendingCombat)
                {
                    if (command.IsCombatLock)
                        _isCombatLock = false;
                    command.Exit();
                    _combatCommands.Remove(command);
                }
                _removePendingCombat.Clear();
            }
        }
        
        private void OnMoveCommandFinished(IMoveCommand moveCommand)
        {
            _removePendingMove.Add(moveCommand);
        }

        private void OnCombatCommandFinished(ICombatCommand command)
        {
            /*
            if (command.LockedMoveGroups is { Length: > 0 })
            {
                foreach (var lockGroup in command.LockedMoveGroups)
                {
                    _lockedMoveGroups[lockGroup]--;
                    if (_lockedMoveGroups[lockGroup] <= 0)
                    {
                        _lockedMoveGroups.Remove(lockGroup);
                        foreach (var moveCommand in _moveCommands)
                            moveCommand.UnLock();
                    }
                }
            }
            */
            command.OnFinished -= OnCombatCommandFinished;
            _removePendingCombat.Add(command);
        }
        #endregion

        private void PlayCommand(MoveCommandType moveCommandType, IMoveCommand transfer = null)
        {
            IMoveCommand newCommand = moveCommandType switch
            {
                MoveCommandType.Move    => new CharacterMoveMoveCommand(),
                MoveCommandType.AimMove => new CharacterAimMoveMoveCommand(),
                MoveCommandType.Jump    => new CharacterJumpMoveCommand(),
                MoveCommandType.AimJump => new CharacterAimJumpMoveCommand(),
                MoveCommandType.Fly     => new CharacterFlyMoveCommand(),
                MoveCommandType.AimFly  => new CharacterAimFlyMoveCommand(),
                _ => null
            };

            if (newCommand == null)
            {
                Debug.LogWarning("Command is not implemented: " + moveCommandType);
                return;
            }

            newCommand.Init(OnMoveCommandFinished, transfer);

            if (newCommand.MoveCommandsGroup.HasValue)
            {
                foreach (var command in _moveCommands)
                {
                    if (newCommand.MoveCommandsGroup == command.MoveCommandsGroup)
                    {
                        command.Exit();
                        _moveCommands.Remove(command);
                        break;
                    }
                }
            }
            _moveCommands.Add(newCommand);
            newCommand.Entry(_componentBank, false);
        }

        private void PlayCommand(CombatCommandType combatCommandType)
        {
            if (_isCombatLock)
                return;
            
            ICombatCommand newCommand = combatCommandType switch
            {
                CombatCommandType.Zoom => new CharacterZoomCombatCommand(),
                _ => null
            };

            if (newCommand == null)
            {
                Debug.LogWarning("Command is not implemented: " + combatCommandType);
                return;
            }
            
            if (newCommand.IsCombatLock)
                _isCombatLock = true;
            
            newCommand.OnFinished += OnCombatCommandFinished;
            _combatCommands.Add(newCommand);
            newCommand.Entry(_componentBank);
        }

        private void FinishCommand(IMoveCommand moveCommand)
        {
            switch (moveCommand.CommandType)
            {
                case MoveCommandType.Move:
                    PlayCommand(MoveCommandType.Fly);
                    break;
                case MoveCommandType.AimMove:
                    PlayCommand(MoveCommandType.AimFly);
                    break;
                case MoveCommandType.Jump or MoveCommandType.Fly:
                    PlayCommand(MoveCommandType.Move);
                    break;
                case MoveCommandType.AimJump or MoveCommandType.AimFly:
                    PlayCommand(MoveCommandType.AimMove);
                    break;
                default:
                    Debug.LogError("MoveCommand Finish is not implemented: " + moveCommand.CommandType);
                    break;
            }
        }

        private void OnDestroy()
        {
            Global.Instance?.UnBindUpdate(this);
            if (_inputHub != null)
            {
                _inputHub.OnMove -= OnMove;
                _inputHub.OnLeftClick -= OnLeftClick;
                _inputHub.OnRightClick -= OnRightClick;
                _inputHub.OnShiftClick -= OnShiftClick;
                _inputHub.OnSpaceClick -= OnSpaceClick;
            }
        }
    }
}
