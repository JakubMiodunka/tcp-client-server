# Data transfer

## Overview

Data transfer between clients and server is utilized with [TCP](https://en.wikipedia.org/wiki/Transmission_Control_Protocol "Wikipedia article") sockets programmed on relatively low level
(mainly for educational purposes rather than some more relevant benefits).

Project requirements specified, that transmission shall be encrypted, but implementation commercial-grade solution was not necessary (as it'a more educational project), so to make things more convenient it was decided to use some sort of [symmetric-key algorithm](https://en.wikipedia.org/wiki/Symmetric-key_algorithm "Wikipedia article"), where encryption key is stored locally as one of configuration value.
Finally [TEA algorithm](https://en.wikipedia.org/wiki/Tiny_Encryption_Algorithm "Wikipedia article") was chosen as its implementation was easy. Architecture of communication cipher was designed to be modular, so changing the encryption method in the future shall not be a problem.

TEA algorithm is operating on fixed-length data blocks, so additionally some sort of padding function was required - [PKCS algorithm](https://www.ibm.com/docs/en/zos/2.4.0?topic=rules-pkcs-padding-method "IBM documentation") was introduced. It was not embedded in communication cipher, but implemented as separate entity, so it can be used also for other purposes if necessary or easily change for other padding method.

To reliably handle communication at the [session (5th) layer](https://en.wikipedia.org/wiki/Session_layer "Wikipedia article") of the [OSI model](https://en.wikipedia.org/wiki/OSI_model "Wikipedia article") and ensure a full message has been transferred, we require a mechanism to delimit data packets. For sake of this purpose Simple Session Layer Protocol (SSLP) was invented - its description is available in
*/doc/SSLPv1.md* and */doc/SSLPv2.md* files, depending on protocol version.

## Impact on maximal size of single message

It is worth to mention, that maximal length of payload, which can be handled by [session layer](https://en.wikipedia.org/wiki/Session_layer "Wikipedia article") protocol has impact on maximal length of message, which can be sent through established connection at once (in one call).

For example, when data is encrypted using [TEA algorithm](https://en.wikipedia.org/wiki/Tiny_Encryption_Algorithm "Wikipedia article") and 5th layer utilize SSLP, which header is 4-byte-long, maximal payload length is 889 bytes. Header limits maximal packet length to 1020 bytes - 889 bytes of payload encrypted using [TEA algorithm](https://en.wikipedia.org/wiki/Tiny_Encryption_Algorithm "Wikipedia article") have a length of exactly 1020 bytes.

## Authors

* Jakub Miodunka
  * [GitHub](https://github.com/JakubMiodunka "GitHub profile")
  * [LinkedIn](https://www.linkedin.com/in/jakubmiodunka/ "LinkedIn profile")