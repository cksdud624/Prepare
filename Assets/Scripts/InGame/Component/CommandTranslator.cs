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

namespace InGame.Component
{
    public class CommandTranslator : MonoBehaviour, IUpdateable, IFixedUpdateable
    {
        private InGameModel _inGameModel;
        private ComponentBank _componentBank;
        private InputHub _inputHub;

        private readonly List<IMoveCommand> _moveCommands = new();
        private readonly List<ICombatCommand> _combatCommands = new();
        private readonly List<ICombatCommand> _removePending = new();
        private readonly Dictionary<MoveCommandGroup, int> _lockedMoveGroups = new();
        private bool _isCombatLock;

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
                if (moveCommand.CommandType == MoveCommandType.Move)
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
            
            
            foreach (var command in _combatCommands)
            {
                if (command.CommandType == CombatCommandType.Zoom)
                {
                    command.Exit();
                    _combatCommands.Remove(command);
                    PlayCommand(MoveCommandType.Move);
                    return;
                }
            }
            PlayCommand(CombatCommandType.Zoom);
            PlayCommand(MoveCommandType.AimMove);
        }

        private void OnShiftClick(bool isClick)
        {
            _componentBank.CharacterModel.IsRun.Value = isClick;
        }

        private void OnSpaceClick(bool isClick)
        {
            if (!isClick)
                return;
            Debug.Log("점프");
        }

        public void OnUpdate()
        {
            foreach (var moveCommand in _moveCommands)
                moveCommand.Stay();
            foreach (var combatCommand in _combatCommands)
                combatCommand.Stay();
            
            if (_removePending.Count > 0)
            {
                foreach (var command in _removePending)
                {
                    if (command.IsCombatLock)
                        _isCombatLock = false;
                    command.Exit();
                    _combatCommands.Remove(command);
                }
                _removePending.Clear();
            }
        }

        public void OnFixedUpdate()
        {
            foreach (var moveCommand in _moveCommands)
                moveCommand.FixedStay();
            foreach (var combatCommand in _combatCommands)
                combatCommand.FixedStay();
            
            if (_removePending.Count > 0)
            {
                foreach (var command in _removePending)
                {
                    if (command.IsCombatLock)
                        _isCombatLock = false;
                    command.Exit();
                    _combatCommands.Remove(command);
                }
                _removePending.Clear();
            }
        }
        #endregion

        private void PlayCommand(MoveCommandType moveCommandType)
        {
            IMoveCommand newCommand = moveCommandType switch
            {
                MoveCommandType.Move => new CharacterMoveMoveCommand(),
                MoveCommandType.AimMove => new CharacterAimMoveMoveCommand(),
                _ => null
            };

            if (newCommand == null)
            {
                Debug.LogWarning("Command is not implemented: " + moveCommandType);
                return;
            }

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
            
            /*
             일단 락기능은 비활성화
            if (newCommand.LockedMoveGroups is { Length: > 0 })
            {
                foreach (var lockGroup in newCommand.LockedMoveGroups)
                {
                    _lockedMoveGroups.TryAdd(lockGroup, 0);
                    _lockedMoveGroups[lockGroup]++;
                    
                    foreach (var moveCommand in _moveCommands)
                    {
                        if(lockGroup == moveCommand.MoveCommandsGroup)
                            moveCommand.Lock();
                    }
                }
            }
            */

            newCommand.OnFinished += OnCombatCommandFinished;
            _combatCommands.Add(newCommand);
            newCommand.Entry(_componentBank);
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
            _removePending.Add(command);
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
