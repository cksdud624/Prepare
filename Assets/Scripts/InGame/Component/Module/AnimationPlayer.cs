using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Common;
using Common.Template.Interface;
using InGame.Component.Model;
using InGame.Model;
using Newtonsoft.Json.Serialization;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using AnimationType = Common.GameDefine.AnimationType;
using BlendTreeType = Common.GameDefine.BlendTreeType;
using AvatarMaskType = Common.GameDefine.AvatarMaskType;
using WeaponType = Common.GameDefine.WeaponType;
using WeaponAnimationType = Common.GameDefine.WeaponAnimationType;
using WeaponBlendTreeType = Common.GameDefine.WeaponBlendTreeType;
using LoadTarget = Common.AssetKeys.LoadTarget;

namespace InGame.Component.Module
{
    public class AnimationPlayer : MonoBehaviour, ILateUpdateable
    {
        private class LayerState
        {
            private const int PortCapacity = 2;
            public AvatarMaskType Layer;
            public int TargetPort;
            public AnimationMixerPlayable Mixer;
            public List<Playable> Ports = new (PortCapacity) { default, default };
            public CancellationTokenSource CrossFadeCancelToken;
            public CancellationTokenSource ParameterCancelToken;
            public int PreviousPort => 1 - TargetPort;
        }
        
        private InGameModel _inGameModel;
        private Animator _animator;
        private CharacterModel _characterModel;
        
        //커스텀 애니메이션
        private readonly HashSet<AnimationType> _customAnimationType = new();
        private readonly HashSet<BlendTreeType> _customBlendTreeType = new();
        private readonly HashSet<(WeaponType, WeaponAnimationType)> _customWeaponAnimationType = new();
        private readonly HashSet<(WeaponType, WeaponBlendTreeType)> _customWeaponBlendTreeType = new();

        //애니메이션 데이터 캐싱
        private readonly Dictionary<AnimationType, AnimationClip> _animationClips = new();
        private readonly Dictionary<BlendTreeType, RuntimeAnimatorController> _blendTrees = new();
        private readonly Dictionary<(WeaponType, WeaponAnimationType), AnimationClip> _weaponAnimationClips = new();
        private readonly Dictionary<(WeaponType, WeaponBlendTreeType), RuntimeAnimatorController> _weaponBlendTrees = new();
        
        private readonly Dictionary<AvatarMaskType, LayerState> _layerStates = new();
        
        private PlayableGraph _graph;
        private AnimationLayerMixerPlayable _layerMixer;
        private CancellationTokenSource _layerCancelToken;

        private bool _isAimIKEnabled;
        private Transform _aimPivot;
        
        
        public async UniTask Init(InGameModel inGameModel, Animator animator, CharacterModel characterModel)
        {
            _animator = animator;
            _characterModel = characterModel;

            var characterData = _characterModel.CharacterData;
            
            /*
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
            */

            var assetManager = Global.Instance.AssetManager;
            string key;
            foreach (AnimationType type in Enum.GetValues(typeof(AnimationType)))
            {
                key = _customAnimationType.Contains(type) ? $"{characterData.Id}_{type}" : $"default_{type}";
                var clip = await assetManager.LoadAssetAsync<AnimationClip>(LoadTarget.AnimationClip, key);
                if (clip != null) _animationClips[type] = clip;
                else Debug.LogError("AnimationClip not found: " + key);
            }
            
            foreach (BlendTreeType type in Enum.GetValues(typeof(BlendTreeType)))
            {
                key = _customBlendTreeType.Contains(type) ? $"{characterData.Id}_{type}" : $"default_{type}";
                var tree = await assetManager.LoadAssetAsync<RuntimeAnimatorController>(LoadTarget.BlendTree, key);
                if (tree != null) _blendTrees[type] = tree;
            }
            
            foreach (var weaponData in characterModel.WeaponStatusDataList)
            {
                var weaponType = (WeaponType)weaponData.WeaponType;
                foreach (WeaponAnimationType animType in Enum.GetValues(typeof(WeaponAnimationType)))
                {
                    key = _customWeaponAnimationType.Contains((weaponType, animType))
                    ? $"{weaponData.Id}_{animType}" : $"default_{weaponType}_{animType}";
                    var clip = await assetManager.LoadAssetAsync<AnimationClip>(LoadTarget.WeaponAnimationClip, key);
                    if (clip != null) _weaponAnimationClips[(weaponType, animType)] = clip;
                    else Debug.LogError("AnimationClip not found: " + key);
                }

                foreach (WeaponBlendTreeType blendTreeType in Enum.GetValues(typeof(WeaponBlendTreeType)))
                {
                    key = _customWeaponBlendTreeType.Contains((weaponType, blendTreeType))
                        ? $"{weaponData.Id}_{blendTreeType}" : $"default_{weaponType}_{blendTreeType}";
                    var tree = await assetManager.LoadAssetAsync<RuntimeAnimatorController>(LoadTarget.WeaponBlendTree, key);
                    if (tree != null) _weaponBlendTrees[(weaponType, blendTreeType)] = tree;
                    else Debug.LogError("WeaponBlendTree not found: " + key);
                }
            }

            _graph = PlayableGraph.Create($"AnimationPlayer_{gameObject.name}");
            _graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
            var output = AnimationPlayableOutput.Create(_graph, "Animation", _animator);

            var layerCount = Enum.GetValues(typeof(AvatarMaskType)).Length;
            _layerMixer = AnimationLayerMixerPlayable.Create(_graph, layerCount);
            output.SetSourcePlayable(_layerMixer);
            
            foreach (AvatarMaskType type in Enum.GetValues(typeof(AvatarMaskType)))
            {
                float weight = 1;
                if (type != AvatarMaskType.Base)
                {
                    var mask = await assetManager.LoadAssetAsync<AvatarMask>(LoadTarget.AvatarMask, nameof(AvatarMaskType.Upper));
                    if(mask != null)
                        _layerMixer.SetLayerMaskFromAvatarMask((uint)type, mask);
                    weight = 0;
                }
                _layerMixer.SetInputWeight((int)type, weight);

                LayerState state = new ()
                {
                    Layer = type,
                    Mixer = AnimationMixerPlayable.Create(_graph, 2)
                };
                _graph.Connect(state.Mixer, 0, _layerMixer, (int)type);
                _layerStates.Add(type, state);
            }

            _graph.Play();
            Global.Instance.BindLateUpdate(this);
        }

        public void PlayAnimation(AnimationType type, AvatarMaskType maskType, float? playDuration = null)
        {
            if (!_animationClips.TryGetValue(type, out var clip))
            {
                Debug.LogError($"AnimationClip not found: {type}");
                return;
            }
            
            var clipPlayable = AnimationClipPlayable.Create(_graph, clip);
            if(playDuration.HasValue && playDuration.Value != 0f)
                clipPlayable.SetSpeed(clip.length / playDuration.Value);

            CrossFadeAnimation(_layerStates[maskType], clipPlayable).Forget();

            if (maskType == AvatarMaskType.Upper)
            {
                CrossFadeLayer((int)maskType, 1).Forget();
            }
        }

        public void PlayAnimation(WeaponType weaponType, WeaponAnimationType animType, AvatarMaskType maskType, float? playDuration = null)
        {
            if (!_weaponAnimationClips.TryGetValue((weaponType, animType), out var clip))
            {
                Debug.LogError($"WeaponAnimationClip not found: {weaponType}_{animType}");
                return;
            }

            var clipPlayable = AnimationClipPlayable.Create(_graph, clip);
            if (playDuration.HasValue && playDuration.Value != 0f)
                clipPlayable.SetSpeed(clip.length / playDuration.Value);

            CrossFadeAnimation(_layerStates[maskType], clipPlayable).Forget();

            if (maskType == AvatarMaskType.Upper)
                CrossFadeLayer((int)maskType, 1).Forget();
        }

        public void PlayBlendTree(BlendTreeType type, AvatarMaskType maskType)
        {
            if (!_blendTrees.TryGetValue(type, out var blendTree))
            {
                Debug.LogError($"BlendTree not found: {type}");
                return;
            }

            var blendTreePlayable = AnimatorControllerPlayable.Create(_graph, blendTree);
            CrossFadeAnimation(_layerStates[maskType], blendTreePlayable).Forget();
        }

        public void PlayBlendTree(WeaponType weaponType, WeaponBlendTreeType type, AvatarMaskType maskType)
        {
            if (!_weaponBlendTrees.TryGetValue((weaponType, type), out var blendTree))
            {
                Debug.LogError($"WeaponBlendTree not found: {weaponType}_{type}");
                return;
            }

            var blendTreePlayable = AnimatorControllerPlayable.Create(_graph, blendTree);
            CrossFadeAnimation(_layerStates[maskType], blendTreePlayable).Forget();

            if (maskType == AvatarMaskType.Upper)
                CrossFadeLayer((int)maskType, 1).Forget();
        }

        public void SetParameter(AvatarMaskType maskType, float value, bool isLerp = true, float lerpDuration = 0.1f)
        {
            if (!TryGetControllerPlayable(maskType, out var playable)) return;

            if (isLerp)
                LerpParameter(_layerStates[maskType], playable, value, lerpDuration).Forget();
            else
                playable.SetFloat(GameDefine.Parameter1, value);
        }

        public void SetParameter(AvatarMaskType maskType, Vector2 value, bool isLerp = true)
        {
            if (!TryGetControllerPlayable(maskType, out var playable)) return;

            if (isLerp)
                LerpParameter(_layerStates[maskType], playable, value).Forget();
            else
            {
                playable.SetFloat(GameDefine.Parameter1, value.x);
                playable.SetFloat(GameDefine.Parameter2, value.y);
            }
        }

        private bool TryGetControllerPlayable(AvatarMaskType maskType, out AnimatorControllerPlayable playable)
        {
            var activePlayable = _layerStates[maskType].Ports[_layerStates[maskType].PreviousPort];
            if (activePlayable.IsValid() && activePlayable.IsPlayableOfType<AnimatorControllerPlayable>())
            {
                playable = (AnimatorControllerPlayable)activePlayable;
                return true;
            }
            Debug.Log("Playable is not BlendTree");
            playable = default;
            return false;
        }

        private async UniTask LerpParameter(LayerState layerState, AnimatorControllerPlayable controllerPlayable, float value, float lerpDuration)
        {
            layerState.ParameterCancelToken?.Cancel();
            layerState.ParameterCancelToken = new CancellationTokenSource();
            if (lerpDuration <= 0f)
            {
                controllerPlayable.SetFloat(GameDefine.Parameter1, value);
                return;
            }

            float elapsed = 0f;
            float start = controllerPlayable.GetFloat(GameDefine.Parameter1);
            try
            {
                while (elapsed < lerpDuration)
                {
                    elapsed += Time.deltaTime;
                    controllerPlayable.SetFloat(GameDefine.Parameter1, Mathf.Lerp(start, value, Mathf.Clamp01(elapsed / lerpDuration)));
                    await UniTask.Yield(PlayerLoopTiming.Update, layerState.ParameterCancelToken.Token);
                }
                controllerPlayable.SetFloat(GameDefine.Parameter1, value);
            }
            catch (OperationCanceledException)
            {
            }
        }

        private async UniTask LerpParameter(LayerState layerState, AnimatorControllerPlayable controllerPlayable, Vector2 value, float lerpDuration = 0.1f)
        {
            layerState.ParameterCancelToken?.Cancel();
            layerState.ParameterCancelToken = new CancellationTokenSource();
            if (lerpDuration <= 0f)
            {
                controllerPlayable.SetFloat(GameDefine.Parameter1, value.x);
                controllerPlayable.SetFloat(GameDefine.Parameter2, value.y);
                return;
            }

            float elapsed = 0f;
            float start1 = controllerPlayable.GetFloat(GameDefine.Parameter1);
            float start2 = controllerPlayable.GetFloat(GameDefine.Parameter2);
            try
            {
                while (elapsed < lerpDuration)
                {
                    elapsed += Time.deltaTime;
                    float t = Mathf.Clamp01(elapsed / lerpDuration);
                    controllerPlayable.SetFloat(GameDefine.Parameter1, Mathf.Lerp(start1, value.x, t));
                    controllerPlayable.SetFloat(GameDefine.Parameter2, Mathf.Lerp(start2, value.y, t));
                    await UniTask.Yield(PlayerLoopTiming.Update, layerState.ParameterCancelToken.Token);
                }
                controllerPlayable.SetFloat(GameDefine.Parameter1, value.x);
                controllerPlayable.SetFloat(GameDefine.Parameter2, value.y);
            }
            catch (OperationCanceledException)
            {
            }
        }

        public void SetAimIK(bool enable, Transform aimPivot = null)
        {
            _isAimIKEnabled = enable;
            _aimPivot = aimPivot;
        }

        public void OnLateUpdate()
        {
            if (!_isAimIKEnabled || _aimPivot == null) return;

            var aimForward = _aimPivot.forward;
            var modelRight = _animator.transform.right;
            var horizontalForward = Vector3.ProjectOnPlane(aimForward, Vector3.up).normalized;
            float pitch = Vector3.SignedAngle(horizontalForward, aimForward, modelRight);

            var spine = _animator.GetBoneTransform(HumanBodyBones.Spine);
            var chest = _animator.GetBoneTransform(HumanBodyBones.Chest);

            spine?.Rotate(modelRight, pitch * 0.5f, Space.World);
            chest?.Rotate(modelRight, pitch * 0.5f, Space.World);
        }

        public void StopAnimation(AvatarMaskType maskType)
        {
            if (maskType != AvatarMaskType.Upper) return;
            StopUpperLayer();
        }

        public void StopBlendTree(AvatarMaskType maskType)
        {
            if (maskType != AvatarMaskType.Upper) return;
            StopUpperLayer();
        }

        private void StopUpperLayer()
        {
            var state = _layerStates[AvatarMaskType.Upper];
            state.CrossFadeCancelToken?.Cancel();
            state.ParameterCancelToken?.Cancel();

            for (int i = 0; i < state.Ports.Count; i++)
            {
                if (state.Ports[i].IsValid())
                {
                    _graph.Disconnect(state.Mixer, i);
                    state.Ports[i].Destroy();
                }
                state.Ports[i] = default;
                state.Mixer.SetInputWeight(i, 0f);
            }
            state.TargetPort = 0;

            CrossFadeLayer((int)AvatarMaskType.Upper, 0).Forget();
        }

        private async UniTask CrossFadeAnimation(LayerState state, Playable nextPlayable, float crossFadeDuration = 0.1f)
        {
            state.CrossFadeCancelToken?.Cancel();
            state.CrossFadeCancelToken = new CancellationTokenSource();
            state.ParameterCancelToken?.Cancel();

            int previousPort = state.PreviousPort;
            int targetPort = state.TargetPort;
            state.TargetPort = previousPort;
            
            
            float startWeight = state.Mixer.GetInputWeight(targetPort);
            if (state.Ports[targetPort].IsValid())
            {
                _graph.Disconnect(state.Mixer, targetPort);
                state.Ports[targetPort].Destroy();
            }
            _graph.Connect(nextPlayable, 0, state.Mixer, targetPort);
            state.Ports[targetPort] = nextPlayable; 
            
            float elapsed = 0f;
            float previousWeight = state.Mixer.GetInputWeight(previousPort);

            try
            {
                while (elapsed < crossFadeDuration)
                {
                    elapsed += Time.deltaTime;
                    float progress = Mathf.Clamp01(elapsed / crossFadeDuration);
                    state.Mixer.SetInputWeight(targetPort, Mathf.Lerp(startWeight, 1f, progress));
                    state.Mixer.SetInputWeight(previousPort, Mathf.Lerp(previousWeight, 0f, progress));
                    await UniTask.Yield(PlayerLoopTiming.Update, state.CrossFadeCancelToken.Token);
                }
                state.Mixer.SetInputWeight(targetPort, 1f);
                state.Mixer.SetInputWeight(previousPort, 0f);
            }
            catch (OperationCanceledException)
            {
            }
        }

        private async UniTask CrossFadeLayer(int layerIndex, float targetWeight, float crossFadeDuration = 0.1f)
        {
            _layerCancelToken?.Cancel();
            _layerCancelToken = new CancellationTokenSource();
            var token = _layerCancelToken.Token;
            
            if (crossFadeDuration <= 0f)
            {
                _layerMixer.SetInputWeight(layerIndex, targetWeight);
                return;
            }
            
            float startWeight = _layerMixer.GetInputWeight(layerIndex);
            float elapsed = 0f;
            
            try
            {
                while (elapsed < crossFadeDuration)
                {
                    elapsed += Time.deltaTime;
                    float progress = Mathf.Clamp01(elapsed / crossFadeDuration);
                    _layerMixer.SetInputWeight(layerIndex, Mathf.Lerp(startWeight, targetWeight, progress));
                    await UniTask.Yield(PlayerLoopTiming.Update, token);
                }
                _layerMixer.SetInputWeight(layerIndex, targetWeight);
            }
            catch (OperationCanceledException)
            {
            }
        }
        
        private void OnDestroy()
        {
            Global.Instance?.UnBindLateUpdate(this);
            if (_graph.IsValid())
                _graph.Destroy();

            foreach (var layerState in _layerStates)
            {
                layerState.Value.CrossFadeCancelToken?.Cancel();
                layerState.Value.ParameterCancelToken?.Cancel();
            }

            _layerCancelToken?.Cancel();

            var assetManager = Global.Instance?.AssetManager;
            if (assetManager == null) return;
            
            foreach (AvatarMaskType type in Enum.GetValues(typeof(AvatarMaskType)))
            {
                if (type == AvatarMaskType.Base)
                    continue;
                
                assetManager.ReleaseAsset<AvatarMask>(LoadTarget.AvatarMask, nameof(type));
            }

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

            foreach (var weaponData in _characterModel.WeaponStatusDataList)
            {
                var weaponType = (WeaponType)weaponData.WeaponType;
                foreach (WeaponAnimationType animType in Enum.GetValues(typeof(WeaponAnimationType)))
                {
                    key = _customWeaponAnimationType.Contains((weaponType, animType))
                        ? $"{weaponData.Id}_{animType}" : $"default_{weaponType}_{animType}";
                    assetManager.ReleaseAsset<AnimationClip>(LoadTarget.WeaponAnimationClip, key);
                }

                foreach (WeaponBlendTreeType blendTreeType in Enum.GetValues(typeof(WeaponBlendTreeType)))
                {
                    key = _customWeaponBlendTreeType.Contains((weaponType, blendTreeType))
                        ? $"{weaponData.Id}_{blendTreeType}" : $"default_{weaponType}_{blendTreeType}";
                    assetManager.ReleaseAsset<RuntimeAnimatorController>(LoadTarget.WeaponBlendTree, key);
                }
            }
        }
    }
}
