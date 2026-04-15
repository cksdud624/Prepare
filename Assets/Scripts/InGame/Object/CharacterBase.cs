using System;
using InGame.Animation;
using UnityEngine;
using System.Collections.Generic;
using InGame.Model;
using Unity.VisualScripting;
using static Common.GameDefine;

namespace InGame.Object
{
    public class CharacterBase : ObjectBase
    {
        
        #region Object Management
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
            var playerData = inGameModel.InGameObjectModel.PlayerData;
            var model = inGameModel.InGameAssetModel.GetModel(inGameModel.InGameObjectModel.PlayerData.Id);
            Model = Instantiate(model, this.transform);
            var modelCollider = model.GetComponent<Collider>();
            if (modelCollider != null)
                Collider = modelCollider;

            Rigidbody = gameObject.AddComponent<Rigidbody>();
            Rigidbody.constraints = RigidbodyConstraints.FreezeAll & ~RigidbodyConstraints.FreezePositionY;
            
            //애니메이션 클립 가져오기
            var customClips = playerData.CustomAnimation.ToHashSet();
            Dictionary<InGameCommonAnimation, AnimationClip> animationClips = new ();
            var assetModel = inGameModel.InGameAssetModel;
            foreach (InGameCommonAnimation anim in Enum.GetValues(typeof(InGameCommonAnimation)))
            {
                string animName = anim.ToString();
                string key;
                if (customClips.Contains(animName))
                    key = playerData.Id + "_" + animName;
                else
                    key = "default_"  + animName;
                var clip = assetModel.GetAnimationClip(key);
                if(clip == null)
                    Debug.LogError(key + " is not a valid animation clip");
                else
                    animationClips.Add(anim, clip);
            }
            
            AnimationPlayer = gameObject.AddComponent<AnimationPlayer>();
            AnimationPlayer.Init(Model, Model.GetComponent<Animator>(), animationClips);
            AnimationPlayer.PlayAnimation(InGameCommonAnimation.Idle);
        }
        #endregion
        
        private AnimationPlayer AnimationPlayer { get; set; }
    }
}
