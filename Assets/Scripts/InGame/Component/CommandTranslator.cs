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

namespace InGame.Component
{
    public class CommandTranslator : MonoBehaviour, IUpdateable
    {
        private InGameModel _inGameModel;
        private ComponentBank _componentBank;
        private InputHub _inputHub;

        private readonly List<IMoveCommand> _moveCommands = new();
        private readonly List<ICombatCommand> _combatCommands = new();
        private bool _isCombatLock = false;

        public async UniTask Init(InGameModel inGameModel, InputHub inputHub, ComponentBank componentBank)
        {
            _inGameModel = inGameModel;
            _componentBank = componentBank;
            _inputHub = inputHub;
            _inputHub.OnMove += OnMove;
            _inputHub.OnLeftClick += OnLeftClick;
            _inputHub.OnRightClick += OnRightClick;
            
            Global.Instance.BindUpdate(this);

            PlayCommand(GameDefine.MoveCommandType.Idle);
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
            Debug.Log("OnRightClick: " + isClick);
        }

        public void OnUpdate()
        {
            foreach (var moveCommand in _moveCommands)
                moveCommand.Stay();

            foreach (var combatCommand in _combatCommands)
                combatCommand.Stay();

            for (int i = _combatCommands.Count - 1; i >= 0; i--)
            {
                if (_combatCommands[i].IsFinished)
                {
                    if(_combatCommands[i].IsLock)
                        _isCombatLock = false;
                    _combatCommands[i].Exit();
                    _combatCommands.RemoveAt(i);
                }
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

            if (newCommand.ExclusiveGroup.HasValue)
            {
                for (int i = _moveCommands.Count - 1; i >= 0; i--)
                {
                    if (_moveCommands[i].ExclusiveGroup == newCommand.ExclusiveGroup)
                    {
                        _moveCommands[i].Exit();
                        _moveCommands.RemoveAt(i);
                    }
                }
            }

            _moveCommands.Add(newCommand);
            newCommand.Entry(_componentBank);
        }

        private void PlayCommand(CombatCommandType combatCommandType)
        {
            if (_isCombatLock)
                return;
            
            ICombatCommand newCommand = combatCommandType switch
            {
                CombatCommandType.Fire => new CharacterFireCombatCommand(),
                _ => null
            };

            if (newCommand == null)
            {
                Debug.LogWarning("Command is not implemented: " + combatCommandType);
                return;
            }
            
            if(newCommand.IsLock)
                _isCombatLock = true;
            
            _combatCommands.Add(newCommand);
            newCommand.Entry(_componentBank);
        }

        private void OnDestroy()
        {
            Global.Instance?.UnBindUpdate(this);
            if (_inputHub != null)
                _inputHub.OnMove -= OnMove;
        }
    }
}
