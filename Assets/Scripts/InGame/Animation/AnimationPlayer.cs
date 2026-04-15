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
        private Animator Animator { get; set; }
        private Dictionary<InGameCommonAnimation, AnimationClipPlayable> _animationClips = new();
        
        private PlayableGraph Graph { get; set; }
        private AnimationPlayableOutput Output { get; set; }
        private AnimationMixerPlayable Mixer { get; set; }
        private AnimationClipPlayable CurrentClip { get; set; }

        public void Init(GameObject model, Animator animator, Dictionary<InGameCommonAnimation, AnimationClip> clips)
        {
            Graph = PlayableGraph.Create("AnimationGraph");
            
            if (animator == null)
                model.AddComponent<AnimationPlayer>();
            else
                Animator = animator;
            
            foreach (var clip in clips)
            {
                var clipPlayable = AnimationClipPlayable.Create(Graph, clip.Value);
                _animationClips.Add(clip.Key, clipPlayable);
            }
            
            Output = AnimationPlayableOutput.Create(Graph, "AnimationPlayer", Animator);
            Mixer = AnimationMixerPlayable.Create(Graph, 2);
            Output.SetSourcePlayable(Mixer);
            Graph.Play();
        }

        public void PlayAnimation(InGameCommonAnimation anim)
        {
            if (_animationClips.TryGetValue(anim, out var clip))
                CrossFade(clip).Forget();
        }

        private async UniTask CrossFade(AnimationClipPlayable clip, float duration = 0.3f)
        {
            if (CurrentClip.IsValid() && CurrentClip.GetHandle() == clip.GetHandle())
            {
                Debug.Log($"{CurrentClip.GetAnimationClip().name} is already playing");
                return;
            }
            
            clip.SetTime(0);
            clip.SetDone(false);
            clip.Play();

            if (!CurrentClip.IsValid())
            {
                Mixer.ConnectInput(0, clip, 0);
                Mixer.SetInputWeight(0, 1f);
                Mixer.SetInputWeight(1, 0f);
                CurrentClip = clip;
                return;
            }
            
            await UniTask.CompletedTask;
        }

        private void OnDestroy()
        {
            if(Graph.IsValid())
                Graph.Destroy();
        }
    }
}
