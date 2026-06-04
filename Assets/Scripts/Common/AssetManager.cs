using System;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using static Common.AssetKeys;
using Object = UnityEngine.Object;

namespace Common
{
    public class AssetManager : MonoBehaviour
    {
        private readonly Dictionary<string, List<AsyncOperationHandle>> _addressableCache = new ();
        
        public async UniTask<T> LoadAssetAsync<T>(LoadTarget target, string assetName) where T : Object
        {
            string key = GetAddressableKey(target, assetName);
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

        public void ReleaseAsset<T>(LoadTarget target, string assetName) where T : Object
        {
            string key = GetAddressableKey(target, assetName);
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
            Model,
            AnimationClip,
            AvatarMask,
            BlendTree,
            WeaponAnimationClip,
            WeaponBlendTree
        }

        private const string Model = "Assets/AddressableAssets/Prefab/Model/";
        private const string AnimationClip = "Assets/AddressableAssets/AnimationClip/";
        private const string AvatarMask = "Assets/AddressableAssets/AvatarMask/";
        private const string BlendTree = "Assets/AddressableAssets/BlendTree/";
        private const string WeaponAnimationClip = "Assets/AddressableAssets/AnimationClip/Weapon/";
        private const string WeaponBlendTree = "Assets/AddressableAssets/BlendTree/Weapon/";

        public static string GetAddressableKey(LoadTarget target, string assetName)
        {
            string key;
            switch (target)
            {
                case LoadTarget.Model:
                    key = Model + assetName + ".prefab";
                    break;
                case LoadTarget.AnimationClip:
                    key = AnimationClip + assetName + ".anim";
                    break;
                case LoadTarget.AvatarMask:
                    key = AvatarMask + assetName + ".mask";
                    break;
                case LoadTarget.BlendTree:
                    key = BlendTree + assetName + ".controller";
                    break;
                case LoadTarget.WeaponAnimationClip:
                    key = WeaponAnimationClip + assetName + ".anim";
                    break;
                case LoadTarget.WeaponBlendTree:
                    key = WeaponBlendTree + assetName + ".controller";
                    break;
                default:
                    key = string.Empty;
                    break;
            }

            return key;
        }
    }
}