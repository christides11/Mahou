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
                //Debug.Log($"Played on frame {SimulationManagerBase.instance.CurrentRealTick}.");
                return CreateAudioSource(SimulationManagerBase.instance.CurrentRealTick, clip, position, 0.0f);
            }
            else
            {
                //Debug.Log($"ATTEMPTED play on ROLLBACK frame {SimulationManagerBase.instance.CurrentRollbackTick} ({SimulationManagerBase.instance.CurrentTick})." +
                //    $"Last confirmed server tick was {(SimulationManagerBase.instance as ClientSimulationManager).latestAckedServerWorldStateTick}");

                // Check if the clip was played before.
                for(int i = 0; i < currentPlayingAudio.Count; i++)
                {
                    if (currentPlayingAudio[i].clip == clip 
                            && currentPlayingAudio[i].frameCreated == SimulationManagerBase.instance.CurrentRollbackTick 
                            && Vector3.Distance(currentPlayingAudio[i].position, position) < 0.1f){
                        // Confirm the audio if needed.
                        if(currentPlayingAudio[i].frameConfirmed != SimulationManagerBase.instance.CurrentRealTick)
                        {
                            currentPlayingAudio[i] = new SimAudioDefinition()
                            {
                                frameCreated = currentPlayingAudio[i].frameCreated,
                                frameConfirmed = SimulationManagerBase.instance.CurrentRealTick,
                                position = currentPlayingAudio[i].position,
                                clip = currentPlayingAudio[i].clip,
                                source = currentPlayingAudio[i].source
                            };
                            /*Debug.Log($"SUCCESSFUL confirm {currentPlayingAudio[i].frameConfirmed}. " +
                                $"{currentPlayingAudio[i].frameCreated} while the latest confirmed frame is " +
                                $"{(SimulationManagerBase.instance as ClientSimulationManager).latestAckedServerWorldStateTick}.");*/
                        }
                        return currentPlayingAudio[i].source;
                    }
                }

                return CreateAudioSource(
                    SimulationManagerBase.instance.CurrentRollbackTick,
                    clip, 
                    position, 
                    ((float)SimulationManagerBase.instance.CurrentRealTick-(float)SimulationManagerBase.instance.CurrentRollbackTick)/60.0f, true);
            }
        }

        private static AudioSource CreateAudioSource(int frame, AudioClip clip, Vector3 position, float audioTime = 0.0f, bool autoconfirm = false)
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
                    frameConfirmed = autoconfirm ? SimulationManagerBase.instance.CurrentRealTick : frame,
                    position = position,
                    clip = clip,
                    source = audioSource
                });
            }
            return audioSource;
        }

        public static void Cleanup()
        {
            for (int i = currentPlayingAudio.Count - 1; i >= 0; i--)
            {
                if (currentPlayingAudio[i].frameConfirmed != SimulationManagerBase.instance.CurrentRealTick
                    && currentPlayingAudio[i].frameCreated >= (SimulationManagerBase.instance as ClientSimulationManager).latestAckedServerWorldStateTick)
                {
                    //Debug.Log($"Audio was not confirmed. {currentPlayingAudio[i].frameCreated} but already acked " +
                    //    $"{(SimulationManagerBase.instance as ClientSimulationManager).latestAckedServerWorldStateTick}.");
                    currentPlayingAudio[i].source.Stop();
                    GameObject.Destroy(currentPlayingAudio[i].source.gameObject);
                    currentPlayingAudio.RemoveAt(i);
                    continue;
                }

                if((SimulationManagerBase.instance as ClientSimulationManager).latestAckedServerWorldStateTick > currentPlayingAudio[i].frameCreated)
                {
                    currentPlayingAudio.RemoveAt(i);
                }
            }
        }
    }

    public struct SimAudioDefinition
    {
        public int frameCreated;
        public int frameConfirmed;
        public Vector3 position;
        public AudioClip clip;
        public AudioSource source;
    }
}