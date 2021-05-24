using UnityEngine;

namespace Mahou.Content
{
    [CreateAssetMenu(fileName = "Song", menuName = "Mahou/Content/Song")]
    public class Song : ScriptableObject
    {
        public float bpm;
        public float firstBeatOffset;
        public AudioClip song;
    }
}