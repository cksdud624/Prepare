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
            Hub = new();
            Hub.isPlayer = isPlayer;
            inGameModel = model;
            CharacterData = characterData;
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
            var model = inGameModel.InGameAssetModel.GetModel(inGameModel.InGameObjectModel.PlayerData.Id);
            Hub.Model = Instantiate(model, this.transform);
            var modelCollider = model.GetComponent<Collider>();
            if (modelCollider != null)
                Hub.Collider = modelCollider;

            Hub.Rigidbody = gameObject.AddComponent<Rigidbody>();
            Hub.Rigidbody.constraints = RigidbodyConstraints.FreezeAll & ~RigidbodyConstraints.FreezePositionY;
            
            var customClips = CharacterData.CustomAnimation.ToHashSet();
            Dictionary<InGameCommonAnimation, AnimationClip> animationClips = new ();
            var assetModel = inGameModel.InGameAssetModel;
            foreach (InGameCommonAnimation anim in Enum.GetValues(typeof(InGameCommonAnimation)))
            {
                string animName = anim.ToString();
                string key;
                if (customClips.Contains(animName))
                    key = CharacterData.Id + "_" + animName;
                else
                    key = "default_"  + animName;
                var clip = assetModel.GetAnimationClip(key);
                if(clip == null)
                    Debug.LogError(key + " is not a valid animation clip");
                else
                    animationClips.Add(anim, clip);
            }
            
            Hub.AnimationPlayer = gameObject.AddComponent<AnimationPlayer>();
            Hub.AnimationPlayer.Init(Hub.Model, Hub.Model.GetComponent<Animator>(), animationClips);
            Hub.AnimationPlayer.PlayAnimation(InGameCommonAnimation.Idle);

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
        private CharacterData CharacterData { get; set; }
    }
}
