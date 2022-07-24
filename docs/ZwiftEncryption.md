# Zwift Encrypted Communications

This document describes the Zwift encrypted connection and how to perform the encryption / decryption.

## Key generation

The key is a 128-bit AES key which is Base64 encoded and sent as part of the `relay` payload to the Zwift API to initialize the connection between RoadCaptain and Zwift.

## Encrypted payload structure

The encrypted payload has the following structure:

`| 0x0 | header | encrypted data |`

### `header`

The header is between 1 and 11 bytes depending of the type of connection. It is constructed as follows:

The first byte is a flag that indicates whether this connection is:

- A relay
- Has a connection id
- Has a counter

and is a bitwise OR constructed as: `((isRelay ? 4 : 0) | 0 | (hasConnectionId ? 2 : 0) | (hasCounter ? 1 : 0))`. If none of the options is active the value of the first byte is zero.

Depending on whether any of these options is enabled, the rest of the header is written as (in sequence):

- 4 byte integer: relay id
- 2 byte short: connection id
- 4 byte integer: counter

## Encryption

The encryption used is `AES/GCM/NoPadding` with the following parameters:

| Parameter | Size (bits) | Description |
| -----------|------------|-------------|
| Key | 128 | |
| Initialization Vector (IV) | 96 |
| Tag | 32 | This is also known as AAD or Additional Authentication Data |

The key is generated on start up of the application that wishes to initiate a connection with Zwift and it is supplied as a Base64 encoded string in the `relay` API call payload in the `secret` field.

The IV is described in the next section.

The tag is generated from the header of the encrypted output stream and is between 1 and 12 bytes (see [header](#header) above).

Encryption uses the IV _client to game_. After encryption succeeds the `counter` of the _client to game_ IV is incremented.

## Decryption

Decryption uses the _game to client_ IV and retrieves the tag from the encrypted payload, depending on the options of the connection this is between 1 and 12 bytes.

After decryption succeeds the `counter` of the _game to client_ IV is incremented. 

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

The device type is always `0x2`.

The channel type is `0x3` for _client to game_ and `0x4` for _game to client_.