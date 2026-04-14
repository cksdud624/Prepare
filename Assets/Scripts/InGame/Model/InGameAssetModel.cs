using System.Collections.Generic;
using UnityEngine;

namespace InGame.Model
{
    public class InGameAssetModel
    {
        private readonly Dictionary<long, GameObject> _modelAssets = new();
        public void AddModel(long id, GameObject model) => _modelAssets.Add(id, model);
        public void RemoveModel(long id) => _modelAssets.Remove(id);
        public GameObject GetModel(long id) => _modelAssets.GetValueOrDefault(id);
    }
}
