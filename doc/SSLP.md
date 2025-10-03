# Simple Session Layer Protocol

## Overview

TCP sockets are stream-oriented and can fragment transferred data in (more or less) unpredicted way. They don't guarantee that a single Socket.Send operation will result in a single, complete Socket.Receive on the recipient's side. Data can be fragmented in an unpredictable way across the network.

TCP sockets are stream-oriented and can fragment transferred data in (more or less) unpredicted way. To handle communication on 5th [ISO/OSI](https://en.wikipedia.org/wiki/OSI_model "Wikipedia article") layer properly, it is required to use some kind of mechanism, which will check, if full patch of data was already transferred.

Above mentioned need was the origin of Simple Session Layer Protocol (SSLP) - trivial and easy to implement protocol operating on [session (5th) layer](https://en.wikipedia.org/wiki/Session_layer "Wikipedia article") of [OSI model](https://en.wikipedia.org/wiki/OSI_model "Wikipedia article"), created for sake of this project.

## Packet structure

Patch of data (packet payload) sent through the socket is preceded by fixed length header. Sum of bytes creating a header indicates the length of the payload.

**Example 1:** Lest's assume, that header length is equal to three bytes and recipient received  following sequence of bytes: [0, 0] (decimal notation). Length of received data is smaller than header length, which means that only part of packet was received. Recipient needs to wait for remaining part of data.

**Example 2:** Lest's assume, that header length is equal to three bytes and recipient received  following sequence of bytes: [0, 0, 5, 1, 2] (decimal notation). Sequence can be spited into header part, which are first 3 bytes [0, 0, 5] and payload part [1, 2], which proceeds the header. Sum of bytes creating a header is equal to 5, but payload consists of only 2 bytes, which means that recipient received partial packet and needs to wait for missing 3 bytes.

**Example 2:** Lest's assume, that header length is equal to three bytes and recipient received  following sequence of bytes: [0 0 3 1 2 3] (decimal notation). Sequence can be spited into header part, which are first 3 bytes [0 0 3] and payload part [1 2 3], which proceeds the header. Sum of bytes creating a header is equal to 3 and payload consists of only 3 bytes, which means that packet was fully received. Recipient can process int further.

## Authors

* Jakub Miodunka
  * [GitHub](https://github.com/JakubMiodunka "GitHub profile")
  * [LinkedIn](https://www.linkedin.com/in/jakubmiodunka/ "LinkedIn profile")
