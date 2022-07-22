using System;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using RoadCaptain.Ports;

namespace RoadCaptain.Adapters
{
    public enum ChannelType {
        None,
        UdpClient,
        UdpServer,
        TcpClient,
        TcpServer
    }

    public class ZwiftCrypto : IZwiftCrypto
    {
        private readonly byte[] _key;
        private InitializationVector _clientToGameInitializationVector;
        private InitializationVector _gameToClientInitializationVector;
        
        public ZwiftCrypto(IUserPreferences userPreferences, MonitoringEvents monitoringEvents)
        {
            _monitoringEvents = monitoringEvents;
            _key = userPreferences.ConnectionSecret;
            _clientToGameInitializationVector = new InitializationVector(ChannelType.TcpServer);
            _gameToClientInitializationVector = new InitializationVector(ChannelType.TcpClient);
        }

        public byte[] Encrypt(byte[] input)
        {
            var encryptedOutput = new byte[input.Length + 1 + 4];
            encryptedOutput[0] = 0x0;
            var tag = new byte[1];
            tag[0] = encryptedOutput[1];

            var cipher = CipherUtilities.GetCipher("AES/GCM/NoPadding");
            
            cipher.Init(true, 
                new AeadParameters(new KeyParameter(_key), 32, _clientToGameInitializationVector));
            
            cipher.DoFinal(input, encryptedOutput, 1);

            _clientToGameInitializationVector.incrementCounter();

            return encryptedOutput;
        }

        public byte[]? Decrypt(byte[] inputMessage)
        {
            _monitoringEvents.Information(
                "Decrypting using key: {Key} and data: {Data}",
                Convert.ToBase64String(_key),
                Convert.ToBase64String(inputMessage));

            var input = new ByteBuffer(inputMessage);

            ByteBuffer? decryptedOutput = null;
            
            int position = input.position();
            int a = AABitTwiddle(input.get());

            if (BitTwiddle(a)) {
                if (HasRelayId(a)) {
                    if (input.remaining() >= 4) {
                        if (input.getInt() != this.relayId) {
                            throw new Exception("Relay id does not match");
                        }
                    } else {
                        throw new Exception("Relay id announced but missing");
                    }
                }
                if (HasConnectionId(a)) {
                    if (input.remaining() >= 2) {
                        var a2 = BABitTwiddle(input.getShort());
                        if (a2 != this._gameToClientInitializationVector.getConnectionId()) {
                            initInitializationVectors((short)a2);
                        }
                        this.hasConnectionId = true;
                    } else {
                        throw new Exception("Connection id announced but missing");
                    }
                } else if (this.isEncryptedConnection && !this.hasConnectionId) {
                    throw new Exception("Connection id expected but missing");
                }
                if (HasCounter(a)) {
                    if (input.remaining() >= 4) {
                        this._gameToClientInitializationVector.setCounter(CABitTwiddle(input.getInt()));
                    } else {
                        throw new Exception("Sequence number announced but missing");
                    }
                }
                int remaining = input.remaining();
                if (remaining > 0) {
                    int position2 = input.position();
                    int i = position2 - position;
                    int c = remaining - 4;
                    if (decryptedOutput == null) {
                        decryptedOutput = ByteBuffer.allocate(c);
                    }
                    ByteBuffer additionalAuthenticationData = ByteBuffer.allocate(i);
                    int limit = input.limit();
                    input.position(position);
                    input.limit(position + i);
                    additionalAuthenticationData.put(input);
                    input.limit(limit);
                    input.position(position2);
                    additionalAuthenticationData.flip();

                    var cipher = CipherUtilities.GetCipher("AES/GCM/NoPadding");
                    
                    cipher.Init(false,
                        new AeadParameters(new KeyParameter(_key), 32, _gameToClientInitializationVector, additionalAuthenticationData.array()));

                    cipher.DoFinal(input.array(), decryptedOutput, 0);
                }

                _gameToClientInitializationVector.incrementCounter();
                return decryptedOutput;
            }

            throw new Exception($"Unsupported protocol version {f(a)}");
        }

        private long CABitTwiddle(int value)
        {
            return value & 4294967295L;
        }

        private void initInitializationVectors(short connectionId)
        {
            _clientToGameInitializationVector = new InitializationVector(ChannelType.TcpServer);
            _clientToGameInitializationVector.setConnectionId(connectionId);
            _gameToClientInitializationVector = new InitializationVector(ChannelType.TcpClient);
            _gameToClientInitializationVector.setConnectionId(connectionId);
        }

        private int BABitTwiddle(short value)
        {
            return value & 65535;
        }

        private int AABitTwiddle(byte value)
        {
            return value & 255;
        }

        private bool a = false;
        private bool b = false;
        private bool c = false;
        private int relayId;
        private bool hasConnectionId;
        private bool isEncryptedConnection;
        private readonly MonitoringEvents _monitoringEvents;

        private byte e() {
            return (byte) ((this.a ? 4 : 0) | 0 | (this.b ? 2 : 0) | (this.c ? 1 : 0));
        }

        private static int f(int i) {
            return (i & 240) >> 4;
        }

        private static bool HasConnectionId(int i) {
            return (i & 2) != 0;
        }

        private static bool HasRelayId(int i) {
            return (i & 4) != 0;
        }

        private static bool HasCounter(int i) {
            return (i & 1) != 0;
        }

        private static bool BitTwiddle(int i) {
            return f(i) == 0;
        }

    }
}