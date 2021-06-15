using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mahou.Content
{
    [CreateAssetMenu(fileName = "SongChart", menuName = "Mahou/Content/Song/Chart")]
    public class SongChart : ScriptableObject
    {
        public SongNote[] notes = new SongNote[0];
    }
}