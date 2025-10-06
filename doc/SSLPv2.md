# Simple Session Layer Protocol version 2

## Overview

Version 2 of Simple Session Layer Protocol is an evolution, which made usage of available header bytes more efficient, thus now it is required significantly shorter header to transmit the same amout of data comparing to version 1.

General idea was to replace simple sum of header bytes with treating the header as base 255 number, which represent length of a payload contained by the packet.

## Packet structure

Patch of data (packet payload) sent through the socket is preceded by fixed length header, which is base 255 representation of payload length.

**Example 1:** Lest's assume, that header length is equal to three bytes and recipient received  following sequence of bytes: [0, 0] (decimal notation). Length of received data is smaller than header length, which means that only part of packet was received. Recipient needs to wait for remaining part of data.

**Example 2:** Lest's assume, that header length is equal to two bytes and recipient received  following sequence of bytes: [0, 5, 5, 1, 2] (decimal notation). Sequence can be spited into header part, which are first 2 bytes [0, 5] and payload part [5, 1, 2], which proceeds the header. If header will be treated as base 255 number, its value is equal to 5 (0 * 255^1 + 5). Payload consists of only 2 bytes, which means that recipient received partial packet and needs to wait for missing 3 bytes.

**Example 2:** Lest's assume, that header length is equal to two bytes and recipient received  following sequence of bytes: [0 4 3 1 2 3] (decimal notation). Sequence can be spited into header part, which are first 2 bytes [0 4] and payload part [3 1 2 3], which proceeds the header. If header will be treated as base 255 number, its value is equal to 4 (0 * 255^1 + 4). Payload length is 4 bytes, which is equal value represented by the header, which means that packet was fully received. Recipient can process it further.

## Authors

* Jakub Miodunka
  * [GitHub](https://github.com/JakubMiodunka "GitHub profile")
  * [LinkedIn](https://www.linkedin.com/in/jakubmiodunka/ "LinkedIn profile")
