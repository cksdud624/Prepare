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
using LoadTarget = Common.AssetKeys.LoadTarget;
using AvatarMaskType = Common.GameDefine.AvatarMaskType;

namespace InGame.Component.Module
{
    public class AnimationPlayer : MonoBehaviour
    {
        private class LayerState
        {
            public AnimationMixerPlayable Mixer;
            public AnimationClipPlayable Slot0;
            public AnimationClipPlayable Slot1;
            public AnimationType? CurrentType;
            public CancellationTokenSource CrossFadeCts;
        }

        private InGameModel _inGameModel;
        private Animator _animator;
        private Dictionary<AnimationType, AnimationClip> _animationClips = new();

        private PlayableGraph _graph;
        private AnimationLayerMixerPlayable _layerMixer;
        private LayerState[] _layers;

        public async UniTask Init(InGameModel inGameModel, Animator animator, CharacterModel characterModel)
        {
            _inGameModel = inGameModel;
            _animator = animator;

            var characterData = characterModel.CharacterData;
            HashSet<AnimationType> customAnimationType = new();
            if (!(characterData.CustomAnimation.Count == 1 && string.IsNullOrEmpty(characterData.CustomAnimation[0])))
            {
                foreach (var custom in characterData.CustomAnimation)
                {
                    if (Enum.TryParse<AnimationType>(custom, true, out var result))
                        customAnimationType.Add(result);
                    else
                        Debug.LogError("Custom Animation Type not found: " + custom);
                }
            }

            var assetManager = Global.Instance.AssetManager;
            foreach (AnimationType type in Enum.GetValues(typeof(AnimationType)))
            {
                string key = customAnimationType.Contains(type)
                    ? $"{characterData.Id}_{type}"
                    : $"default_{type}";

                var clip = await assetManager.LoadAssetAsync<AnimationClip>(LoadTarget.AnimationClip, key);
                if (clip != null)
                    _animationClips[type] = clip;
            }

            var lowerMask = await assetManager.LoadAssetAsync<AvatarMask>(LoadTarget.AvatarMask, "lower");
            var upperMask = await assetManager.LoadAssetAsync<AvatarMask>(LoadTarget.AvatarMask, "upper");

            _graph = PlayableGraph.Create($"AnimationPlayer_{gameObject.name}");
            _graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);

            var output = AnimationPlayableOutput.Create(_graph, "Animation", _animator);

            // Layer 0: Full body (base), Layer 1: Lower body override, Layer 2: Upper body override
            _layerMixer = AnimationLayerMixerPlayable.Create(_graph, 3);
            output.SetSourcePlayable(_layerMixer);

            _layers = new LayerState[3];
            for (int i = 0; i < 3; i++)
            {
                _layers[i] = new LayerState();
                _layers[i].Mixer = AnimationMixerPlayable.Create(_graph, 2);
                _layerMixer.ConnectInput(i, _layers[i].Mixer, 0, 0f);
            }

            if (lowerMask != null)
                _layerMixer.SetLayerMaskFromAvatarMask((uint)ToLayerIndex(AvatarMaskType.Lower), lowerMask);
            if (upperMask != null)
                _layerMixer.SetLayerMaskFromAvatarMask((uint)ToLayerIndex(AvatarMaskType.Upper), upperMask);

            _graph.Play();
        }

        public void PlayAnimation(AnimationType animationType, AvatarMaskType maskType, float crossFadeDuration = GameDefine.DefaultCrossFadeDuration, float playDuration = 0f)
        {
            if (!_animationClips.TryGetValue(animationType, out var clip))
            {
                Debug.LogWarning($"AnimationClip not found: {animationType}");
                return;
            }

            int idx = ToLayerIndex(maskType);
            var layer = _layers[idx];

            if (!layer.Slot0.IsValid())
            {
                layer.Slot0 = AnimationClipPlayable.Create(_graph, clip);
                ApplyPlayDuration(layer.Slot0, clip, playDuration);
                layer.Mixer.ConnectInput(0, layer.Slot0, 0, 1f);
                layer.CurrentType = animationType;
                _layerMixer.SetInputWeight(idx, 1f);
                return;
            }

            if (layer.CurrentType == animationType)
                return;

            layer.CurrentType = animationType;
            _layerMixer.SetInputWeight(idx, 1f);
            CrossFadeAsync(idx, clip, crossFadeDuration, playDuration).Forget();
        }

        public void StopLayer(AvatarMaskType maskType, float fadeDuration = GameDefine.DefaultCrossFadeDuration)
        {
            FadeOutLayerAsync(ToLayerIndex(maskType), fadeDuration).Forget();
        }

        private static int ToLayerIndex(AvatarMaskType maskType) => maskType switch
        {
            AvatarMaskType.Full => 0,
            AvatarMaskType.Lower => 1,
            AvatarMaskType.Upper => 2,
            _ => 0
        };

        private static void ApplyPlayDuration(AnimationClipPlayable playable, AnimationClip clip, float playDuration)
        {
            if (playDuration > 0f)
                playable.SetSpeed(clip.length / playDuration);
        }

        private async UniTaskVoid CrossFadeAsync(int idx, AnimationClip clip, float duration, float playDuration)
        {
            var layer = _layers[idx];
            layer.CrossFadeCts?.Cancel();
            layer.CrossFadeCts?.Dispose();
            layer.CrossFadeCts = new CancellationTokenSource();
            var token = CancellationTokenSource.CreateLinkedTokenSource(
                layer.CrossFadeCts.Token, this.GetCancellationTokenOnDestroy()).Token;

            if (layer.Slot1.IsValid())
            {
                layer.Mixer.DisconnectInput(1);
                layer.Slot1.Destroy();
                layer.Slot1 = default;
            }
            layer.Mixer.SetInputWeight(0, 1f);

            var nextPlayable = AnimationClipPlayable.Create(_graph, clip);
            ApplyPlayDuration(nextPlayable, clip, playDuration);
            layer.Mixer.ConnectInput(1, nextPlayable, 0, 0f);
            layer.Slot1 = nextPlayable;

            float elapsed = 0f;
            try
            {
                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float t = Mathf.Clamp01(elapsed / duration);
                    layer.Mixer.SetInputWeight(0, 1f - t);
                    layer.Mixer.SetInputWeight(1, t);
                    await UniTask.Yield(PlayerLoopTiming.Update, token);
                }
            }
            catch (OperationCanceledException)
            {
                return;
            }

            layer.Mixer.SetInputWeight(0, 0f);
            layer.Mixer.SetInputWeight(1, 1f);

            layer.Mixer.DisconnectInput(0);
            layer.Slot0.Destroy();
            layer.Mixer.DisconnectInput(1);

            layer.Mixer.ConnectInput(0, nextPlayable, 0, 1f);
            layer.Slot0 = nextPlayable;
            layer.Slot1 = default;
        }

        private async UniTaskVoid FadeOutLayerAsync(int idx, float duration)
        {
            var layer = _layers[idx];
            layer.CrossFadeCts?.Cancel();
            layer.CrossFadeCts?.Dispose();
            layer.CrossFadeCts = new CancellationTokenSource();
            var token = CancellationTokenSource.CreateLinkedTokenSource(
                layer.CrossFadeCts.Token, this.GetCancellationTokenOnDestroy()).Token;

            float startWeight = _layerMixer.GetInputWeight(idx);
            float elapsed = 0f;
            try
            {
                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float t = Mathf.Clamp01(elapsed / duration);
                    _layerMixer.SetInputWeight(idx, Mathf.Lerp(startWeight, 0f, t));
                    await UniTask.Yield(PlayerLoopTiming.Update, token);
                }
            }
            catch (OperationCanceledException) { return; }

            _layerMixer.SetInputWeight(idx, 0f);
            layer.CurrentType = null;

            if (layer.Slot1.IsValid())
            {
                layer.Mixer.DisconnectInput(1);
                layer.Slot1.Destroy();
                layer.Slot1 = default;
            }
            if (layer.Slot0.IsValid())
            {
                layer.Mixer.DisconnectInput(0);
                layer.Slot0.Destroy();
                layer.Slot0 = default;
            }
        }

        private void OnDestroy()
        {
            if (_layers != null)
            {
                foreach (var layer in _layers)
                {
                    layer?.CrossFadeCts?.Cancel();
                    layer?.CrossFadeCts?.Dispose();
                }
            }
            if (_graph.IsValid())
                _graph.Destroy();
        }
    }
}
