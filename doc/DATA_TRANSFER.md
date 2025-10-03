# Data transfer

## Overview

Data transfer between clients and server is utilized with [TCP](https://en.wikipedia.org/wiki/Transmission_Control_Protocol "Wikipedia article") sockets programmed on relatively low level
(mainly for educational purposes rather than some more relevant benefits).

Project requirements specified, that transmission shall be encrypted, but implementation commercial-grade solution was not necessary (as it'a more educational project), so to make things more convenient it was decided to use some sort of [symmetric-key algorithm](https://en.wikipedia.org/wiki/Symmetric-key_algorithm "Wikipedia article"), where encryption key is stored locally as one of configuration value.
Finally [TEA algorithm](https://en.wikipedia.org/wiki/Tiny_Encryption_Algorithm "Wikipedia article") was chosen as its implementation was easy. Architecture of communication cipher was designed to be modular, so changing the encryption method in the future shall not be a problem.

TEA algorithm is operating on fixed-length data blocks, so additionally some sort of padding function was required - [PKCS algorithm](https://www.ibm.com/docs/en/zos/2.4.0?topic=rules-pkcs-padding-method "IBM documentation") was introduced. It was not embedded in communication cipher, but implemented as separate entity, so it can be used also for other purposes if necessary or easily change for other padding method.

To reliably handle communication at the [session (5th) layer](https://en.wikipedia.org/wiki/Session_layer "Wikipedia article") of the [OSI model](https://en.wikipedia.org/wiki/OSI_model "Wikipedia article") and ensure a full message has been transferred, we require a mechanism to delimit data packets. For sake of this purpose Simple Session Layer Protocol (SSLP) was invented - its description is available in */doc/SSLP.md* file.

## Authors

* Jakub Miodunka
  * [GitHub](https://github.com/JakubMiodunka "GitHub profile")
  * [LinkedIn](https://www.linkedin.com/in/jakubmiodunka/ "LinkedIn profile")