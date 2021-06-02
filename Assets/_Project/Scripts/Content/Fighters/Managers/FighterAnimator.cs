using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace Mahou.Content.Fighters
{
    public class FighterAnimator : MonoBehaviour
    {
        public delegate void AnimationEmptyAction();

        [System.Serializable]
        public class AnimationState
        {
            public double Time { get { return playableClip[0].GetTime(); } }

            public PlayableGraph playableGraph;
            public AnimationClipPlayable[] playableClip = new AnimationClipPlayable[1];
            public AnimationEmptyAction onEnd;
            public AnimationClip clip;

            public void Cleanup()
            {
                for (int i = 0; i < playableClip.Length; i++)
                {
                    if (playableClip[i].IsValid())
                    {
                        playableClip[i].Destroy();
                    }
                }
                if (playableGraph.IsValid())
                {
                    playableGraph.Destroy();
                }
            }

            public void SetTime(double value)
            {
                for (int i = 0; i < playableClip.Length; i++)
                {
                    playableClip[i].SetTime(value);
                }
                if (playableClip[0].GetTime() >= clip.length)
                {
                    /*
                    switch (clip.wrapMode)
                    {
                        case WrapMode.ClampForever:
                            playableClip[0].SetTime(clip.length);
                            break;
                    }*/
                    onEnd?.Invoke();
                }
            }
        }

        [SerializeField] private FighterManager manager;
        public Animator[] animators;
        public AnimationState currentAnimationState;

        public AnimationState PlayAnimation(AnimationClip animationClip)
        {
            if (animationClip == null)
            {
                return null;
            }
            if (currentAnimationState != null)
            {
                currentAnimationState.Cleanup();
            }
            currentAnimationState = new AnimationState();
            currentAnimationState.playableGraph = PlayableGraph.Create();
            currentAnimationState.playableClip = new AnimationClipPlayable[animators.Length];

            for (int i = 0; i < animators.Length; i++)
            {
                currentAnimationState.playableClip[i] = AnimationClipPlayable.Create(currentAnimationState.playableGraph, animationClip);
                AnimationPlayableOutput playableOutput = AnimationPlayableOutput.Create(currentAnimationState.playableGraph, "Animation", animators[i]);
                playableOutput.SetSourcePlayable(currentAnimationState.playableClip[i]);
            }
            currentAnimationState.playableGraph.Play();
            for (int i = 0; i < currentAnimationState.playableClip.Length; i++)
            {
                currentAnimationState.playableClip[i].Pause();
            }
            currentAnimationState.clip = animationClip;
            return currentAnimationState;
        }

        public void SetFrame(int frame)
        {
            if (currentAnimationState != null)
            {
                currentAnimationState.SetTime(frame * Time.fixedDeltaTime);
            }
        }

        public void SetTime(float time)
        {
            if (currentAnimationState != null)
            {
                currentAnimationState.SetTime(time);
            }
        }
    }
}