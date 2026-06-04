using System;
using Common;
using UnityEngine;
using MoveCommandType = Common.GameDefine.MoveCommandType;
using AnimationType = Common.GameDefine.AnimationType;
using AvatarMaskType = Common.GameDefine.AvatarMaskType;
using UniRx;

namespace InGame.Component.Command
{
    public class CharacterAimFlyMoveCommand : IMoveCommand
    {
        public MoveCommandType CommandType { get; } = MoveCommandType.AimFly;
        public MoveCommandGroup? MoveCommandsGroup { get; } = MoveCommandGroup.Locomotion;

        public void Init(Action<IMoveCommand> onFinished, IMoveCommand transfer, ComponentBank componentBank)
        {
            _onMoveCommandFinished = onFinished;
            _componentBank = componentBank;
            _transferLerpTime = transfer is CharacterFlyMoveCommand ? 0f : GameDefine.DefaultEntryRotationTime;
        }

        private ComponentBank _componentBank;
        private Action<IMoveCommand> _onMoveCommandFinished;
        private Vector2 _moveDirection;
        private bool _isSprint;
        private bool _isFlyHolding;
        private bool _isLand;
        public float Elapsed { get; private set; }
        public bool IsLanding { get; private set; }

        private float _transferLerpTime;
        private IDisposable _isLandChangedDisposable;
        private IDisposable _moveDirectionDisposable;
        private IDisposable _isSprintDisposable;
        private IDisposable _isFlyHoldingDisposable;

        public void Entry()
        {
            _componentBank.AnimationPlayer.PlayAnimation(AnimationType.AimFly, AvatarMaskType.Base);

            _moveDirection = _componentBank.CharacterModel.MoveDirection.Value;
            _isSprint = _componentBank.CharacterModel.IsSprint.Value;
            _isFlyHolding = _componentBank.CharacterModel.IsSpaceHolding.Value;
            _moveDirectionDisposable = _componentBank.CharacterModel.MoveDirection.Subscribe(OnMoveDirectionChanged);
            _isSprintDisposable = _componentBank.CharacterModel.IsSprint.Subscribe(v => _isSprint = v);
            _isFlyHoldingDisposable = _componentBank.CharacterModel.IsSpaceHolding.Subscribe(v => _isFlyHolding = v);
            _isLandChangedDisposable = _componentBank.CharacterModel.IsLand.Subscribe(OnIsLandChanged);

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
                var horizontalForward = new Vector3(camForward.x, 0f, camForward.z);
                if (horizontalForward.sqrMagnitude > 0.001f)
                {
                    _transferLerpTime += Time.deltaTime;
                    if (_transferLerpTime >= GameDefine.DefaultEntryRotationTime)
                        _componentBank.Model.transform.rotation = Quaternion.LookRotation(horizontalForward);
                    else
                        _componentBank.Model.transform.rotation = Quaternion.Slerp(
                            _componentBank.Model.transform.rotation,
                            Quaternion.LookRotation(horizontalForward),
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
            if (IsLanding)
                return;

            var camForward = _componentBank.CameraController.GetForward();
            var horizontalForward = new Vector3(camForward.x, 0f, camForward.z);

            if (_moveDirection != Vector2.zero)
            {
                var camRight = Vector3.Cross(Vector3.up, camForward);
                var worldDirection = horizontalForward.normalized * _moveDirection.y + camRight * _moveDirection.x;
                worldDirection *= _isSprint ? GameDefine.DefaultRunSpeed : GameDefine.DefaultFlySpeed;
                _componentBank.Rigidbody.linearVelocity = new Vector3(
                    worldDirection.x,
                    _isFlyHolding ? GameDefine.DefaultFlySpeed : _componentBank.Rigidbody.linearVelocity.y,
                    worldDirection.z);
            }
            else
            {
                var vel = _componentBank.Rigidbody.linearVelocity;
                _componentBank.Rigidbody.linearVelocity = new Vector3(
                    vel.x,
                    _isFlyHolding ? GameDefine.DefaultFlySpeed : vel.y,
                    vel.z);
            }
        }

        public void Exit()
        {
            _isLandChangedDisposable?.Dispose();
            _moveDirectionDisposable?.Dispose();
            _isSprintDisposable?.Dispose();
            _isFlyHoldingDisposable?.Dispose();
            _onMoveCommandFinished = null;
        }

        #region Events
        private void OnIsLandChanged(bool isLand) => _isLand = isLand;

        private void OnMoveDirectionChanged(Vector2 direction) => _moveDirection = direction;
        #endregion

    }
}
