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

        public byte[] Encrypt(byte[] inputMessage)
        {
            var input = new ByteBuffer(inputMessage);

            ByteBuffer? encryptedOutput = null;

            if (input != null && input.remaining() != 0)
            {
                if (isEncryptedConnection && !hasConnectionId)
                {
                    throw new Exception("Connection id expected but missing");
                }

                int i = 0;
                int i2 = (a ? 4 : 0) + 1 + (b ? 2 : 0); // a and b are always false zo i2 is always 1
                if (c)
                {
                    // c is always false so i is 0
                    i = 4;
                }

                int i3 = i2 + i; // i3 = 1
                int remaining = input.remaining() + i3 + 4; // remaining bytes + 5
                if (encryptedOutput == null)
                {
                    encryptedOutput = ByteBuffer.allocate(remaining);
                }

                int position = encryptedOutput.position();
                encryptedOutput.put(e()); // as a, b and c are false this puts a 0 byte to the encrypted output
                if (a)
                {
                    encryptedOutput.putInt(relayId);
                }

                if (b)
                {
                    encryptedOutput.putShort((short)_clientToGameInitializationVector.getConnectionId());
                }

                if (c)
                {
                    encryptedOutput.putInt((int)_clientToGameInitializationVector.getCounter());
                }

                ByteBuffer additionalAuthenticationData = ByteBuffer.allocate(i3); // this allocates a 1-byte ByteBuffer
                int position2 = encryptedOutput.position(); // position2 = 1
                int limit = encryptedOutput.limit(); // limit is total size of the buffer
                encryptedOutput.position(position); // position = 1 because we've written one 0-byte previously
                encryptedOutput.limit(position + i3); // limit is 1 + 1 = 2
                additionalAuthenticationData.put(encryptedOutput); // this writes from position -> limit from enryptedOutput to allocate
                encryptedOutput.limit(limit);
                encryptedOutput.position(position2); // this ensures the encryption skips the first byte
                additionalAuthenticationData.flip(); // this sets limit to the current position of allocate and sets position to 0
                // Effectively it's 0 -> 1 now because only 1 byte has been written to allocate
                
                var cipher = CipherUtilities.GetCipher("AES/GCM/NoPadding");
                    
                cipher.Init(true,
                    new AeadParameters(new KeyParameter(_key), 32, _clientToGameInitializationVector, additionalAuthenticationData.array()));

                var encryptedPayloadOutput = new byte[inputMessage.Length + 4];

                cipher.DoFinal(input.array(), encryptedPayloadOutput, 0);

                encryptedOutput.put(new ByteBuffer(encryptedPayloadOutput));
                
                _monitoringEvents.Information(
                    "Encrypted using key: {Key} and IV: {IV}: data: {Data}",
                    Convert.ToBase64String(_key),
                    Convert.ToBase64String((byte[])_clientToGameInitializationVector),
                    Convert.ToBase64String((byte[])encryptedOutput));

                _clientToGameInitializationVector.incrementCounter();

                _monitoringEvents.Information("Incremented IV counter to {Count}", _clientToGameInitializationVector.getCounter());

                return encryptedOutput;
            }

            throw new Exception("Empty message");
        }

        public byte[]? Decrypt(byte[] inputMessage)
        {
            var input = new ByteBuffer(inputMessage);

            ByteBuffer? decryptedOutput = null;
            
            int position = input.position();
            int a = AABitTwiddle(input.get());

            if (BitTwiddle(a)) {
                if (HasRelayId(a)) {
                    if (input.remaining() >= 4) {
                        if (input.getInt() != relayId) {
                            throw new Exception("Relay id does not match");
                        }
                    } else {
                        throw new Exception("Relay id announced but missing");
                    }
                }
                if (HasConnectionId(a)) {
                    if (input.remaining() >= 2) {
                        var a2 = BABitTwiddle(input.getShort());
                        if (a2 != _gameToClientInitializationVector.getConnectionId()) {
                            initInitializationVectors((short)a2);
                        }
                        hasConnectionId = true;
                    } else {
                        throw new Exception("Connection id announced but missing");
                    }
                } else if (isEncryptedConnection && !hasConnectionId) {
                    throw new Exception("Connection id expected but missing");
                }
                if (HasCounter(a)) {
                    if (input.remaining() >= 4) {
                        _gameToClientInitializationVector.setCounter(CABitTwiddle(input.getInt()));
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

                _monitoringEvents.Information(
                    "Decrypted using key: {Key}, input: {Input} and IV: {IV} => data: {Data}",
                    Convert.ToBase64String(_key),
                    Convert.ToBase64String(inputMessage),
                    Convert.ToBase64String((byte[])_gameToClientInitializationVector),
                    Convert.ToBase64String((byte[])decryptedOutput));

                _gameToClientInitializationVector.incrementCounter();

                _monitoringEvents.Information("Incremented IV counter to {Count}", _gameToClientInitializationVector.getCounter());

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
            return (byte) ((a ? 4 : 0) | 0 | (b ? 2 : 0) | (c ? 1 : 0));
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