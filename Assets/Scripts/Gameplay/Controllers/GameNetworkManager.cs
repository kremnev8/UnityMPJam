using System.Linq;
using Gameplay.Conrollers;
using Gameplay.Core;
using Mirror;
using UnityEngine;

namespace Gameplay.Controllers
{
    public class GameNetworkManager : NetworkManager
    {
        public RoleController roleController;


        [Scene]
        public string lobbyScene;
        [Scene]
        public string gameScene;

        public override void OnServerAddPlayer(NetworkConnectionToClient conn)
        {
            base.OnServerAddPlayer(conn);
            
            Debug.Log($"Player ready {conn.address}, identity: {conn.identity == null}");
            PlayerController controller = conn.identity.gameObject.GetComponent<PlayerController>();
            roleController.SetPlayerRole(controller, roleController.GetNextRole());
        }
    }
}