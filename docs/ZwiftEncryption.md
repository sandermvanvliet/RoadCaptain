# Zwift Encrypted Communications

Uses AES/GCM/NoPadding
Uses BouncyCastle

Key: 128 bits => 16 bytes
IV: 96 bits => 12 bytes
Tag: 32 bits => 4 bytes (this is AAD)

## Key generation

The key is a 128-bit AES key which is Base64 encoded and sent as part of the `relay` payload to the Zwift API to initialize the connection between RoadCaptain and Zwift.

## Initialization Vector

There are two initialization vectors:

- Client to game (server)
- Game (server) to client

IV is built up with device type and channel type like this:

| Offset | Value | Description |
|--------|-------|-----------|
|  0  | 0 |  |
|  1  | 0 |  |
|  2  | 0 |  device type |
|  3  | 0 |  device type |
|  4  | 0 |  channel type |
|  5  | 0 |  channel type |
|  6  | 0 |  connection id |
|  7  | 0 |  connection id |
|  8  | 0 | counter |
|  9  | 0 | counter |
| 10  | 0 | counter |
| 11  | 0 | counter |

The counter bytes are always zero when the IV is created.

After every decrypt and encrypt operation the app increments the int value starting at offset 8 of the respective IV.

## Encryption

When the encryption starts there is a check whether the current connection is encrypted and a connection id has been set by a previous decrypt operation.

Steps in encryption:

- Allocate the encrypted output buffer to the size of `input + 1 + 4`
- Write a `0` byte to the encrypted output buffer
- Allocate a `tag` buffer of 1 byte
- Copy byte at offset 1 from `encryptedOutput` buffer to `tag` buffer
- Create AES/GCM/NoPadding cipher with the key and a 32 bit tag (AAD) size and the IV bytes

## Decryption

- Read single byte from input
- Do some bit twiddling and check if the result is `0`
- 