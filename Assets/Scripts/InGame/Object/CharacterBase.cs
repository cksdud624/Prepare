using Common;
using UnityEngine;
using UnityEngine.AddressableAssets;

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
        
        #region Asset Management
        protected override void LoadAsset()
        {
            var model = inGameModel.InGameAssetModel.GetModel(inGameModel.InGameObjectModel.PlayerData.Id);
            Model = Instantiate(model, this.transform);
        }
        #endregion
    }
}
