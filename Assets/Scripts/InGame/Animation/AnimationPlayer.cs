using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Animations;
using UnityEngine.Playables;
using static Common.GameDefine;
using Cysharp.Threading.Tasks;
using UnityEngine.Rendering.Universal;

namespace InGame.Animation
{
    public class AnimationPlayer : MonoBehaviour
    {
        private Animator _animator;
        
        private PlayableGraph _graph;
        private AnimationPlayableOutput _output;
        private AnimationMixerPlayable _mixer;
        private AnimationClipPlayable _currentClip;
        private Dictionary<AnimationClip, AnimationClipPlayable> _clipsCache = new();

        public void Init(GameObject model)
        {
            _graph = PlayableGraph.Create("AnimationGraph");
            
            _animator = model.GetComponent<Animator>();
            if(_animator == null)
                _animator = model.AddComponent<Animator>();
            
            _output = AnimationPlayableOutput.Create(_graph, "AnimationPlayer", _animator);
            _mixer = AnimationMixerPlayable.Create(_graph, 2);
            _output.SetSourcePlayable(_mixer);
            _graph.Play();
        }

        public void PlayAnimation(AnimationClip clip, float length = 0f, float crossfade = 0.2f)
        {
            if(!_clipsCache.ContainsKey(clip))
                _clipsCache.Add(clip, AnimationClipPlayable.Create(_graph, clip));
            CrossFade(_clipsCache[clip]).Forget();
        }

        private async UniTask CrossFade(AnimationClipPlayable clip, float duration = 0.3f)
        {
            if (_currentClip.IsValid() && _currentClip.GetHandle() == clip.GetHandle())
            {
                Debug.Log($"{_currentClip.GetAnimationClip().name} is already playing");
                return;
            }
            
            clip.SetTime(0);
            clip.SetDone(false);
            clip.Play();

            if (!_currentClip.IsValid())
            {
                _mixer.ConnectInput(0, clip, 0);
                _mixer.SetInputWeight(0, 1f);
                _mixer.SetInputWeight(1, 0f);
                _currentClip = clip;
                return;
            }
            
            await UniTask.CompletedTask;
        }

        private void OnDestroy()
        {
            if(_graph.IsValid())
                _graph.Destroy();
        }
    }
}
