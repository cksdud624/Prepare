using System;
using InGame.Animation;
using UnityEngine;
using System.Collections.Generic;
using Generated.Table;
using InGame.Controller;
using InGame.Model;
using Unity.VisualScripting;
using UnityEngine.TextCore.Text;
using static Common.GameDefine;

namespace InGame.Object
{
    public class CharacterBase : ObjectBase
    {
        
        #region Object Management
        public void Init(InGameModel model, CharacterData characterData ,bool isPlayer = false)
        {
            inGameModel = model;
            Hub = new();
            Hub.isPlayer = isPlayer;
            Hub.CharacterData = characterData;
            AddObject();
            AddParts();
            Hub.State = ObjectState.Ready;
        }
        protected override void AddObject()
        {
            inGameModel.InGameObjectModel.AddCharacter(this);
        }

        protected override void OnDestroy()
        {
            inGameModel.InGameObjectModel.RemoveCharacter(this);
        }
        #endregion
        
        #region Components
        protected override void AddParts()
        {
            //플레이어 모델
            var model = inGameModel.InGameAssetModel.GetModel(Hub.CharacterData.Id);
            Hub.Model = Instantiate(model, transform);
            
            //콜라이더
            Hub.MoveCollider = gameObject.AddComponent<CapsuleCollider>();
            Hub.MoveCollider.center = Hub.CharacterData.MoveColliderCenter;
            Hub.MoveCollider.radius = Hub.CharacterData.MoveColliderRadius;
            Hub.MoveCollider.height = Hub.CharacterData.MoveColliderHeight;
            
            //리지드바디
            Hub.Rigidbody = gameObject.AddComponent<Rigidbody>();
            Hub.Rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
            
            //커스텀 애니메이션 클립 => 기본 InGameCommonAnimation으로 명시되어있는 애니메이션들을 커스텀 가능
            var customClips = Hub.CharacterData.CustomAnimation.ToHashSet();
            Dictionary<InGameCommonAnimation, AnimationClip> animationClips = new ();
            var assetModel = inGameModel.InGameAssetModel;
            foreach (InGameCommonAnimation anim in Enum.GetValues(typeof(InGameCommonAnimation)))
            {
                string key;
                string animName = anim.ToString();
                if (customClips.Contains(animName))
                    key = Hub.CharacterData.Id + "_" + animName;
                else
                    key = "default_"  + animName;
                var clip = assetModel.GetAnimationClip(key);
                if(clip == null)
                    Debug.LogError(key + " is not a valid animation clip");
                else
                    animationClips.Add(anim, clip);
            }
            
            Hub.AnimationPlayer = gameObject.AddComponent<AnimationPlayer>();
            Hub.AnimationPlayer.Init(Hub.Model);
            //Hub.AnimationPlayer.PlayAnimation(InGameCommonAnimation.Idle);

            if (Hub.isPlayer)
            {
                Hub.Controller = gameObject.AddComponent<ControllerPlayer>();
            }
        }
        #endregion

        private new CharacterHub Hub
        {
            get => (CharacterHub)base.Hub;
            set => base.Hub = value;
        }
    }
}
