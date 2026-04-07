using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Table
{
    public class RecordBase
    {
        protected virtual string TableName => string.Empty;

        public async UniTask LoadRecord()
        {
            string key = "Assets/Tables/" + TableName + ".bytes";
            var handle = Addressables.LoadAssetAsync<TextAsset>(key);
            await handle.Task;
            if (handle.Status == AsyncOperationStatus.Succeeded)
                SetRecord(handle.Result);
            else
                Debug.LogError($"{TableName} has error");
        }

        protected virtual void SetRecord(TextAsset result)
        {
            
        }
    }
}
