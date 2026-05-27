using System;
using Common;
using UnityEngine;
using MoveCommandType = Common.GameDefine.MoveCommandType;
using AnimationType = Common.GameDefine.AnimationType;
using AvatarMaskType = Common.GameDefine.AvatarMaskType;
using UniRx;

namespace InGame.Component.Command
{
    public class CharacterAimJumpMoveCommand : IMoveCommand
    {
        public MoveCommandType CommandType { get; } = MoveCommandType.AimJump;
        public MoveCommandGroup? MoveCommandsGroup { get; } = MoveCommandGroup.Locomotion;
        public float Elapsed { get; private set; }
        public bool IsLanding { get; private set; }

        private ComponentBank _componentBank;
        private Action<IMoveCommand> _onMoveCommandFinished;
        private bool _isLand;
        private Vector2 _moveDirection;
        private bool _isRun;
        private bool _applyForce = true;

        private IDisposable _isLandChangedDisposable;
        private IDisposable _moveDirectionDisposable;

        private float _transferLerpTime = GameDefine.DefaultEntryRotationTime;

        public void Init(Action<IMoveCommand> onFinished, IMoveCommand transfer, ComponentBank componentBank)
        {
            _onMoveCommandFinished = onFinished;
            _componentBank = componentBank;
            if (transfer is CharacterJumpMoveCommand jump)
            {
                Elapsed = jump.Elapsed;
                IsLanding = jump.IsLanding;
                _applyForce = false;
                _transferLerpTime = 0f;
            }
        }

        public void Entry()
        {
            if (_applyForce)
            {
                _componentBank.AnimationPlayer.PlayAnimation(AnimationType.AimJump, AvatarMaskType.Base, GameDefine.DefaultJumpTime);
                _componentBank.Rigidbody.linearVelocity += new Vector3(0f, GameDefine.DefaultJumpForce, 0f);
            }
            else if (IsLanding)
                _componentBank.AnimationPlayer.PlayAnimation(AnimationType.AimLand, AvatarMaskType.Base, GameDefine.DefaultLandTime);
            _isLand = _componentBank.CharacterModel.IsLand.Value;
            _isLandChangedDisposable = _componentBank.CharacterModel.IsLand.Subscribe(OnIsLandChanged);
            _moveDirection = _componentBank.CharacterModel.MoveDirection.Value;
            _moveDirectionDisposable = _componentBank.CharacterModel.MoveDirection.Subscribe(OnMoveDirectionChanged);
            _isRun = _componentBank.CharacterModel.IsRun.Value;
        }

        public void Stay()
        {
            Elapsed += Time.deltaTime;
            if (!IsLanding)
            {
                if (Elapsed >= 0.1f && _isLand)
                {
                    _componentBank.AnimationPlayer.PlayAnimation(AnimationType.AimLand, AvatarMaskType.Base, GameDefine.DefaultLandTime);
                    IsLanding = true;
                    Elapsed = 0f;
                }

                var camForward = _componentBank.CameraController.GetForward();

                _transferLerpTime += Time.deltaTime;
                if (_transferLerpTime > GameDefine.DefaultEntryRotationTime)
                    _componentBank.Model.transform.rotation = Quaternion.LookRotation(camForward);
                else
                {
                    _componentBank.Model.transform.rotation = Quaternion.Slerp(
                        _componentBank.Model.transform.rotation,
                        Quaternion.LookRotation(camForward),
                        _transferLerpTime / GameDefine.DefaultEntryRotationTime
                    );
                }
            }
            else
            {
                if (Elapsed >= GameDefine.DefaultLandTime / 4f && _moveDirection != Vector2.zero)
                    _onMoveCommandFinished?.Invoke(this);
                else if (Elapsed >= GameDefine.DefaultLandTime && _moveDirection == Vector2.zero)
                    _onMoveCommandFinished?.Invoke(this);
            }
        }

        public void FixedStay()
        {
            if (IsLanding || _moveDirection == Vector2.zero)
                return;

            var camForward = _componentBank.CameraController.GetForward();
            var camRight = Vector3.Cross(Vector3.up, camForward);
            var worldDirection = camForward * _moveDirection.y + camRight * _moveDirection.x;
            worldDirection *= _isRun ? GameDefine.DefaultRunSpeed : GameDefine.DefaultMoveSpeed;

            var currentY = _componentBank.Rigidbody.linearVelocity.y;
            _componentBank.Rigidbody.linearVelocity = new Vector3(worldDirection.x, currentY, worldDirection.z);
        }

        public void Exit()
        {
            _isLandChangedDisposable?.Dispose();
            _moveDirectionDisposable?.Dispose();
        }

        #region Events
        private void OnIsLandChanged(bool isLand) => _isLand = isLand;
        private void OnMoveDirectionChanged(Vector2 direction) => _moveDirection = direction;
        #endregion
    }
}
