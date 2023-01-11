using UnityEngine;

#if MIRROR

using Mirror;

#endif

namespace DigitalRuby.WeatherMaker
{
    /// <summary>
    /// Script to assign to players in the game
    /// </summary>
    public class WeatherMakerIsPlayerScript : MonoBehaviour
    {
        [Tooltip("Whether this is a local player, or a network player. For MIRROR networking this is set automatically if there " +
			"is a NetworkIdentity component on this same object.")]
        public bool IsLocalPlayer;

        private void OnEnable()
        {

#if MIRROR

            NetworkIdentity id = GetComponent<NetworkIdentity>();
            if (id != null)
            {
                IsLocalPlayer = id.isLocalPlayer;
            }

#endif

        }
    }
}
