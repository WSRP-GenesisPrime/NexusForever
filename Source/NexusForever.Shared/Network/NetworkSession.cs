﻿using System;
using System.Net;
using System.Net.Sockets;
using NexusForever.Shared.Game.Events;
using NexusForever.Shared.Network.Static;
using NLog;

namespace NexusForever.Shared.Network
{
    public abstract class NetworkSession : IUpdate
    {
        protected static readonly ILogger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Unique id for <see cref="NetworkSession"/>.
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// <see cref="IEvent"/> queue that will be processed during <see cref="NetworkSession"/> update.
        /// </summary>
        public EventQueue Events { get; } = new();

        /// <summary>
        /// Heartbeat to check if <see cref="NetworkSession"/> is still alive.
        /// </summary>
        /// <remarks>
        /// If <see cref="SocketHeartbeat"/> flatlines the <see cref="NetworkSession"/> will be disconnected.
        /// </remarks>
        public SocketHeartbeat Heartbeat { get; } = new();

        public DateTime AcceptTime { get; private set; }

        private Socket socket;
        private readonly byte[] buffer = new byte[4096];
        private int bufferOffset;

        private DisconnectState? disconnectState;

        /// <summary>
        /// Initialise <see cref="NetworkSession"/> with new <see cref="Socket"/> and begin listening for data.
        /// </summary>
        public virtual void OnAccept(Socket newSocket)
        {
            if (socket != null)
                throw new InvalidOperationException();

            AcceptTime = DateTime.Now;

            Id = Guid.NewGuid().ToString();

            socket = newSocket;
            socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, ReceiveDataCallback, null);

            log.Trace($"New client {Id} connected from {newSocket.RemoteEndPoint}.");
        }

        /// <summary>
        /// Update <see cref="NetworkSession"/> existing id with a new supplied id.
        /// </summary>
        /// <remarks>
        /// This should be used when the default session id can be replaced with a known unique id.
        /// </remarks>
        public void UpdateId(string id)
        {
            log.Trace($"Client {Id} updated id to {id}.");
            Id = id;
        }

        /// <summary>
        /// Invoked each world tick with the delta since the previous tick occurred.
        /// </summary>
        public virtual void Update(double lastTick)
        {
            Events.Update(lastTick);

            if (!disconnectState.HasValue)
                Heartbeat.Update(lastTick);

            if ((Heartbeat.Flatline && disconnectState != DisconnectState.Complete) || disconnectState == DisconnectState.Pending)
            {
                // no defibrillator is going to save this session
                if (Heartbeat.Flatline)
                    log.Trace($"Client {Id} has flatlined.");

                OnDisconnect();
            }
        }

        public virtual void ReportLoginFinish()
        {
            log.Trace($"New session; login took {DateTime.Now.Subtract(AcceptTime).TotalMilliseconds} ms.");
        }

        protected virtual void OnDisconnect()
        {
            EndPoint remoteEndPoint = socket.RemoteEndPoint;
            socket.Close();

            log.Trace($"Client {Id} disconnected. {remoteEndPoint}");

            disconnectState = DisconnectState.Complete;
        }

        /// <summary>
        /// Returns if <see cref="NetworkSession"/> can be disposed.
        /// </summary>
        public virtual bool CanDispose()
        {
            return disconnectState == DisconnectState.Complete && !Events.PendingEvents;
        }

        /// <summary>
        /// Invoked with <see cref="IAsyncResult"/> when data from the <see cref="Socket"/> is received.
        /// </summary>
        private void ReceiveDataCallback(IAsyncResult ar)
        {
            try
            {
                int length = socket.EndReceive(ar);
                if (length == 0)
                {
                    ForceDisconnect();
                    return;
                }

                byte[] data = new byte[length + bufferOffset];
                Buffer.BlockCopy(buffer, 0, data, 0, data.Length);
                bufferOffset = (int)OnData(data);

                // if we have data that wasn't processed move it to the start of the buffer
                // any new data will be amended to it
                if (bufferOffset != 0)
                    Buffer.BlockCopy(buffer, data.Length - bufferOffset, buffer, 0, bufferOffset);

                socket.BeginReceive(buffer, bufferOffset, buffer.Length - bufferOffset, SocketFlags.None, ReceiveDataCallback, null);
            }
            catch
            {
                ForceDisconnect();
            }
        }

        protected abstract uint OnData(byte[] data);

        /// <summary>
        /// Send supplied data to remote client on <see cref="Socket"/>.
        /// </summary>
        protected void SendRaw(byte[] data)
        {
            try
            {
                socket.Send(data, 0, data.Length, SocketFlags.None);
            }
            catch
            {
                ForceDisconnect();
            }
        }

        public bool IsLocalIp()
        {
            IPEndPoint ep = socket.RemoteEndPoint as IPEndPoint;
            byte[] bytes = ep.Address.GetAddressBytes();
            return (bytes[0] == 192 && bytes[1] == 168);
        }

        /// <summary>
        /// Forece disconnect of <see cref="NetworkSession"/>.
        /// </summary>
        public void ForceDisconnect()
        {
            if (disconnectState.HasValue)
                return;

            disconnectState = DisconnectState.Pending;
        }
    }
}
