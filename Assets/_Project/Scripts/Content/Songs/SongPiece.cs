using UnityEngine;

namespace Mahou.Content
{
    [CreateAssetMenu(fileName = "SongPiece", menuName = "Mahou/Content/Song/Piece")]
    public class SongPiece : ScriptableObject
    {
        public float bpm;
        public float firstBeatOffset;
        public AudioClip song;

        public SongPieceChart leadChart;
    }
}