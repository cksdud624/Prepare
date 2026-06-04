using System.Collections.Generic;
using System.Data;
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
        private readonly List<IMoveCommand> _removePendingMove = new();
        private readonly List<ICombatCommand> _removePendingCombat = new();
        
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
            PlayCommand(CombatCommandType.Move);
            await UniTask.CompletedTask;
        }

        #region Events
        private void OnMove(Vector2 direction) => _componentBank.CharacterModel.MoveDirection.Value = direction;

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
                    ChangeCombatState(CombatState.Aim);
                    break;
                case CombatState.Aim:
                    ChangeCombatState(CombatState.Standard);
                    break;
            }
        }

        private void OnShiftClick(bool isClick)
        {
            _componentBank.CharacterModel.IsSprint.Value = isClick;
            if (isClick && _componentBank.CharacterModel.CombatState.Value is CombatState.Aim)
                ChangeCombatState(CombatState.Standard);
        }

        private void OnSpaceClick(bool isClick)
        {
            _componentBank.CharacterModel.IsSpaceHolding.Value = isClick;
            
            if (!isClick)
                return;

            IMoveCommand flyCommand = null;
            IMoveCommand jumpCommand = null;
            foreach (var command in _moveCommands)
            {
                if (command.MoveCommandsGroup is MoveCommandGroup.Locomotion)
                {
                    switch (command.CommandType)
                    {
                        case MoveCommandType.Fly or MoveCommandType.AimFly:
                            flyCommand = command;
                            break;
                        case MoveCommandType.Jump or MoveCommandType.AimJump:
                            jumpCommand = command;
                            break;
                    }
                }
            }

            if (flyCommand != null)
                return;
            if (jumpCommand != null)
            {
                switch (_componentBank.CharacterModel.CombatState.Value)
                {
                    case CombatState.Standard:
                        PlayCommand(MoveCommandType.Fly);
                        break;
                    case CombatState.Aim:
                        PlayCommand(MoveCommandType.AimFly);
                        break;
                }
            }
            else if (_componentBank.CharacterModel.IsLand.Value)
            {
                switch (_componentBank.CharacterModel.CombatState.Value)
                {
                    case CombatState.Standard:
                        PlayCommand(MoveCommandType.Jump);
                        break;
                    case CombatState.Aim:
                        PlayCommand(MoveCommandType.AimJump);
                        break;
                }
            }

        }

        public void OnUpdate()
        {
            foreach (var moveCommand in _moveCommands)
                moveCommand.Stay();
            foreach (var combatCommand in _combatCommands)
                combatCommand.Stay();

            if (_removePendingMove.Count > 0)
            {
                foreach (var moveCommand in _removePendingMove)
                    FinishCommand(moveCommand);
                _removePendingMove.Clear();
            }

            if (_removePendingCombat.Count > 0)
            {
                foreach (var combatCommand in _removePendingCombat)
                    FinishCommand(combatCommand);
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
                foreach (var moveCommand in _removePendingMove)
                    FinishCommand(moveCommand);
                _removePendingMove.Clear();
            }

            if (_removePendingCombat.Count > 0)
            {
                foreach (var combatCommand in _removePendingCombat)
                    FinishCommand(combatCommand);
                _removePendingCombat.Clear();
            }
        }
        
        private void OnMoveCommandFinished(IMoveCommand moveCommand) => _removePendingMove.Add(moveCommand);

        private void OnCombatCommandFinished(ICombatCommand command) => _removePendingCombat.Add(command);
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
            
            newCommand.Init(OnMoveCommandFinished, transfer, _componentBank);
            _moveCommands.Add(newCommand);
            newCommand.Entry();
        }

        private void PlayCommand(CombatCommandType combatCommandType, ICombatCommand transfer = null)
        {
            ICombatCommand newCommand = combatCommandType switch
            {
                CombatCommandType.Move   => new CharacterMoveCombatCommand(),
                CombatCommandType.Air    => new CharacterAirCombatCommand(),
                CombatCommandType.Aim    => new CharacterAimCombatCommand(),
                _ => null
            };

            if (newCommand == null)
            {
                Debug.LogWarning("Command is not implemented: " + combatCommandType);
                return;
            }

            if (newCommand.CombatCommandsGroup.HasValue)
            {
                foreach (var command in _combatCommands)
                {
                    if (newCommand.CombatCommandsGroup == command.CombatCommandsGroup)
                    {
                        command.Exit();
                        _combatCommands.Remove(command);
                        break;
                    }
                }
            }
            
            newCommand.Init(OnCombatCommandFinished, transfer, _componentBank);
            _combatCommands.Add(newCommand);
            newCommand.Entry();
        }
        
        private void FinishCommand(IMoveCommand moveCommand)
        {
            moveCommand.Exit();
            _moveCommands.Remove(moveCommand);

            if (_componentBank.CharacterModel.IsLand.Value)
            {
                switch(_componentBank.CharacterModel.CombatState.Value)
                {
                    case CombatState.Standard:
                        PlayCommand(MoveCommandType.Move);
                        break;
                    case CombatState.Aim:
                        PlayCommand(MoveCommandType.AimMove);
                        break;
                }
            }
            else
            {
                switch(_componentBank.CharacterModel.CombatState.Value)
                {
                    case CombatState.Standard:
                        PlayCommand(MoveCommandType.Fly);
                        break;
                    case CombatState.Aim:
                        PlayCommand(MoveCommandType.AimFly);
                        break;
                }
            }
        }
        
        private void FinishCommand(ICombatCommand combatCommand)
        {
            combatCommand.Exit();
            _combatCommands.Remove(combatCommand);
            
            RefreshCombatCommand();
        }

        private void RefreshCombatCommand()
        {
            switch (_componentBank.CharacterModel.CombatState.Value)
            {
                case CombatState.Standard:
                    PlayCommand(_componentBank.CharacterModel.IsLand.Value
                        ? CombatCommandType.Move
                        : CombatCommandType.Air);
                    break;
                case CombatState.Aim:
                    PlayCommand(CombatCommandType.Aim);
                    break;
            }
        }

        private void ChangeCombatState(CombatState combatState)
        {
            _componentBank.CharacterModel.CombatState.Value = combatState;
            switch (combatState)
            {
                case CombatState.Standard:
                    foreach (var command in _moveCommands)
                    {
                        if (command.MoveCommandsGroup != MoveCommandGroup.Locomotion) continue;
                        switch (command.CommandType)
                        {
                            case MoveCommandType.AimMove:  PlayCommand(MoveCommandType.Move, command);  break;
                            case MoveCommandType.AimJump:  PlayCommand(MoveCommandType.Jump, command); break;
                            case MoveCommandType.AimFly:   PlayCommand(MoveCommandType.Fly, command);  break;
                        }
                        break;
                    }
                    break;
                case CombatState.Aim:
                    foreach (var command in _moveCommands)
                    {
                        if (command.MoveCommandsGroup != MoveCommandGroup.Locomotion) continue;
                        switch (command.CommandType)
                        {
                            case MoveCommandType.Move:  PlayCommand(MoveCommandType.AimMove, command);  break;
                            case MoveCommandType.Jump:  PlayCommand(MoveCommandType.AimJump, command); break;
                            case MoveCommandType.Fly:   PlayCommand(MoveCommandType.AimFly, command);  break;
                        }
                        break;
                    }
                    break;
            }
            RefreshCombatCommand();
        }

        private void OnDestroy()
        {
            Global.Instance?.UnBindUpdate(this);
            Global.Instance?.UnBindFixedUpdate(this);
            
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
