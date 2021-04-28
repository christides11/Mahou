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
        /// <summary>
        /// If the client should interpolate positions of remote entities.
        /// </summary>
        public bool useClientInterpolation = true;
        /// <summary>
        /// How often the client sends updates to the server.
        /// </summary>
        public int clientTickRate = 60;
        /// <summary>
        /// How often the server sends updates to the client.
        /// </summary>
        public int serverTickRate = 60;
        public int simulationRate = 60;
    }
}