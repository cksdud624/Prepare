using Cysharp.Threading.Tasks;
using UnityEngine;
using LoadTarget = Common.AssetKeys.LoadTarget;

namespace Common
{
    public class AssetManager : MonoBehaviour
    {
        public async UniTask LoadAssetAsync(LoadTarget target, long id)
        {
            
        }
    }

    public static class AssetKeys
    {
        public enum LoadTarget
        {
            Model
        }
        
        //public const string 
    }
}