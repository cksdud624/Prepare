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
            
            Global.Instance.BindUpdate(this);
            Global.Instance.BindFixedUpdate(this);

            PlayCommand(MoveCommandType.Idle);
            await UniTask.CompletedTask;
        }

        #region Events

        private void OnMove(Vector2 direction)
        {
            PlayCommand(direction == Vector2.zero ? MoveCommandType.Idle : MoveCommandType.Walk);
        }

        public void OnLeftClick(bool isClick)
        {
            if(isClick)
                PlayCommand(CombatCommandType.Fire);
        }

        public void OnRightClick(bool isClick)
        {
            var characterModel = _componentBank.CharacterModel;
            if (isClick && characterModel.CombatState is CombatState.Standard)
                PlayCommand(CombatCommandType.Aim);
            else if(isClick && characterModel.CombatState is CombatState.Aim)
                Debug.Log("아잇!");
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
                MoveCommandType.Idle => new CharacterIdleMoveCommand(),
                MoveCommandType.Walk => new CharacterWalkMoveCommand(),
                _ => null
            };

            if (newCommand == null)
            {
                Debug.LogWarning("Command is not implemented: " + moveCommandType);
                return;
            }

            bool isLocked = false;
            if (newCommand.MoveCommandsGroup.HasValue)
            {
                for (int i = _moveCommands.Count - 1; i >= 0; i--)
                {
                    if (_moveCommands[i].MoveCommandsGroup == newCommand.MoveCommandsGroup)
                    {
                        _moveCommands[i].Exit();
                        _moveCommands.RemoveAt(i);
                    }
                }

                if (_lockedMoveGroups.ContainsKey(newCommand.MoveCommandsGroup.Value))
                    isLocked = true;
            }

            _moveCommands.Add(newCommand);
            newCommand.Entry(_componentBank, isLocked);
        }

        private void PlayCommand(CombatCommandType combatCommandType)
        {
            if (_isCombatLock)
                return;
            
            ICombatCommand newCommand = combatCommandType switch
            {
                CombatCommandType.Fire => new CharacterFireCombatCommand(),
                CombatCommandType.Aim => new CharacterAimCombatCommand(),
                _ => null
            };

            if (newCommand == null)
            {
                Debug.LogWarning("Command is not implemented: " + combatCommandType);
                return;
            }
            
            if (newCommand.IsCombatLock)
                _isCombatLock = true;

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

            newCommand.OnFinished += OnCombatCommandFinished;
            _combatCommands.Add(newCommand);
            newCommand.Entry(_componentBank);
        }

        private void OnCombatCommandFinished(ICombatCommand command)
        {
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
            
            _removePending.Add(command);
        }

        private void OnDestroy()
        {
            Global.Instance?.UnBindUpdate(this);
            if (_inputHub != null)
                _inputHub.OnMove -= OnMove;
        }
    }
}
