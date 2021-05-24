using Mahou.Content;
using UnityEngine;

namespace Mahou.Core
{
    public class MusicMeterComponent : GameModeComponent
    {
        [SerializeField] private AudioSource audioSource;

        public Song song;
        public float secPerBeat;
        public float songPosition;
        public float songPositionInBeats;
        public float dspSongTime;
    }
}
