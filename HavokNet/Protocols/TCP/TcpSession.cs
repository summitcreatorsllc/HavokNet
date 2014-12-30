using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PcapDotNet.Packets;

namespace HavokNet.Protocols.TCP
{
    public class TcpSession
    {
        public const int TIMEOUT = 5;

        public TcpSession(TcpClient client, Common.IPv4Address myip, ushort myport)
        {
            _client = client;

            SequenceNumber = 0;
            AcknowledgementNumber = 0;

            Source = myip;
            Queue = new ConcurrentQueue<Common.PacketData>();
            SourcePort = myport;

            State = TcpState.LISTEN;

        }

        private TcpClient _client { get; set; }
        public volatile uint AcknowledgementNumber;
        public volatile uint SequenceNumber;
        public Common.IPv4Address Source { get; set; }
        public string Destination { get; set; }
        public ushort SourcePort { get; set; }
        public ushort DestinationPort { get; set; }
        public TcpState State { get; set; }

        public void Connect(string destination, ushort port)
        {
            SequenceNumber = (ushort)_client._rnd.Next(0, (int)ushort.MaxValue);
            AcknowledgementNumber = 0;

            Destination = destination;
            DestinationPort = port;

            OSI.Layer4TcpPacket packet = new OSI.Layer4TcpPacket();
            packet.Destination = destination;
            packet.RemotePort = port;
            packet.LocalPort = SourcePort;
            packet.SequenceNumber = SequenceNumber;
            packet.AcknowledgementNumber = AcknowledgementNumber;
            packet.SYN = true;
            State = TcpState.SYN_SENT;
            _client.SendPacket(packet);

            DateTime now = DateTime.Now;
            while ((DateTime.Now - now).TotalSeconds < TIMEOUT)
            {
                if (State == TcpState.ESTABLISHED) return;
                System.Threading.Thread.Sleep(25);
            }

            throw new Exception("TCP Timeout.");
        }
        public void Disconnect()
        {
            State = TcpState.CLOSED;
            AcknowledgementNumber = 0;
            SequenceNumber = 0;
            
        }

        public delegate void PacketReceivedHandler(Common.PacketData packet);
        public event PacketReceivedHandler PacketReceived;
        public ConcurrentQueue<Common.PacketData> Queue { get; set; }
        public void AddPacketToQueue(Common.PacketData pdata)
        {
            if (Queue.Count == 0) HandlePacket(pdata);
            else Queue.Enqueue(pdata);
        }
        public void HandlePacket(Common.PacketData pdata)
        {
            

            var ip = pdata.Packet.Ethernet.IpV4;
            var tcp = ip.Tcp;
            if (State == TcpState.SYN_SENT)
            {
                if (tcp.IsSynchronize && tcp.IsAcknowledgment)
                {
                    // BAM. SYN-ACK received, TCP handshake complete after we send
                    // this ACK
                    State = TcpState.ESTABLISHED;
                    AcknowledgementNumber = tcp.SequenceNumber + 1;
                    SequenceNumber++;
                    SendAck();
                }
                else
                {
                    // BAD. Nothing to do here, becuase they are stupid.
                }
            }
            else if (State == TcpState.ESTABLISHED)
            {
                if (tcp.PayloadLength != 0)
                {
                    AcknowledgementNumber += (uint)tcp.PayloadLength;

                    PacketReceived(pdata);
                    SendAck();
                }
                if (tcp.IsFin)
                {
                    CloseConnection();
                }
            }

            if (Queue.Count != 0)
            {
                Common.PacketData p = null;
                Queue.TryDequeue(out p);
                if (p != null) HandlePacket(p);
            }
        }

        public void SendPacket(TcpPacket packet)
        {
            try
            {
                OSI.Layer4TcpPacket l4 = new OSI.Layer4TcpPacket();
                l4.PSH = true;
                l4.ACK = true;
                l4.RemotePort = DestinationPort;
                l4.LocalPort = SourcePort;
                l4.SequenceNumber = SequenceNumber;
                l4.AcknowledgementNumber = AcknowledgementNumber;
                l4.Destination = Destination;

                int length = 0;
                foreach (ILayer layer in packet.Layers)
                {
                    length += layer.Length;
                    l4.NextLayers.Add(layer);
                }

                _client.SendPacket(l4);
                SequenceNumber += (uint)length;
            }
            catch (Exception ex)
            {
                throw new Exception("No TCP Client is associated with this session.");
            }
        }

        private void SendAck()
        {
            OSI.Layer4TcpPacket l4 = new OSI.Layer4TcpPacket();
            l4.ACK = true;
            l4.RemotePort = DestinationPort;
            l4.LocalPort = SourcePort;
            l4.SequenceNumber = SequenceNumber;
            l4.AcknowledgementNumber = AcknowledgementNumber;
            l4.Destination = Destination;

            _client.SendPacket(l4);
        }
        private void SendFin()
        {
            OSI.Layer4TcpPacket l4 = new OSI.Layer4TcpPacket();
            l4.FIN = true;
            l4.ACK = true;
            l4.RemotePort = DestinationPort;
            l4.LocalPort = SourcePort;
            l4.SequenceNumber = SequenceNumber;
            l4.AcknowledgementNumber = AcknowledgementNumber;
            l4.Destination = Destination;

            _client.SendPacket(l4);
        }

        public delegate void CloseConnectionHandler();
        public event CloseConnectionHandler CloseConnection;
    }
}
