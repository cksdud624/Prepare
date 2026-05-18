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

        public enum MoveCommandType
        {
            Move,
            AimMove
        }

        public enum CombatCommandType
        {
            Fire,
            Zoom
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
            Fire,
            Aim,
            AimFire
        }

        public enum BlendTreeType
        {
            Move1D,
            AimMove2D
        }

        public enum AvatarMaskType
        {
            Base,
            Upper,
        }
        
        #endregion
        
        #region Combat

        public enum CombatState
        {
            Standard,
            Zoom
        }

        public enum WeaponType
        {
            None,
            Rifle,
            Pistol,
            Melee
        }
        #endregion
        
        #region Move
        public const float DefaultMoveSpeed = 1f;
        public const float DefaultRunSpeed = 2f;
        #endregion
        
        #region Rotation
        public const float DefaultRotationSpeed = 10f;
        public const float DefaultDragRotationSensitivity = 0.1f;
        public const float DefaultCameraPitchMin = -60f;
        public const float DefaultCameraPitchMax = 60f;
        #endregion
    }
}