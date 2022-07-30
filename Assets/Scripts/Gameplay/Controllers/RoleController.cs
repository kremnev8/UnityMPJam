using System.Linq;
using Gameplay.Conrollers;
using Gameplay.Core;
using Mirror;
using UnityEngine;

namespace Gameplay.Controllers
{
    public class RoleController : NetworkBehaviour
    {
        private GameModel model;

        public PlayerController pastPlayer;
        public PlayerController futurePlayer;

        
        public void Start()
        {
            model = Simulation.GetModel<GameModel>();
        }

        public PlayerRole GetNextRole()
        {
            if (pastPlayer == null)
            {
                return PlayerRole.PAST;
            }else if (futurePlayer == null)
            {
                return PlayerRole.FUTURE;
            }

            return PlayerRole.FUTURE;
        }

        [Server]
        public void SetPlayerRole(PlayerController player, PlayerRole role)
        {
            if (role == PlayerRole.PAST)
            {
                if (pastPlayer == player) return;

                (pastPlayer, futurePlayer) = (futurePlayer, pastPlayer);

                pastPlayer = player;

                if (pastPlayer != null)
                    pastPlayer.role = PlayerRole.PAST;
                
                if (futurePlayer != null)
                    futurePlayer.role = PlayerRole.FUTURE;
            }
            else
            {
                if (futurePlayer == player) return;
                
                (pastPlayer, futurePlayer) = (futurePlayer, pastPlayer);

                futurePlayer = player;

                if (pastPlayer != null)
                    pastPlayer.role = PlayerRole.PAST;
                
                if (futurePlayer != null)
                    futurePlayer.role = PlayerRole.FUTURE;
            }
        }
        
        [Client]
        public void SetPlayerRole(PlayerRole role)
        {
            CmdSetPlayerRole(role);
        }
        
        [ClientRpc]
        public void RpcUpdateWaitUI()
        {
            model.lobbyUI.UpdateWaitUI();
        }

        [Command(requiresAuthority = false)]
        public void CmdSetPlayerRole(PlayerRole role, NetworkConnectionToClient sender = null)
        {
            PlayerController clientController = sender.identity.GetComponent<PlayerController>();
            SetPlayerRole(clientController, role);
            RpcUpdateWaitUI();
        }
        
    }
}