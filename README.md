# tcp-client-server

## Description

Repository contains libraries, which cane be used during development of client-server application to utilize efficient communication between one main server and many clients.
Transferring data over developed socket wrappers is based on consumption of consecutive data patches from queues dedicated for sending and receiving data over established connection.
Solution is capable to transfer encrypted data in full-duplex manner (to send and receive data simultaneously).

It was created for educational purposes to gain knowledge in following fields:

* low level socket programming
* asynchronous programming
* basics of cryptography
* GuitHub projects and issues system

## Repository Structure

* */doc* - Contains project documentation.
* */src* - Contains project source code.

## Source Code Structure

* */src/Server* - Library, which can be used to utilize communication with clients on server site.

* */src/Client* - Library, which can be used to utilize communication with server on client site.

* */src/Common* - Library containing functionalities used both by server and client related libraries.

* */src/UnitTests* - Source code of unit tests.

* */src/IntegrationTests* - Source code of integration tests.

## Implementation Details

### Data transmission

Data transfer between clients and server is utilized with [TCP](https://en.wikipedia.org/wiki/Transmission_Control_Protocol "Wikipedia article") sockets programmed on relatively low level (mainly for educational purposes rather than some more relevant benefits).

Project requirements specified, that transmission shall be encrypted, but implementation commercial-grade solution was not necessary (as it'a more educational project), so to make things more convenient it was decided to use some sort of [symmetric-key algorithm](https://en.wikipedia.org/wiki/Symmetric-key_algorithm "Wikipedia article"), where encryption key is stored locally as one of configuration value. Finally [TEA algorithm](https://en.wikipedia.org/wiki/Tiny_Encryption_Algorithm "Wikipedia article") was chosen as its implementation was easy. Architecture of communication cipher was designed to be modular, so changing the encryption method in the future shall not be a problem.

TEA algorithm is operating on fixed-length data blocks, so additionally some sort of padding function was required - [PKCS algorithm](https://www.ibm.com/docs/en/zos/2.4.0?topic=rules-pkcs-padding-method "IBM documentation") was introduced. It was not embedded in communication cipher, but implemented as separate entity, so it can be used also for other purposes if necessary or easily change for other padding method.

TCP sockets are stream-oriented and can fragment transferred data in (more or less) unpredicted way. To handle communication on 5th [ISO/OSI](https://en.wikipedia.org/wiki/OSI_model "Wikipedia article") layer properly, it is required to use some kind of mechanism, which will check, if full patch of data was already transferred. For sake of this purpose Simple Session Layer Protocol (SSLP) was invented and used within the project. Its description is available in separate section of this README.

## Simple Session Layer Protocol

### Overview

When dealing with TCP sockets it is not guaranteed, that exactly one call of Socket. Sent on the sender site will result in exactly one call of Socket.Receive on the recipient site - there is a need to check if particular patch of data was received fully or partially.

Above mentioned need was the origin of Simple Session Layer Protocol (SSLP) - trivial and easy to implement protocol operating on [session (5th) layer](https://en.wikipedia.org/wiki/Session_layer "Wikipedia article") of [OSI model](https://en.wikipedia.org/wiki/OSI_model "Wikipedia article"), created for sake of this project.

### Packet structure

Patch of data (packet payload) sent through the socket is preceded by fixed length header. Sum of bytes creating a header indicates the length of the payload.

**Example 1:** Lest's assume, that header length is equal to three bytes and recipient received  following sequence of bytes: [0, 0] (decimal notation). Length of received data is smaller than header length, which means that only part of packet was received. Recipient needs to wait for remaining part of data.

**Example 2:** Lest's assume, that header length is equal to three bytes and recipient received  following sequence of bytes: [0, 0, 5, 1, 2] (decimal notation). Sequence can be spited into header part, which are first 3 bytes [0, 0, 5] and payload part [1, 2], which proceeds the header. Sum of bytes creating a header is equal to 5, but payload consists of only 2 bytes, which means that recipient received partial packet and needs to wait for missing 3 bytes.

**Example 2:** Lest's assume, that header length is equal to three bytes and recipient received  following sequence of bytes: [0 0 3 1 2 3] (decimal notation). Sequence can be spited into header part, which are first 3 bytes [0 0 3] and payload part [1 2 3], which proceeds the header. Sum of bytes creating a header is equal to 3 and payload consists of only 3 bytes, which means that packet was fully received. Recipient can process int further.

## Used Tools

* IDE: [Visual Studio 2022](https://visualstudio.microsoft.com/vs/ "Visual Studio website")
* Documentation generator: [DoxyGen 1.12.0](https://www.doxygen.nl/ "DoxyGen website")

## Authors

* Jakub Miodunka
  * [GitHub](https://github.com/JakubMiodunka "GitHub profile")
  * [LinkedIn](https://www.linkedin.com/in/jakubmiodunka/ "LinkedIn profile")

## License

This project is licensed under the MIT License - see the [*LICENSE.md*](./LICENSE "Licence") file for details.
