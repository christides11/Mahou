using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Content
{
    [CreateAssetMenu(fileName = "SongPiece", menuName = "Mahou/Content/Song/Chart")]
    public class SongPieceChart : ScriptableObject
    {
        public SongNote[] notes = new SongNote[0];
    }
}