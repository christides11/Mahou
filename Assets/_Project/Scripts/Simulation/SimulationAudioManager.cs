using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Simulation
{
    public enum AudioPlayMode { 
        FIRE_AND_FORGET,
        ROLLBACK
    }
    public static class SimulationAudioManager
    {
        public static string audioSourceName = "audio clip";

        public static List<SimAudioDefinition> currentPlayingAudio = new List<SimAudioDefinition>();

        public static AudioSource Play(AudioClip clip, Vector3 position, AudioPlayMode playMode)
        {
            if (SimulationManagerBase.IsRollbackFrame == false)
            {
                return CreateAudioSource(SimulationManagerBase.instance.CurrentTick, clip, position, 0.0f);
            }
            else
            {
                Debug.Log("ROLLBACK AUDIO");
                for(int i = 0; i < currentPlayingAudio.Count; i++)
                {
                    if (currentPlayingAudio[i].clip == clip && Vector3.Distance(currentPlayingAudio[i].position, position) < 0.1f){
                        Debug.Log($"Similar clip and position. {currentPlayingAudio[i].frameCreated} vs {SimulationManagerBase.instance.CurrentRollbackTick}");
                        if (currentPlayingAudio[i].frameCreated == SimulationManagerBase.instance.CurrentRollbackTick)
                        {
                            if (currentPlayingAudio[i].confirmed == false
                                && (SimulationManagerBase.instance as ClientSimulationManager).lastAckedServerTick >= currentPlayingAudio[i].frameCreated)
                            {
                                currentPlayingAudio[i] = new SimAudioDefinition()
                                {
                                    frameCreated = currentPlayingAudio[i].frameCreated,
                                    confirmed = true,
                                    position = currentPlayingAudio[i].position,
                                    clip = currentPlayingAudio[i].clip,
                                    source = currentPlayingAudio[i].source
                                };
                                Debug.Log($"Audio confirmed {currentPlayingAudio[i].confirmed}. {currentPlayingAudio[i].frameCreated} while the latest confirmed frame is " +
                                    $"{(SimulationManagerBase.instance as ClientSimulationManager).lastAckedServerTick}.");
                            }
                            return currentPlayingAudio[i].source;
                        }
                    }
                }
                return CreateAudioSource(
                    SimulationManagerBase.instance.CurrentRollbackTick,
                    clip, 
                    position, 
                    ((float)SimulationManagerBase.instance.CurrentTick-(float)SimulationManagerBase.instance.CurrentRollbackTick)/60.0f);
            }
        }

        private static AudioSource CreateAudioSource(int frame, AudioClip clip, Vector3 position, float audioTime = 0.0f)
        {
            AudioSource audioSource = new GameObject(audioSourceName, new System.Type[] { typeof(AudioSource) }).GetComponent<AudioSource>();
            audioSource.clip = clip;
            audioSource.time = audioTime;
            audioSource.Play();
            if (NetworkServer.active == false)
            {
                currentPlayingAudio.Add(new SimAudioDefinition()
                {
                    frameCreated = frame,
                    confirmed = false,
                    position = position,
                    clip = clip,
                    source = audioSource
                });
            }
            return audioSource;
        }

        public static void Cleanup()
        {
            for(int i = currentPlayingAudio.Count-1; i >= 0; i--)
            {
                if(currentPlayingAudio[i].confirmed == false 
                    && (SimulationManagerBase.instance as ClientSimulationManager).lastAckedServerTick > currentPlayingAudio[i].frameCreated)
                {
                    Debug.Log($"Audio was not confirmed. {currentPlayingAudio[i].frameCreated} but already acked " +
                        $"{(SimulationManagerBase.instance as ClientSimulationManager).lastAckedServerTick}.");
                    //currentPlayingAudio[i].source.Stop();
                    //GameObject.Destroy(currentPlayingAudio[i].source.gameObject);
                    currentPlayingAudio.RemoveAt(i);
                    continue;
                }

                if(currentPlayingAudio[i].confirmed == true
                    && (SimulationManagerBase.instance.CurrentTick - currentPlayingAudio[i].frameCreated) > 20)
                {
                    currentPlayingAudio.RemoveAt(i);
                    continue;
                }
            }
        }
    }

    public struct SimAudioDefinition
    {
        public int frameCreated;
        public bool confirmed;
        public Vector3 position;
        public AudioClip clip;
        public AudioSource source;
    }
}