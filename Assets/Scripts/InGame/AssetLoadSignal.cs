using System;
using System.Collections.Generic;
using Common;
using Cysharp.Threading.Tasks;
using InGame.Model;
using UnityEngine;
using static Common.AssetKeys;
using static Common.GameDefine;

namespace InGame
{
    public class AssetLoadSignal : MonoBehaviour
    {
        private InGameModel _inGameModel;
        
        public void Init(InGameModel inGameModel)
        {
            _inGameModel = inGameModel;
            LoadInitAssetsAsync().Forget();
        }

        private async UniTask LoadInitAssetsAsync()
        {
            var assetManager = Global.Instance.AssetManager;
            var assetModel = _inGameModel.InGameAssetModel;
            var data = _inGameModel.InGameObjectModel.PlayerData;
            if (assetModel.GetModel(data.Id) == null)
            {
                var model = await assetManager.LoadAssetAsync<GameObject>(LoadTarget.Model, data.Id.ToString());
                assetModel.AddModel(data.Id, model);
            }

            foreach (InGameCommonAnimation anim in Enum.GetValues(typeof(InGameCommonAnimation)))
            {
                string defaultKey = "default_" + anim;
                if (assetModel.GetAnimationClip(defaultKey) == null)
                {
                    var clip = await assetManager.LoadAssetAsync<AnimationClip>(
                        LoadTarget.AnimationClip, defaultKey);
                    assetModel.AddAnimationClip(defaultKey, clip);
                }
            }
            
            _inGameModel.OnAssetInitialized?.Invoke();
        }
        
        public void Dispose()
        {
            var assetManager = Global.Instance.AssetManager;
            var assetModel = _inGameModel.InGameAssetModel;
            foreach (var key in assetModel.GetModels().Keys)
            {
                assetModel.RemoveModel(key);
                assetManager.ReleaseAsset<GameObject>(LoadTarget.Model, key.ToString());
            }
            foreach (var key in assetModel.GetAnimationClips().Keys)
            {
                assetModel.RemoveAnimationClip(key);
                assetManager.ReleaseAsset<AnimationClip>(LoadTarget.AnimationClip, key);
            }
        }
    }
}
