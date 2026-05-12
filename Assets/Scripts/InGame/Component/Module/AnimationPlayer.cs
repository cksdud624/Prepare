using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Common;
using Generated.Table;
using InGame.Component.Model;
using InGame.Model;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using AnimationType = Common.GameDefine.AnimationType;
using LoadTarget = Common.AssetKeys.LoadTarget;

namespace InGame.Component.Module
{
    public class AnimationPlayer : MonoBehaviour
    {
        private InGameModel _inGameModel;
        private CharacterModel _characterModel;
        private Animator _animator;
        private Dictionary<AnimationType, AnimationClip> _animationClips = new();

        private PlayableGraph _graph;
        private AnimationMixerPlayable _mixer;
        private AnimationClipPlayable _slot0;
        private AnimationClipPlayable _slot1;
        private AnimationType _currentAnimationType;
        private CancellationTokenSource _crossFadeCts;

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
                string key;
                if (customAnimationType.Contains(type))
                    key = $"{characterData.Id}_{type}";
                else
                    key = $"default_{type}";
                
                var clip = await assetManager.LoadAssetAsync<AnimationClip>(LoadTarget.AnimationClip, key);
                if (clip != null)
                    _animationClips[type] = clip;
            }

            _graph = PlayableGraph.Create($"AnimationPlayer_{gameObject.name}");
            _graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);

            var output = AnimationPlayableOutput.Create(_graph, "Animation", _animator);
            _mixer = AnimationMixerPlayable.Create(_graph, 2);
            output.SetSourcePlayable(_mixer);

            _graph.Play();
        }

        public void PlayAnimation(AnimationType animationType, float crossFadeDuration = GameDefine.DefaultCrossFadeDuration)
        {
            if (!_animationClips.TryGetValue(animationType, out var clip))
            {
                Debug.LogWarning($"AnimationClip not found: {animationType}");
                return;
            }

            if (!_slot0.IsValid())
            {
                _slot0 = AnimationClipPlayable.Create(_graph, clip);
                _mixer.ConnectInput(0, _slot0, 0, 1f);
                _currentAnimationType = animationType;
                return;
            }

            if (_currentAnimationType == animationType)
                return;

            _currentAnimationType = animationType;
            CrossFadeAsync(animationType, clip, crossFadeDuration).Forget();
        }

        private async UniTaskVoid CrossFadeAsync(AnimationType animationType, AnimationClip clip, float duration)
        {
            _crossFadeCts?.Cancel();
            _crossFadeCts?.Dispose();
            _crossFadeCts = new CancellationTokenSource();
            var token = CancellationTokenSource.CreateLinkedTokenSource(
                _crossFadeCts.Token, this.GetCancellationTokenOnDestroy()).Token;

            // 이전 페이드가 중단된 경우 slot1 정리 후 slot0 가중치 초기화
            if (_slot1.IsValid())
            {
                _mixer.DisconnectInput(1);
                _slot1.Destroy();
                _slot1 = default;
            }
            _mixer.SetInputWeight(0, 1f);

            var nextPlayable = AnimationClipPlayable.Create(_graph, clip);
            _mixer.ConnectInput(1, nextPlayable, 0, 0f);
            _slot1 = nextPlayable;

            float elapsed = 0f;
            try
            {
                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float t = Mathf.Clamp01(elapsed / duration);
                    _mixer.SetInputWeight(0, 1f - t);
                    _mixer.SetInputWeight(1, t);
                    await UniTask.Yield(PlayerLoopTiming.Update, token);
                }
            }
            catch (OperationCanceledException)
            {
                return;
            }

            _mixer.SetInputWeight(0, 0f);
            _mixer.SetInputWeight(1, 1f);

            _mixer.DisconnectInput(0);
            _slot0.Destroy();
            _mixer.DisconnectInput(1);

            _mixer.ConnectInput(0, nextPlayable, 0, 1f);
            _slot0 = nextPlayable;
            _slot1 = default;
        }

        private void OnDestroy()
        {
            _crossFadeCts?.Cancel();
            _crossFadeCts?.Dispose();
            if (_graph.IsValid())
                _graph.Destroy();
        }
    }
}
