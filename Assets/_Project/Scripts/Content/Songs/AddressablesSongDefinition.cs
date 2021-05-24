using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using System;

namespace Mahou.Content
{
    [CreateAssetMenu(fileName = "AddressablesSongDefinition", menuName = "Mahou/Content/Addressables/SongDefinition")]
    public class AddressablesSongDefinition : ISongDefinition
    {
        public override string Name { get { return songName; } }
        public override string Description { get { return description; } }

        [SerializeField] private string songName;
        [SerializeField] [TextArea] private string description;
        [SerializeField] private AssetReferenceT<ScriptableObject> songReference;

        [NonSerialized] private ScriptableObject song;

        public override async UniTask<bool> LoadSong()
        {
            if (song != null)
            {
                return true;
            }

            // Load song
            try
            {
                var loadResult = await Addressables.LoadAssetAsync<ScriptableObject>(songReference).Task;
                song = loadResult;
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                return false;
            }
        }

        public override Song GetSong()
        {
            return (Song)song;
        }

        public override async void UnloadSong()
        {
            song = null;
            Addressables.Release(songReference);
        }
    }
}