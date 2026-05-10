using Common;
using Cysharp.Threading.Tasks;
using Generated.Table;
using InGame.Model;
using UnityEngine;
using static Common.AssetKeys;

namespace InGame.Component
{
    public class ComponentBank : MonoBehaviour
    {
        private InGameModel _inGameModel;
        private CharacterData _characterData;
        
        public GameObject Model { get; private set; }
        public Animator Animator { get; private set; }
        
        public async UniTask Init(InGameModel inGameModel, CharacterData characterData)
        {
            _inGameModel = inGameModel;
            _characterData = characterData;
            
            var assetManager = Global.Instance.AssetManager;
            var model = await assetManager.LoadAssetAsync<GameObject>(LoadTarget.Model, _characterData.Id.ToString());
            if (model == null)
            {
                Debug.LogWarning("Model not found: " + _characterData.Id);
                return;
            }
            Model = Instantiate(model, transform);
            Model.transform.position = Vector3.zero;
            Model.transform.rotation = Quaternion.identity;
            
            //일단 휴머노이드는 애니메이터를 직접 집어넣는걸로 함
            var animator = model.GetComponent<Animator>();
            Animator = animator == null ? Model.AddComponent<Animator>() : animator;
        }

        public void Dispose()
        {
            var assetManager = Global.Instance?.AssetManager;
            if (assetManager == null) return;
            assetManager.ReleaseAsset<GameObject>(LoadTarget.Model, _characterData.Id.ToString());
        }
    }
}
