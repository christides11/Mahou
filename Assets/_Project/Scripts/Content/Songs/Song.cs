using UnityEngine;

namespace Mahou.Content
{
    [CreateAssetMenu(fileName = "Song", menuName = "Mahou/Content/Song/Definition")]
    public class Song : ScriptableObject
    {
        public SongPiece introduction;
        public SongPiece verse;
        public SongPiece[] choruses = new SongPiece[0];
        public SongPiece outro;
    }
}