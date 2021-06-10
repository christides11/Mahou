using Mahou.Menus;
using UnityEngine;

namespace Mahou
{
    [CreateAssetMenu(fileName = "GameSettings", menuName = "Config/GameSettings")]
    public class GameSettings : ScriptableObject
    {
        public CharacterSelectMenu characterSelectMenu;
        public PlayerCamera playerCamera;
        public GameObject thirdPersonVirtualCamera;
        public int serverWorldStateSendRate = 60;
        public int simulationRate = 60;
        public int maxRollbackFrames = 20;
    }
}