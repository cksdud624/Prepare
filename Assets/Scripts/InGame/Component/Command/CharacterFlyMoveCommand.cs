using System;
using Common;
using UnityEngine;
using MoveCommandType = Common.GameDefine.MoveCommandType;
using AnimationType = Common.GameDefine.AnimationType;
using BlendTreeType = Common.GameDefine.BlendTreeType;
using AvatarMaskType = Common.GameDefine.AvatarMaskType;
using UniRx;

namespace InGame.Component.Command
{
    public class CharacterFlyMoveCommand : IMoveCommand
    {
        public MoveCommandType CommandType { get; } = MoveCommandType.Fly;
        public MoveCommandGroup? MoveCommandsGroup { get; } = MoveCommandGroup.Locomotion;

        public void Init(Action<IMoveCommand> onFinished, IMoveCommand transfer)
        {
            _onMoveCommandFinished = onFinished;
            IsAscending = transfer is CharacterAimFlyMoveCommand aimFly ? aimFly.IsAscending : true;
        }

        private ComponentBank _componentBank;
        private Action<IMoveCommand> _onMoveCommandFinished;
        private Vector2 _moveDirection;
        private bool _isRun;
        private bool _isFlyHolding;
        public bool IsAscending { get; private set; }
        private bool _isLand;
        public float Elapsed { get; private set; }
        public bool IsLanding { get; private set; }

        private IDisposable _isLandChangedDisposable;
        private IDisposable _moveDirectionDisposable;
        private IDisposable _isRunDisposable;
        private IDisposable _isFlyHoldingDisposable;

        public void Entry(ComponentBank componentBank, bool isLocked)
        {
            _componentBank = componentBank;
            _componentBank.AnimationPlayer.PlayBlendTree(BlendTreeType.Fly1D, AvatarMaskType.Base);

            _moveDirection = _componentBank.CharacterModel.MoveDirection.Value;
            _isRun = _componentBank.CharacterModel.IsRun.Value;
            _isFlyHolding = _componentBank.CharacterModel.IsFlyHolding.Value;
            _moveDirectionDisposable = _componentBank.CharacterModel.MoveDirection.Subscribe(OnMoveDirectionChanged);
            _isRunDisposable = _componentBank.CharacterModel.IsRun.Subscribe(OnIsRunChanged);
            _isFlyHoldingDisposable = _componentBank.CharacterModel.IsFlyHolding.Subscribe(OnIsFlyHoldingChanged);

            _isLandChangedDisposable = _componentBank.CharacterModel.IsLand.Subscribe(OnIsLandChanged);
            
            SetParameter(IsAscending ? 1f : 0f, false);
        }

        public void Stay()
        {
            Elapsed += Time.deltaTime;

            if (!IsLanding)
            {
                var ascending = _componentBank.Rigidbody.linearVelocity.y >= 0f;
                if (ascending != IsAscending)
                {
                    IsAscending = ascending;
                    SetParameter(IsAscending ? 1f : 0f);
                }

                if (Elapsed >= 0.1f && _isLand)
                {
                    _componentBank.AnimationPlayer.PlayAnimation(AnimationType.Land, AvatarMaskType.Base, GameDefine.DefaultLandTime);
                    IsLanding = true;
                    Elapsed = 0f;
                }

                if (_moveDirection != Vector2.zero)
                {
                    var camForward = _componentBank.CameraController.GetForward();
                    var camRight = Vector3.Cross(Vector3.up, camForward);
                    var forward = new Vector3(camForward.x, 0f, camForward.z).normalized;
                    var worldDirection = forward * _moveDirection.y + camRight * _moveDirection.x;

                    if (worldDirection.sqrMagnitude > 0.001f)
                    {
                        _componentBank.Model.transform.rotation = Quaternion.Slerp(
                            _componentBank.Model.transform.rotation,
                            Quaternion.LookRotation(worldDirection),
                            GameDefine.DefaultRotationSpeed * Time.deltaTime
                        );
                    }
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
            var forward = new Vector3(camForward.x, 0f, camForward.z);
            
            if (_moveDirection != Vector2.zero)
            {
                var camRight = Vector3.Cross(Vector3.up, camForward);
                var worldDirection = forward.normalized * _moveDirection.y + camRight * _moveDirection.x;
                worldDirection *= _isRun ? GameDefine.DefaultRunSpeed : GameDefine.DefaultFlySpeed;
                _componentBank.Rigidbody.linearVelocity = new Vector3(worldDirection.x,
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
            _isRunDisposable?.Dispose();
            _isFlyHoldingDisposable?.Dispose();
            _onMoveCommandFinished = null;
        }

        public void Lock() { }
        public void UnLock() { }

        #region Events
        private void OnIsLandChanged(bool isLand) => _isLand = isLand;

        private void OnMoveDirectionChanged(Vector2 direction) => _moveDirection = direction;

        private void OnIsRunChanged(bool isRun) => _isRun = isRun;
        
        private void OnIsFlyHoldingChanged(bool isFlyHolding) => _isFlyHolding = isFlyHolding;
        #endregion

        private void SetParameter(float parameter, bool isLerp = true)
        {
            _componentBank.AnimationPlayer.SetParameter(AvatarMaskType.Base, parameter, isLerp, GameDefine.DefaultFlyBlendSpeed);
        }
    }
}
