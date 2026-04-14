using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using static Common.AssetKeys;

namespace Common
{
    public class AssetManager : MonoBehaviour
    {
        private Dictionary<string, List<AsyncOperationHandle>> _addressableCache = new ();
        
        public async UniTask<T> LoadAssetAsync<T>(LoadTarget target, long id) where T : Object
        {
            string key = GetAddressableKey(target, id);
            var handle = Addressables.LoadAssetAsync<T>(key);
            T asset = await handle.ToUniTask();
            if(asset == null)
                Debug.LogError($"{key} not found");
            else
            {
                if(!_addressableCache.ContainsKey(key))
                    _addressableCache[key] = new List<AsyncOperationHandle>();
                _addressableCache[key].Add(handle);
            }
            return asset;
        }

        public void ReleaseAsset<T>(LoadTarget target, long id) where T : Object
        {
            string key = GetAddressableKey(target, id);
            if (_addressableCache.TryGetValue(key, out var list) && list.Count > 0)
            {
                var handle = list[^1];
                Addressables.Release(handle);
                list.RemoveAt(list.Count - 1);
                if(list.Count == 0)
                    _addressableCache.Remove(key);
            }
            else
                Debug.LogError($"{key} not found to release");
        }
    }

    public static class AssetKeys
    {
        public enum LoadTarget
        {
            Model
        }

        private const string Model = "Assets/AddressableAssets/Prefab/Model/";

        public static string GetAddressableKey(LoadTarget target, long id)
        {
            string key;
            switch (target)
            {
                case  LoadTarget.Model:
                    key = Model + id + ".prefab";
                    break;
                default:
                    key = string.Empty;
                    break;
            }
            
            return key;
        }
    }
}