using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Soulvan.Multiplayer
{
    /// <summary>
    /// Online multiplayer manager with text chat, voice call, and video call.
    /// Supports cross-platform play (PC, PlayStation, Xbox, Android).
    /// Uses WebRTC for voice/video streaming.
    /// </summary>
    public class MultiplayerManager : MonoBehaviour
    {
        [Header("Session Configuration")]
        [SerializeField] private string sessionId;
        [SerializeField] private string localPlayerId;
        [SerializeField] private int maxPlayers = 8;
        [SerializeField] private bool isHost = false;

        [Header("Connected Players")]
        [SerializeField] private List<PlayerInfo> connectedPlayers = new List<PlayerInfo>();

        [Header("Communication Settings")]
        [SerializeField] private bool voiceChatEnabled = true;
        [SerializeField] private bool videoChatEnabled = false;
        [SerializeField] private bool textChatEnabled = true;

        [Header("Voice/Video Quality")]
        [SerializeField] private AudioQuality audioQuality = AudioQuality.High;
        [SerializeField] private VideoQuality videoQuality = VideoQuality.Medium;

        [Header("Chat History")]
        [SerializeField] private List<ChatMessage> chatHistory = new List<ChatMessage>();
        [SerializeField] private int maxChatHistory = 100;

        // WebRTC connections for each player
        private Dictionary<string, WebRTCConnection> webrtcConnections = new Dictionary<string, WebRTCConnection>();

        // Events
        public event Action<PlayerInfo> OnPlayerJoined;
        public event Action<PlayerInfo> OnPlayerLeft;
        public event Action<ChatMessage> OnChatMessageReceived;
        public event Action<string, bool> OnVoiceStateChanged; // playerId, isTalking
        public event Action<string, Texture2D> OnVideoFrameReceived; // playerId, frame

        private void Start()
        {
            InitializeMultiplayer();
        }

        /// <summary>
        /// Initialize multiplayer system.
        /// </summary>
        private void InitializeMultiplayer()
        {
            localPlayerId = SystemInfo.deviceUniqueIdentifier;
            Debug.Log($"[MultiplayerManager] Initialized with player ID: {localPlayerId}");
        }

        /// <summary>
        /// Create a new multiplayer session.
        /// </summary>
        public async Task<string> CreateSession(string cityLocation, int maxPlayers = 8)
        {
            sessionId = GenerateSessionId();
            this.maxPlayers = maxPlayers;
            isHost = true;

            Debug.Log($"[MultiplayerManager] Creating session {sessionId} in {cityLocation}");

            // Add local player as host
            var hostPlayer = new PlayerInfo
            {
                playerId = localPlayerId,
                username = GetLocalUsername(),
                platform = GetCurrentPlatform(),
                isHost = true,
                location = cityLocation,
                isReady = false
            };

            connectedPlayers.Add(hostPlayer);

            // Stub: Register session with matchmaking server
            await Task.Delay(500);

            Debug.Log($"[MultiplayerManager] Session created: {sessionId}");
            return sessionId;
        }

        /// <summary>
        /// Join an existing multiplayer session.
        /// </summary>
        public async Task<bool> JoinSession(string sessionId, string cityLocation)
        {
            this.sessionId = sessionId;
            isHost = false;

            Debug.Log($"[MultiplayerManager] Joining session {sessionId}");

            try
            {
                // Stub: Connect to matchmaking server
                await Task.Delay(1000);

                var localPlayer = new PlayerInfo
                {
                    playerId = localPlayerId,
                    username = GetLocalUsername(),
                    platform = GetCurrentPlatform(),
                    isHost = false,
                    location = cityLocation,
                    isReady = false
                };

                connectedPlayers.Add(localPlayer);

                // Notify other players
                OnPlayerJoined?.Invoke(localPlayer);

                Debug.Log($"[MultiplayerManager] Joined session {sessionId}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[MultiplayerManager] Failed to join session: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Leave current session.
        /// </summary>
        public async Task LeaveSession()
        {
            Debug.Log($"[MultiplayerManager] Leaving session {sessionId}");

            // Close all WebRTC connections
            foreach (var connection in webrtcConnections.Values)
            {
                connection.Close();
            }
            webrtcConnections.Clear();

            // Remove local player
            var localPlayer = connectedPlayers.Find(p => p.playerId == localPlayerId);
            if (localPlayer != null)
            {
                connectedPlayers.Remove(localPlayer);
                OnPlayerLeft?.Invoke(localPlayer);
            }

            // Stub: Disconnect from server
            await Task.Delay(500);

            sessionId = null;
            Debug.Log($"[MultiplayerManager] Left session");
        }

        /// <summary>
        /// Add player to session (called by host).
        /// </summary>
        public void AddPlayer(PlayerInfo playerInfo)
        {
            if (connectedPlayers.Count >= maxPlayers)
            {
                Debug.LogWarning($"[MultiplayerManager] Session full, cannot add player {playerInfo.username}");
                return;
            }

            if (!connectedPlayers.Exists(p => p.playerId == playerInfo.playerId))
            {
                connectedPlayers.Add(playerInfo);
                OnPlayerJoined?.Invoke(playerInfo);

                Debug.Log($"[MultiplayerManager] Player joined: {playerInfo.username} ({playerInfo.platform})");

                // Send welcome message
                SendSystemMessage($"{playerInfo.username} joined the session");
            }
        }

        /// <summary>
        /// Remove player from session.
        /// </summary>
        public void RemovePlayer(string playerId)
        {
            var player = connectedPlayers.Find(p => p.playerId == playerId);
            if (player != null)
            {
                connectedPlayers.Remove(player);
                OnPlayerLeft?.Invoke(player);

                // Close WebRTC connection
                if (webrtcConnections.ContainsKey(playerId))
                {
                    webrtcConnections[playerId].Close();
                    webrtcConnections.Remove(playerId);
                }

                Debug.Log($"[MultiplayerManager] Player left: {player.username}");
                SendSystemMessage($"{player.username} left the session");
            }
        }

        #region Text Chat

        /// <summary>
        /// Send text chat message to all players.
        /// </summary>
        public void SendChatMessage(string message)
        {
            if (!textChatEnabled)
            {
                Debug.LogWarning("[MultiplayerManager] Text chat is disabled");
                return;
            }

            var chatMessage = new ChatMessage
            {
                senderId = localPlayerId,
                senderName = GetLocalUsername(),
                message = message,
                timestamp = DateTime.Now,
                messageType = ChatMessageType.Text
            };

            chatHistory.Add(chatMessage);
            TrimChatHistory();

            OnChatMessageReceived?.Invoke(chatMessage);

            Debug.Log($"[MultiplayerManager] Chat: {chatMessage.senderName}: {message}");

            // Stub: Broadcast to all players via server
        }

        /// <summary>
        /// Send system message (e.g., "Player joined").
        /// </summary>
        private void SendSystemMessage(string message)
        {
            var chatMessage = new ChatMessage
            {
                senderId = "system",
                senderName = "System",
                message = message,
                timestamp = DateTime.Now,
                messageType = ChatMessageType.System
            };

            chatHistory.Add(chatMessage);
            TrimChatHistory();

            OnChatMessageReceived?.Invoke(chatMessage);
        }

        /// <summary>
        /// Receive chat message from another player.
        /// </summary>
        public void ReceiveChatMessage(ChatMessage message)
        {
            chatHistory.Add(message);
            TrimChatHistory();

            OnChatMessageReceived?.Invoke(message);

            Debug.Log($"[MultiplayerManager] Received chat: {message.senderName}: {message.message}");
        }

        /// <summary>
        /// Get chat history.
        /// </summary>
        public List<ChatMessage> GetChatHistory()
        {
            return new List<ChatMessage>(chatHistory);
        }

        /// <summary>
        /// Trim chat history to max size.
        /// </summary>
        private void TrimChatHistory()
        {
            if (chatHistory.Count > maxChatHistory)
            {
                chatHistory.RemoveRange(0, chatHistory.Count - maxChatHistory);
            }
        }

        #endregion

        #region Voice Chat

        /// <summary>
        /// Enable/disable voice chat.
        /// </summary>
        public void SetVoiceChat(bool enabled)
        {
            voiceChatEnabled = enabled;

            if (enabled)
            {
                StartVoiceChat();
            }
            else
            {
                StopVoiceChat();
            }

            Debug.Log($"[MultiplayerManager] Voice chat {(enabled ? "enabled" : "disabled")}");
        }

        /// <summary>
        /// Start voice chat with all players.
        /// </summary>
        private async void StartVoiceChat()
        {
            Debug.Log("[MultiplayerManager] Starting voice chat");

            // Initialize WebRTC audio streams for each player
            foreach (var player in connectedPlayers)
            {
                if (player.playerId == localPlayerId) continue;

                if (!webrtcConnections.ContainsKey(player.playerId))
                {
                    var connection = new WebRTCConnection(player.playerId, true, false);
                    webrtcConnections[player.playerId] = connection;
                    await connection.Connect();
                }
            }

            // Stub: Start capturing microphone
            Debug.Log("[MultiplayerManager] Voice chat started");
        }

        /// <summary>
        /// Stop voice chat.
        /// </summary>
        private void StopVoiceChat()
        {
            Debug.Log("[MultiplayerManager] Stopping voice chat");

            // Stub: Stop capturing microphone

            foreach (var connection in webrtcConnections.Values)
            {
                connection.DisableAudio();
            }

            Debug.Log("[MultiplayerManager] Voice chat stopped");
        }

        /// <summary>
        /// Mute/unmute local microphone.
        /// </summary>
        public void SetMicrophoneMuted(bool muted)
        {
            // Stub: Mute/unmute microphone
            Debug.Log($"[MultiplayerManager] Microphone {(muted ? "muted" : "unmuted")}");
        }

        #endregion

        #region Video Chat

        /// <summary>
        /// Enable/disable video chat.
        /// </summary>
        public void SetVideoChat(bool enabled)
        {
            videoChatEnabled = enabled;

            if (enabled)
            {
                StartVideoChat();
            }
            else
            {
                StopVideoChat();
            }

            Debug.Log($"[MultiplayerManager] Video chat {(enabled ? "enabled" : "disabled")}");
        }

        /// <summary>
        /// Start video chat with all players.
        /// </summary>
        private async void StartVideoChat()
        {
            Debug.Log("[MultiplayerManager] Starting video chat");

            // Initialize WebRTC video streams for each player
            foreach (var player in connectedPlayers)
            {
                if (player.playerId == localPlayerId) continue;

                if (!webrtcConnections.ContainsKey(player.playerId))
                {
                    var connection = new WebRTCConnection(player.playerId, true, true);
                    webrtcConnections[player.playerId] = connection;
                    await connection.Connect();
                }
                else
                {
                    webrtcConnections[player.playerId].EnableVideo();
                }
            }

            // Stub: Start capturing webcam
            Debug.Log("[MultiplayerManager] Video chat started");
        }

        /// <summary>
        /// Stop video chat.
        /// </summary>
        private void StopVideoChat()
        {
            Debug.Log("[MultiplayerManager] Stopping video chat");

            // Stub: Stop capturing webcam

            foreach (var connection in webrtcConnections.Values)
            {
                connection.DisableVideo();
            }

            Debug.Log("[MultiplayerManager] Video chat stopped");
        }

        /// <summary>
        /// Set video quality.
        /// </summary>
        public void SetVideoQuality(VideoQuality quality)
        {
            videoQuality = quality;
            Debug.Log($"[MultiplayerManager] Video quality set to {quality}");

            // Stub: Adjust video bitrate/resolution
        }

        #endregion

        #region Utility

        /// <summary>
        /// Generate unique session ID.
        /// </summary>
        private string GenerateSessionId()
        {
            return $"SS-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
        }

        /// <summary>
        /// Get local player username (stub).
        /// </summary>
        private string GetLocalUsername()
        {
            // Stub: Fetch from player profile
            return $"Player_{UnityEngine.Random.Range(1000, 9999)}";
        }

        /// <summary>
        /// Get current platform.
        /// </summary>
        private Platform GetCurrentPlatform()
        {
            switch (Application.platform)
            {
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.LinuxPlayer:
                case RuntimePlatform.OSXPlayer:
                    return Platform.PC;
                case RuntimePlatform.PS5:
                    return Platform.PlayStation;
                case RuntimePlatform.XboxOne:
                    return Platform.Xbox;
                case RuntimePlatform.Android:
                    return Platform.Android;
                default:
                    return Platform.PC;
            }
        }

        /// <summary>
        /// Get all connected players.
        /// </summary>
        public List<PlayerInfo> GetConnectedPlayers()
        {
            return new List<PlayerInfo>(connectedPlayers);
        }

        #endregion
    }

    #region Data Structures

    /// <summary>
    /// Player information.
    /// </summary>
    [Serializable]
    public class PlayerInfo
    {
        public string playerId;
        public string username;
        public Platform platform;
        public bool isHost;
        public string location; // City name
        public bool isReady;
        public int tier;
        public float ping;
    }

    /// <summary>
    /// Chat message.
    /// </summary>
    [Serializable]
    public class ChatMessage
    {
        public string senderId;
        public string senderName;
        public string message;
        public DateTime timestamp;
        public ChatMessageType messageType;
    }

    /// <summary>
    /// Platform enum.
    /// </summary>
    public enum Platform
    {
        PC,
        PlayStation,
        Xbox,
        Android
    }

    /// <summary>
    /// Chat message type.
    /// </summary>
    public enum ChatMessageType
    {
        Text,
        System,
        Voice,
        Video
    }

    /// <summary>
    /// Audio quality settings.
    /// </summary>
    public enum AudioQuality
    {
        Low,      // 16 kbps
        Medium,   // 32 kbps
        High      // 64 kbps
    }

    /// <summary>
    /// Video quality settings.
    /// </summary>
    public enum VideoQuality
    {
        Low,      // 480p
        Medium,   // 720p
        High      // 1080p
    }

    /// <summary>
    /// WebRTC connection for voice/video (stub implementation).
    /// </summary>
    public class WebRTCConnection
    {
        private string peerId;
        private bool audioEnabled;
        private bool videoEnabled;

        public WebRTCConnection(string peerId, bool audioEnabled, bool videoEnabled)
        {
            this.peerId = peerId;
            this.audioEnabled = audioEnabled;
            this.videoEnabled = videoEnabled;
        }

        public async Task Connect()
        {
            Debug.Log($"[WebRTC] Connecting to peer {peerId}");
            await Task.Delay(500);
            Debug.Log($"[WebRTC] Connected to peer {peerId}");
        }

        public void Close()
        {
            Debug.Log($"[WebRTC] Closing connection to peer {peerId}");
        }

        public void EnableVideo()
        {
            videoEnabled = true;
            Debug.Log($"[WebRTC] Video enabled for peer {peerId}");
        }

        public void DisableVideo()
        {
            videoEnabled = false;
            Debug.Log($"[WebRTC] Video disabled for peer {peerId}");
        }

        public void DisableAudio()
        {
            audioEnabled = false;
            Debug.Log($"[WebRTC] Audio disabled for peer {peerId}");
        }
    }

    #endregion
}
