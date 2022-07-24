using System;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using RoadCaptain.Ports;

namespace RoadCaptain.Adapters
{
    internal class ZwiftCrypto : IZwiftCrypto
    {
        private readonly byte[]? _key;
        private InitializationVector _clientToGameInitializationVector;
        private InitializationVector _gameToClientInitializationVector;
        private bool _hasConnectionId;
        private bool _isEncryptedConnection;
        private int _relayId;

        private readonly bool a = false;
        private readonly bool b = false;
        private readonly bool c = false;

        public ZwiftCrypto(IUserPreferences userPreferences)
        {
            _key = userPreferences.ConnectionSecret;
            _clientToGameInitializationVector = new InitializationVector(ChannelType.TcpServer);
            _gameToClientInitializationVector = new InitializationVector(ChannelType.TcpClient);
        }

        public byte[] Encrypt(byte[] inputMessage)
        {
            // If no key is defined then bypass encryption
            if (_key == null)
            {
                return inputMessage;
            }

            var input = new ByteBuffer(inputMessage);

            ByteBuffer? encryptedOutput = null;

            if (input.Remaining != 0)
            {
                if (_isEncryptedConnection && !_hasConnectionId)
                {
                    throw new Exception("Connection id expected but missing");
                }

                var i = 0;
                var i2 = (a ? 4 : 0) + 1 + (b ? 2 : 0); // a and b are always false zo i2 is always 1
                if (c)
                {
                    // c is always false so i is 0
                    i = 4;
                }

                var i3 = i2 + i; // i3 = 1
                var remaining = input.Remaining + i3 + 4; // remaining bytes + 5
                if (encryptedOutput == null)
                {
                    encryptedOutput = ByteBuffer.Allocate(remaining);
                }

                var position = encryptedOutput.Position;
                encryptedOutput.Put(GenerateHeaderByte()); // as a, b and c are false this puts a 0 byte to the encrypted output
                if (a)
                {
                    encryptedOutput.PutInt(_relayId);
                }

                if (b)
                {
                    encryptedOutput.PutShort((short)_clientToGameInitializationVector.GetConnectionId());
                }

                if (c)
                {
                    encryptedOutput.PutInt(_clientToGameInitializationVector.GetCounter());
                }

                var additionalAuthenticationData = ByteBuffer.Allocate(i3); // this allocates a 1-byte ByteBuffer
                var position2 = encryptedOutput.Position; // position2 = 1
                var limit = encryptedOutput.Limit; // limit is total size of the buffer
                encryptedOutput.Position = position; // position = 1 because we've written one 0-byte previously
                encryptedOutput.Limit = position + i3; // limit is 1 + 1 = 2
                additionalAuthenticationData
                    .Put(encryptedOutput); // this writes from position -> limit from enryptedOutput to allocate
                encryptedOutput.Limit = limit;
                encryptedOutput.Position = position2; // this ensures the encryption skips the first byte
                additionalAuthenticationData
                    .Flip(); // this sets limit to the current position of allocate and sets position to 0
                // Effectively it's 0 -> 1 now because only 1 byte has been written to allocate

                var cipher = CipherUtilities.GetCipher("AES/GCM/NoPadding");

                cipher.Init(true,
                    new AeadParameters(new KeyParameter(_key), 32, _clientToGameInitializationVector.GetBytes(),
                        additionalAuthenticationData.ToArray()));

                var encryptedPayloadOutput = new byte[inputMessage.Length + 4];

                cipher.DoFinal(input.ToArray(), encryptedPayloadOutput, 0);

                encryptedOutput.Put(new ByteBuffer(encryptedPayloadOutput));

                _clientToGameInitializationVector.IncrementCounter();

                return encryptedOutput;
            }

            throw new Exception("Empty message");
        }

        public byte[] Decrypt(byte[] inputMessage)
        {
            // If no key is defined then bypass decryption
            if (_key == null)
            {
                return inputMessage;
            }

            var input = new ByteBuffer(inputMessage);

            var position = input.Position;
            var firstByte = AABitTwiddle(input.GetByte());

            if (ProtocolVersionIsZero(firstByte))
            {
                if (HasRelayId(firstByte))
                {
                    if (input.Remaining >= 4)
                    {
                        if (input.GetInt() != _relayId)
                        {
                            throw new Exception("Relay id does not match");
                        }
                    }
                    else
                    {
                        throw new Exception("Relay id announced but missing");
                    }
                }

                if (HasConnectionId(firstByte))
                {
                    if (input.Remaining >= 2)
                    {
                        var a2 = BABitTwiddle(input.GetShort());
                        if (a2 != _gameToClientInitializationVector.GetConnectionId())
                        {
                            InitInitializationVectors((short)a2);
                        }

                        _hasConnectionId = true;
                    }
                    else
                    {
                        throw new Exception("Connection id announced but missing");
                    }
                }
                else if (_isEncryptedConnection && !_hasConnectionId)
                {
                    throw new Exception("Connection id expected but missing");
                }

                if (HasCounter(firstByte))
                {
                    if (input.Remaining >= 4)
                    {
                        _gameToClientInitializationVector.SetCounter(CABitTwiddle(input.GetInt()));
                    }
                    else
                    {
                        throw new Exception("Sequence number announced but missing");
                    }
                }

                var remaining = input.Remaining;
                ByteBuffer decryptedOutput;

                if (remaining > 0)
                {
                    var position2 = input.Position;
                    var i = position2 - position;
                    var c = remaining - 4;
                    decryptedOutput = ByteBuffer.Allocate(c);
                    var additionalAuthenticationData = ByteBuffer.Allocate(i);
                    var limit = input.Limit;
                    input.Position = position;
                    input.Limit = position + i;
                    additionalAuthenticationData.Put(input);
                    input.Limit = limit;
                    input.Position = position2;
                    additionalAuthenticationData.Flip();

                    var cipher = CipherUtilities.GetCipher("AES/GCM/NoPadding");

                    cipher.Init(false,
                        new AeadParameters(new KeyParameter(_key), 32, _gameToClientInitializationVector.GetBytes(),
                            additionalAuthenticationData.ToArray()));

                    cipher.DoFinal(input.ToArray(), decryptedOutput, 0);
                }
                else
                {
                    decryptedOutput = new ByteBuffer(Array.Empty<byte>());
                }

                _gameToClientInitializationVector.IncrementCounter();

                return decryptedOutput;
            }

            throw new Exception($"Unsupported protocol version {FBitTwiddle(firstByte)}");
        }

        private void InitInitializationVectors(short connectionId)
        {
            _clientToGameInitializationVector = new InitializationVector(ChannelType.TcpServer);
            _clientToGameInitializationVector.SetConnectionId(connectionId);
            _gameToClientInitializationVector = new InitializationVector(ChannelType.TcpClient);
            _gameToClientInitializationVector.SetConnectionId(connectionId);
        }

        private static int AABitTwiddle(byte value)
        {
            return value & 255;
        }

        private static int BABitTwiddle(short value)
        {
            return value & 65535;
        }

        private static long CABitTwiddle(int value)
        {
            return value & 4294967295L;
        }

        private byte GenerateHeaderByte()
        {
            return (byte)((a ? 4 : 0) | 0 | (b ? 2 : 0) | (c ? 1 : 0));
        }

        private static int FBitTwiddle(int i)
        {
            return (i & 240) >> 4;
        }

        private static bool HasConnectionId(int i)
        {
            return (i & 2) != 0;
        }

        private static bool HasRelayId(int i)
        {
            return (i & 4) != 0;
        }

        private static bool HasCounter(int i)
        {
            return (i & 1) != 0;
        }

        private static bool ProtocolVersionIsZero(int i)
        {
            return FBitTwiddle(i) == 0;
        }
    }
}