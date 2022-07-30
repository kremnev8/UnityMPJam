using System;
using Epic.OnlineServices.Lobby;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Attribute = Epic.OnlineServices.Lobby.Attribute;

namespace Gameplay.UI.Lobby
{
    public class LobbyItem : MonoBehaviour
    {
        public TMP_Text lobbyNameText;
        public TMP_Text playerCount;

        public Button joinButton;

        private LobbyDetails lobby;
        private LobbyUI lobbyUI;
        private float timeElapsed;

        public void SetLobby(LobbyUI lobbyUI, LobbyDetails lobby)
        {
            this.lobbyUI = lobbyUI;
            this.lobby = lobby;
            Refresh();
        }

        private void Refresh()
        {
            if (lobby != null)
            {
                lobby.CopyAttributeByKey(new LobbyDetailsCopyAttributeByKeyOptions { AttrKey = lobbyUI.AttributeKeys[0] }, out Attribute lobbyNameAttribute);

                string lobbyName = lobbyNameAttribute.Data.Value.AsUtf8;
                if (lobbyName.Length > 30)
                {
                    lobbyName = lobbyName.Substring(0, 27).Trim() + "...";
                }

                uint memberCount = lobby.GetMemberCount(new LobbyDetailsGetMemberCountOptions { });

                lobbyNameText.text = lobbyName;
                playerCount.text = memberCount.ToString();
                joinButton.interactable = memberCount < 2;
            }

            timeElapsed = 0;
        }

        public void Join()
        {
            lobbyUI.JoinLobby(lobby, lobbyUI.AttributeKeys);
        }

        private void Update()
        {
            timeElapsed += Time.deltaTime;
            if (timeElapsed > 1)
            {
                Refresh();
            }
        }
    }
}