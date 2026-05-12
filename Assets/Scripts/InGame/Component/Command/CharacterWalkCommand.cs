using Common;
using UnityEngine;
using AnimationType = Common.GameDefine.AnimationType;

namespace InGame.Component.Command
{
    public class CharacterWalkMoveCommand : IMoveCommand
    {
        private ComponentBank _componentBank;

        public MoveCommandGroup? ExclusiveGroup => MoveCommandGroup.Locomotion;

        public void Entry(ComponentBank componentBank)
        {
            _componentBank = componentBank;
            componentBank.AnimationPlayer.PlayAnimation(AnimationType.Walk);
        }

        public void Stay()
        {
            var input = _componentBank.InputHub.MoveDirection;
            var forward = _componentBank.CameraController.GetForward();
            var right = Vector3.Cross(Vector3.up, forward);
            var moveDir = forward * input.y + right * input.x;

            var rb = _componentBank.Rigidbody;
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

        public void Exit() { }
    }
}
