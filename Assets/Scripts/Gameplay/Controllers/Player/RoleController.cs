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

        [SyncVar]
        public PlayerController pastPlayer;
        
        [SyncVar]
        public PlayerController futurePlayer;

        
        public void Start()
        {
            model = Simulation.GetModel<GameModel>();
            model.roleController = this;
            DontDestroyOnLoad(gameObject);
        }

        public Timeline GetNextRole()
        {
            if (pastPlayer == null)
            {
                return Timeline.PAST;
            }else if (futurePlayer == null)
            {
                return Timeline.FUTURE;
            }

            return Timeline.FUTURE;
        }

        [Server]
        public void SetPlayerRole(PlayerController player, Timeline role)
        {
            if (role == Timeline.PAST)
            {
                if (pastPlayer == player) return;

                (pastPlayer, futurePlayer) = (futurePlayer, pastPlayer);

                pastPlayer = player;

                if (pastPlayer != null)
                    pastPlayer.role = Timeline.PAST;
                
                if (futurePlayer != null)
                    futurePlayer.role = Timeline.FUTURE;
            }
            else
            {
                if (futurePlayer == player) return;
                
                (pastPlayer, futurePlayer) = (futurePlayer, pastPlayer);

                futurePlayer = player;

                if (pastPlayer != null)
                    pastPlayer.role = Timeline.PAST;
                
                if (futurePlayer != null)
                    futurePlayer.role = Timeline.FUTURE;
            }
        }
        
        [Client]
        public void SetPlayerRole(Timeline role)
        {
            CmdSetPlayerRole(role);
        }

        [Command(requiresAuthority = false)]
        public void CmdSetPlayerRole(Timeline role, NetworkConnectionToClient sender = null)
        {
            PlayerController clientController = sender.identity.GetComponent<PlayerController>();
            SetPlayerRole(clientController, role);
        }
        
    }
}