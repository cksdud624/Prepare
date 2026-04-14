using System.Collections.Generic;
using Common;
using Cysharp.Threading.Tasks;
using InGame.Model;
using UnityEngine;
using static Common.AssetKeys;

namespace InGame
{
    public class AssetLoadSignal : MonoBehaviour
    {
        private InGameModel _inGameModel;

        private Dictionary<long, GameObject> _modelCache = new();
        
        public void Init(InGameModel inGameModel)
        {
            _inGameModel = inGameModel;
            LoadInitAssetsAsync().Forget();
        }

        private async UniTask LoadInitAssetsAsync()
        {
            var assetModel = _inGameModel.InGameAssetModel;
            var data = _inGameModel.InGameObjectModel.PlayerData;
            if (!_modelCache.ContainsKey(data.Id))
            {
                var model = await Global.Instance.AssetManager.LoadAssetAsync<GameObject>(LoadTarget.Model, data.Id);
                assetModel.AddModel(data.Id, model);
            }
            
            _inGameModel.OnAssetInitialized?.Invoke();
        }
        
        public void Dispose()
        {
            var assetManager = Global.Instance.AssetManager;
            var assetModel = _inGameModel.InGameAssetModel;
            foreach (var key in _modelCache.Keys)
            {
                assetModel.RemoveModel(key);
                assetManager.ReleaseAsset<GameObject>(LoadTarget.Model, key);
            }
            _modelCache.Clear();
        }
    }
}
