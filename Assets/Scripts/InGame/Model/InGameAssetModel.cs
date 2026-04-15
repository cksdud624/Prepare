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
        public Dictionary<long, GameObject> GetModels() => _modelAssets;
        
        private readonly Dictionary<string, AnimationClip> _animationClips = new();
        public void AddAnimationClip(string key, AnimationClip clip) => _animationClips.Add(key, clip);
        public void RemoveAnimationClip(string key) => _animationClips.Remove(key);
        public AnimationClip GetAnimationClip(string key) => _animationClips.GetValueOrDefault(key);
        public Dictionary<string, AnimationClip> GetAnimationClips() => _animationClips;
    }
}
