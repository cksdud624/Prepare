using UnityEngine;

namespace Common
{
    public static class GameDefine
    {
        public enum SceneType
        {
            BootStrap = 0,
            Main = 1
        }

        public enum ObjectType
        {
            Object,
            Character
        }

        public enum ObjectState
        {
            Raw,
            Loading,
            Ready,
            Playing,
            Sleep,
            Error
        }

        public static readonly Vector3 DefaultPlayerSight = new (0, 1.6f, 0);
        
        #region Collider
        public static readonly Vector3 DefaultColliderCenter = new(0, 0.9f, 0);
        public const float DefaultColliderRadius = 0.3f;
        public const float DefaultColliderHeight = 1.8f;
        #endregion
        
        #region Animation
        public static readonly int Parameter1 = Animator.StringToHash("Parameter1");
        public static readonly int Parameter2 = Animator.StringToHash("Parameter2");
        public const float DefaultCrossFadeDuration = 0.1f;
        public enum AnimationType
        {
            //점프 애니메이션
            Jump,
            Land,
            //조준 점프 애니메이션
            AimJump,
            AimLand,
        }

        public enum BlendTreeType
        {
            Move1D,
            AimMove2D,
            Fly1D,
            AimFly1D,
        }

        public enum AvatarMaskType
        {
            Base,
            Upper,
        }
        
        public enum WeaponAnimationType
        {
            Idle,
            Aim
        }
        
        #endregion
        
        #region Combat
        public enum CombatCommandType
        {
            Idle,
            Aim
        }

        public enum CombatState
        {
            Standard,
            Aim
        }

        public enum WeaponType
        {
            Pistol
        }
        #endregion
        
        #region Move
        public enum MoveCommandType
        {
            Move,
            Jump,
            AimMove,
            AimJump,
            Fly,
            AimFly,
        }

        public const float DefaultMoveSpeed = 1f;
        public const float DefaultRunSpeed = 2f;
        public const float DefaultJumpTime = 0.4f;
        public const float DefaultJumpForce = 5f;
        public const float DefaultLandTime = 0.4f;
        public const float DefaultFlySpeed = 3f;
        public const float DefaultFlyBlendSpeed = 0.5f;
        public const float DefaultFallToFlyTime = 1f;
        #endregion
        
        #region Rotation

        public const float DefaultCameraDistance = 3f;
        public const float DefaultZoomCameraDistance = 2f;
        public const float DefaultZoomCameraShoulderOffsetX = 0.5f;
        public const float DefaultCameraLerpTime = 0.1f;
        public const float DefaultEntryRotationTime = 0.1f;
        public const float DefaultRotationSpeed = 10f;
        public const float DefaultDragRotationSensitivity = 0.1f;
        public const float DefaultCameraPitchMin = -30f;
        public const float DefaultCameraPitchMax = 30f;
        #endregion
    }
}