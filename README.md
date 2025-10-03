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

* */doc/html* - Contains HTML based documentation of this project created automatically using [DoxyGen](https://www.doxygen.nl/ "DoxyGen website").

* */src* - Contains project source code.

## Source Code Structure

* */src/Server* - Library, which can be used to utilize communication with clients on server site.

* */src/Client* - Library, which can be used to utilize communication with server on client site.

* */src/Common* - Library containing functionalities used both by server and client related libraries.

* */src/UnitTests* - Source code of unit tests.

* */src/IntegrationTests* - Source code of integration tests.

## Project versioning

Versioning for this project follows the established guidelines of [semantic versioning](https://en.m.wikipedia.org/wiki/Software_versioning#Semantic_versioning "Wikipedia article").

Current version: 1.0.0

## Project management

Project development is tracked and managed using GitHub project available under following link: [tcp-client-server](https://github.com/users/JakubMiodunka/projects/4 "Link to tcp-client-server project").
If You spotted some bug or have a suggestion feel free to create corresponding issue there.

## Used Tools

* IDE: [Visual Studio 2022](https://visualstudio.microsoft.com/vs/ "Visual Studio website")
* Documentation generator: [DoxyGen 1.12.0](https://www.doxygen.nl/ "DoxyGen website")

## Authors

* Jakub Miodunka
  * [GitHub](https://github.com/JakubMiodunka "GitHub profile")
  * [LinkedIn](https://www.linkedin.com/in/jakubmiodunka/ "LinkedIn profile")

## License

This project is licensed under the MIT License - see the [*LICENSE.md*](./LICENSE "Licence") file for details.
