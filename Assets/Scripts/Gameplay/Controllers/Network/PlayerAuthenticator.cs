using System;
using UnityEngine;
using System.Collections;
using Gameplay.Core;
using Gameplay.UI.Lobby;
using Mirror;
using UnityEngine.Events;
using Random = UnityEngine.Random;

namespace Gameplay.Controllers.Network
{
    public class PlayerAuthenticator : NetworkAuthenticator
    {
        private LobbyUI lobby;

        private void Start()
        {
            lobby = Simulation.GetModel<GameModel>().lobbyUI;
        }

        public event Action<NetworkConnectionToClient, string> OnAuthenticationResult;

        public struct AuthRequestMessage : NetworkMessage
        {
            public string authUsername;
        }

        public struct AuthResponseMessage : NetworkMessage
        {
            public byte code;
            public string message;
        }

        public override void OnStartServer()
        {
            // register a handler for the authentication request we expect from client
            NetworkServer.RegisterHandler<AuthRequestMessage>(OnAuthRequestMessage, false);
        }

        public override void OnStartClient()
        {
            // register a handler for the authentication response we expect from server
            NetworkClient.RegisterHandler<AuthResponseMessage>(OnAuthResponseMessage, false);
            
        }

        public override void OnServerAuthenticate(NetworkConnectionToClient conn)
        {
            // do nothing...wait for AuthRequestMessage from client
        }
        
        public override void OnClientAuthenticate()
        {
            Debug.Log("Trying to Auth");
            string username = "";
            
            username = lobby.GetPlayerName();
            if (Application.isEditor)
            {
                username = $"Player {Random.Range(0, 25565)}";
            }


            AuthRequestMessage authRequestMessage = new AuthRequestMessage
            {
                authUsername = username
            };
            
            NetworkClient.Send(authRequestMessage);
        }

        public static bool CheckUsername(string username)
        {
            return true;
        }


        public void OnAuthRequestMessage(NetworkConnectionToClient conn, AuthRequestMessage msg)
        {
            // check the credentials by calling your web server, database table, playfab api, or any method appropriate.
            if (CheckUsername(msg.authUsername))
            {
                // create and send msg to client so it knows to proceed
                AuthResponseMessage authResponseMessage = new AuthResponseMessage
                {
                    code = 100,
                    message = "Success!"
                };

                conn.Send(authResponseMessage);
                // Invoke the event to complete a successful authentication
                OnServerAuthenticated.Invoke(conn);
                OnAuthenticationResult?.Invoke(conn, msg.authUsername);
            }
            else
            {
                // create and send msg to client so it knows to disconnect
                AuthResponseMessage authResponseMessage = new AuthResponseMessage
                {
                    code = 200,
                    message = "Username is invalid!"
                };

                conn.Send(authResponseMessage);

                // must set NetworkConnection isAuthenticated = false
                conn.isAuthenticated = false;

                // disconnect the client after 1 second so that response message gets delivered
                StartCoroutine(DelayedDisconnect(conn, 1));
            }
        }

        public IEnumerator DelayedDisconnect(NetworkConnectionToClient conn, float waitTime)
        {
            yield return new WaitForSeconds(waitTime);
            conn.Disconnect();
        }
        
        
        public void OnAuthResponseMessage(AuthResponseMessage msg)
        {
            //lobby.respText.text = msg.message;
            if (msg.code == 100)
            {
                // Invoke the event to complete a successful authentication
                OnClientAuthenticated.Invoke();
            }
            /*else
            {
                // Set this on the client for local reference
                conn.isAuthenticated = false;

                // disconnect the client
                conn.Disconnect();
            }*/
        }
    }
}