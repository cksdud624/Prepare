using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Common;
using InGame.Component.Model;
using InGame.Model;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using AnimationType = Common.GameDefine.AnimationType;
using BlendTreeType = Common.GameDefine.BlendTreeType;
using AvatarMaskType = Common.GameDefine.AvatarMaskType;
using LoadTarget = Common.AssetKeys.LoadTarget;

namespace InGame.Component.Module
{
    public class AnimationPlayer : MonoBehaviour
    {
        private InGameModel _inGameModel;
        private Animator _animator;
        private CharacterModel _characterModel;

        private readonly HashSet<AnimationType> _customAnimationType = new();
        private readonly HashSet<BlendTreeType> _customBlendTreeType = new();
        private readonly Dictionary<AnimationType, AnimationClip> _animationClips = new();
        private readonly Dictionary<BlendTreeType, RuntimeAnimatorController> _blendTrees = new();

        private PlayableGraph _graph;
        private AnimationLayerMixerPlayable _layerMixer;
        private CancellationTokenSource _layerCancelToken;
        
        
        public async UniTask Init(InGameModel inGameModel, Animator animator, CharacterModel characterModel)
        {
            _animator = animator;
            _characterModel = characterModel;

            var characterData = _characterModel.CharacterData;
            if (!(characterData.CustomAnimation.Count == 1 && string.IsNullOrEmpty(characterData.CustomAnimation[0])))
            {
                foreach (var custom in characterData.CustomAnimation)
                {
                    if (Enum.TryParse<AnimationType>(custom, true, out var result))
                        _customAnimationType.Add(result);
                    else
                        Debug.LogError("Custom Animation Type not found: " + custom);
                }
            }

            var assetManager = Global.Instance.AssetManager;
            string key;
            foreach (AnimationType type in Enum.GetValues(typeof(AnimationType)))
            {
                key = _customAnimationType.Contains(type) ? $"{characterData.Id}_{type}" : $"default_{type}";
                var clip = await assetManager.LoadAssetAsync<AnimationClip>(LoadTarget.AnimationClip, key);
                if (clip != null) _animationClips[type] = clip;
            }

            foreach (BlendTreeType type in Enum.GetValues(typeof(BlendTreeType)))
            {
                key = _customBlendTreeType.Contains(type) ? $"{characterData.Id}_{type}" : $"default_{type}";
                var tree = await assetManager.LoadAssetAsync<RuntimeAnimatorController>(LoadTarget.BlendTree, key);
                if (tree != null) _blendTrees[type] = tree;
            }

            var upperMask = await assetManager.LoadAssetAsync<AvatarMask>(LoadTarget.AvatarMask, nameof(AvatarMaskType.Upper));

            _graph = PlayableGraph.Create($"AnimationPlayer_{gameObject.name}");
            _graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
            var output = AnimationPlayableOutput.Create(_graph, "Animation", _animator);

            int layerCount = GameDefine.LayerCount;
            _layerMixer = AnimationLayerMixerPlayable.Create(_graph, layerCount);
            output.SetSourcePlayable(_layerMixer);

            if (upperMask != null)
                _layerMixer.SetLayerMaskFromAvatarMask(1, upperMask);
            
            _layerMixer.SetInputWeight(0, 1);
            _layerMixer.SetInputWeight(1, 0);

            _graph.Play();
        }

        public void PlayAnimation(AnimationType type, AvatarMaskType maskType, float? playDuration = null)
        {
            if (!_animationClips.TryGetValue(type, out var clip))
            {
                Debug.LogError($"AnimationClip not found: {type}");
                return;
            }
            
            var clipPlayable = AnimationClipPlayable.Create(_graph, clip);
            if(playDuration.HasValue)
                clipPlayable.SetSpeed(clip.length / playDuration.Value);

            if (maskType == AvatarMaskType.Upper)
            {
                CrossFadeLayer((int)maskType, 1).Forget();
            }
        }

        public void PlayBlendTree(BlendTreeType type, AvatarMaskType maskType)
        {
            if (!_blendTrees.TryGetValue(type, out var blendTree))
            {
                Debug.LogError($"BlendTree not found: {type}");
                return;
            }
            
            var blendTreePlayable = AnimatorControllerPlayable.Create(_graph, blendTree);
            blendTreePlayable.SetFloat("Move1", 1);
            _graph.Connect(blendTreePlayable, 0, _layerMixer, 0);
        }

        private async UniTask CrossFadeLayer(int layerIndex, float targetWeight, float duration = 0.1f)
        {
            _layerCancelToken?.Cancel();
            _layerCancelToken = new CancellationTokenSource();
            var token = _layerCancelToken.Token;
            
            if (duration <= 0f)
            {
                _layerMixer.SetInputWeight(layerIndex, targetWeight);
                return;
            }
            
            var startWeight = _layerMixer.GetInputWeight(layerIndex);
            var elapsed = 0f;
            try
            {
                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    _layerMixer.SetInputWeight(layerIndex, Mathf.Lerp(startWeight, targetWeight, Mathf.Clamp01(elapsed / duration)));
                    await UniTask.Yield(PlayerLoopTiming.Update, token);
                }
            }
            catch (OperationCanceledException)
            {
                
            }


        }
        
        private void OnDestroy()
        {
            if (_graph.IsValid())
                _graph.Destroy();

            var assetManager = Global.Instance?.AssetManager;
            if (assetManager == null) return;

            assetManager.ReleaseAsset<AvatarMask>(LoadTarget.AvatarMask, nameof(AvatarMaskType.Upper));

            var characterData = _characterModel.CharacterData;
            string key;
            foreach (AnimationType type in Enum.GetValues(typeof(AnimationType)))
            {
                key = _customAnimationType.Contains(type) ? $"{characterData.Id}_{type}" : $"default_{type}";
                assetManager.ReleaseAsset<AnimationClip>(LoadTarget.AnimationClip, key);
            }

            foreach (BlendTreeType type in Enum.GetValues(typeof(BlendTreeType)))
            {
                key = _customBlendTreeType.Contains(type) ? $"{characterData.Id}_{type}" : $"default_{type}";
                assetManager.ReleaseAsset<RuntimeAnimatorController>(LoadTarget.BlendTree, key);
            }
        }
    }
}
