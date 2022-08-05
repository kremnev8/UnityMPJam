using System.Collections.Generic;
using System.Linq;
using Gameplay.Conrollers;
using Gameplay.Controllers.Player;
using Gameplay.Core;
using Mirror;
using UnityEngine;

namespace Gameplay.Controllers
{
    public class RoleController : NetworkBehaviour
    {
        private GameModel model;

        [SyncVar]
        public PlayerController iceMagePlayer;
        
        [SyncVar]
        public PlayerController fireMagePlayer;

        public void Start()
        {
            model = Simulation.GetModel<GameModel>();
            model.roleController = this;
            model.globalInventory = GetComponent<Inventory>();
            DontDestroyOnLoad(gameObject);
        }

        public PlayerRole GetNextRole()
        {
            if (iceMagePlayer == null)
            {
                return PlayerRole.ICE_MAGE;
            }else if (fireMagePlayer == null)
            {
                return PlayerRole.FIRE_MAGE;
            }

            return PlayerRole.FIRE_MAGE;
        }

        [Server]
        public void SetPlayerRole(PlayerController player, PlayerRole role)
        {
            if (role == PlayerRole.ICE_MAGE)
            {
                if (iceMagePlayer == player) return;

                (iceMagePlayer, fireMagePlayer) = (fireMagePlayer, iceMagePlayer);

                iceMagePlayer = player;

                if (iceMagePlayer != null)
                    iceMagePlayer.role = PlayerRole.ICE_MAGE;
                
                if (fireMagePlayer != null)
                    fireMagePlayer.role = PlayerRole.FIRE_MAGE;
            }
            else
            {
                if (fireMagePlayer == player) return;
                
                (iceMagePlayer, fireMagePlayer) = (fireMagePlayer, iceMagePlayer);

                fireMagePlayer = player;

                if (iceMagePlayer != null)
                    iceMagePlayer.role = PlayerRole.ICE_MAGE;
                
                if (fireMagePlayer != null)
                    fireMagePlayer.role = PlayerRole.FIRE_MAGE;
            }
        }
        
        [Client]
        public void SetPlayerRole(PlayerRole role)
        {
            CmdSetPlayerRole(role);
        }

        [Command(requiresAuthority = false)]
        public void CmdSetPlayerRole(PlayerRole role, NetworkConnectionToClient sender = null)
        {
            PlayerController clientController = sender.identity.GetComponent<PlayerController>();
            SetPlayerRole(clientController, role);
        }
        
    }
}