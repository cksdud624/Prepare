using Common;
using UnityEngine;
using AnimationType = Common.GameDefine.AnimationType;
using MoveCommandType = Common.GameDefine.MoveCommandType;

namespace InGame.Component.Command
{
    public class CharacterWalkMoveCommand : IMoveCommand
    {
        public MoveCommandType CommandType { get; } = MoveCommandType.Walk;
        public MoveCommandGroup? MoveCommandsGroup { get; } = MoveCommandGroup.Locomotion;
        
        private ComponentBank _componentBank;
        private bool _isLocked;

        public void Entry(ComponentBank componentBank, bool isLocked)
        {
            _componentBank = componentBank;
            _isLocked = isLocked;
            if (_isLocked)
                return;
            _componentBank.AnimationPlayer.PlayAnimation(AnimationType.Walk, GameDefine.AvatarMaskType.Lower);
        }

        public void Stay()
        {
            var rb = _componentBank.Rigidbody;
            if (_isLocked)
            {
                rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
                return;
            }
            
            var input = _componentBank.InputHub.MoveDirection;
            var forward = _componentBank.CameraController.GetForward();
            var right = Vector3.Cross(Vector3.up, forward);
            var moveDir = forward * input.y + right * input.x;
            rb.linearVelocity = new Vector3(moveDir.x, rb.linearVelocity.y, moveDir.z);

            if (moveDir != Vector3.zero)
            {
                var targetRotation = Quaternion.LookRotation(moveDir);
                _componentBank.Model.transform.rotation = Quaternion.Slerp(
                    _componentBank.Model.transform.rotation,
                    targetRotation,
                    Time.deltaTime * GameDefine.DefaultRotationSpeed
                );
            }
        }

        public void FixedStay()
        {
        }

        public void Exit() { }
        public void Lock() => _isLocked = true;

        public void UnLock()
        {
            _isLocked = false;
            _componentBank.AnimationPlayer.PlayAnimation(AnimationType.Walk, GameDefine.AvatarMaskType.Lower);
        }
    }
}
