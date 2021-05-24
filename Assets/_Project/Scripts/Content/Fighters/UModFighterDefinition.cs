using Cysharp.Threading.Tasks;
using Mahou.Debugging;
using Mahou.Managers;
using NaughtyAttributes;
using System;
using UMod;
using UnityEngine;

namespace Mahou.Content.Fighters
{
    [CreateAssetMenu(fileName = "UModFighterDefinition", menuName = "Mahou/Content/UMod/FighterDefinition")]
    public class UModFighterDefinition : IFighterDefinition
    {
        public override string Name { get { return fighterName; } }
        public override string Description { get { return description; } }
        public override bool Selectable { get { return selectable; } }

        [SerializeField] private string fighterName;
        [SerializeField] [TextArea] private string description;
        [SerializeField] private bool selectable = true;
        [SerializeField] private ModObjectSharedReference modNamespace;
        [SerializeField] private string fighterPath;
        [SerializeField] private string[] movesetPaths;
        [SerializeField] private string fighterGuid;

        [NonSerialized] private GameObject fighter;
        [NonSerialized] private MovesetDefinition[] movesets;

        [Button(text: "Generate GUID")]
        public void GenerateGUID()
        {
            fighterGuid = System.Guid.NewGuid().ToString();
        }

        public override async UniTask<bool> LoadFighter()
        {
            ModHost modHost = ModManager.instance.ModLoader.loadedMods[modNamespace.reference.modIdentifier].host;
            
            // Load fighter.
            if (modHost.Assets.Exists(fighterPath) == false)
            {
                ConsoleWindow.current.WriteLine($"Fighter does not exist at path {fighterPath} for {modHost.name}.");
                return false;
            }
            ModAsyncOperation request = modHost.Assets.LoadAsync(fighterPath);
            await request;
            fighter = request.Result as GameObject;

            // Load movesets.
            movesets = new MovesetDefinition[movesetPaths.Length];
            for(int i = 0; i < movesetPaths.Length; i++)
            {
                if (modHost.Assets.Exists(movesetPaths[i]) == false)
                {
                    ConsoleWindow.current.WriteLine($"Fighter does not exist at path {movesetPaths[i]} for {modHost.name}.");
                    return false;
                }
                ModAsyncOperation movesetRequest = modHost.Assets.LoadAsync(movesetPaths[i]);
                await movesetRequest;
                movesets[i] = movesetRequest.Result as MovesetDefinition;
            }

            return true;
        }

        public override GameObject GetFighter()
        {
            return fighter;
        }

        public override string GetFighterGUID()
        {
            return fighterGuid;
        }

        public override MovesetDefinition[] GetMovesets()
        {
            return movesets;
        }

        public override void UnloadFighter()
        {
            fighter = null;
            movesets = null;
        }
    }
}