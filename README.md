# DIAC - Linked Data Authorization

## Introduction
This Github repository is an implementation of authorization for Linked Data. Linked Data is stored in triple stores, according to ontologies. Using SPARQL, a query can be performed on Linked Data. Previously, a SPARQL endpoint could only be fully open or closed. The aim of this Proof of Concept is to provide an approach for applying authorization on Linked Data. Authorization is crucial for the roll-out of federated data sharing based on Linked Data. The code exposes two endpoints secured using iShare. One of them provides predefined SPARQL queries that can be executed on the entire dataset, while the other one allows user-defined queries to be executed on periodically updated subsets of the entire dataset.

## Requirements
- .NET 6.0

## Instructions

### Prerequisites
1.  Get an [iShare] test certificate. You can request one [here] . 
2.  Add the certificate to the authorization register (in this PoC, [Poort8]).
3.  Get a triplestore (in this PoC, [Triply]).

### Getting Authorized
1. Using your certificate, generate a client assertion JWS token. In a production environment this should be done locally, but for tests you can use the iShare endpoint [/testing/generate-authorize-request] .
2. In this PoC the authorization register is Poort8, so the [Common] endpoints of the iShare scheme from Poort8 are used, using their docker container. You can pull the docker container using:
    ```sh
    docker pull ghcr.io/poort8/poort8.ishare.common:latest
    ```
   Use the token received before as *client_assertion* in the POST request **/api/Token** to receive an access token back. The container contains also the capabilities endpoint **/api/Capabilities** that can be executed to receive the *capabilities_token*. You can then decode this token (such as in [JWT]) to explore all the capabilities of the user.
3. Set the following variables in the configuration:
   - ConnectionStrings > The base URL of your SPARQL endpoint, your query paths and your authentication header value (Bearer token) in the triplestore. If you are using Triply, you can generate your token via: User settings > API tokens > Create token.
   - ClientId > Your EORI (as a user), for example: EU.EORI.NL888888881.
   - SchemeOwnerUrl > Use the iSHARE scheme owner url of the test environment: https://scheme.isharetest.net.
   - SchemeOwnerIdentifier > Use the EORI of the iSHARE scheme owner: EU.EORI.NL000000000.
   - Certificate > Your iSHARE test certificate as a byte stream, extracted from the certificate file (p12 or pfx).
   - CertificatePassword > The password of the iSHARE test certificate.
   - CertificateChain > The certificates of the [iSHARE Test CA]: as a comma separated byte stream. In case your certificate is issued by *C=NL, O=iSHARE Foundation, CN=TEST iSHARE Foundation PKIoverheid Organisatie Server CA - G3*, you can use [this] chain.
   - CertificateChainPassword > Empty string for public test certificates: ""
   - AuthorizationRegistryDelegationUrl > The URL of your delegation evidence in the authorization registry, that is "https://api.poort8.nl/ar-preview/playbook/{{Playbook}}/ishare/delegation"  for Poort8.
   - AuthorizationRegistryTokenUrl > The token URL of the authorization registry, that is "https://api.poort8.nl/ar-preview/ishare/connect/token" for Poort8.
   - AuthorizationRegistryIdentifier >  The EORI of the authorization registry.
   - Playbook > Your playbook name.
4. After building the application, you are navigated to a localhost address in your default browser. This is a Swagger UI containing the main endpoints of this project.
5. Use the *access_token* from step 2 in the Authorize field of Swagger UI. If everything goes well, you are now authorized! The token is valid for about an hour and you should be able to use the rest of the endpoints during that time. In case you call the APIs outside Swagger, use the access token as Authorization header in your request.

### SPARQL Query APIs
There are 2 APIs available that can be used to execute SPARQL queries and receive Linked Data as a response.
- In the first endpoint **/diac/GetData/{Concept}/{Id}** you can provide a *concept*, *id* and *attribute*. 
  - In case you use the API outside swagger, provide the concept and id in the path and add the attribute as parameter. According to these values, a default construct query will be executed through the triplestore and the respective Linked Data will be received as response.
  - By default, the data is received in standard JSON-LD format. If you wish to receive the data in framed JSON-LD instead, you can set the *framed* parameter to true.
  - Triply returns only the first page back when executing a query, meaning that you can get up to 10,000 results. In case you want to receive all the data from Triply at once, you can set the *pagination* parameter to true. 
  - In the data part of the code you can add all the default construct queries, in the form of the examples provided. For each query, an RQ file containing the query (where the id is included as "identifier") and a JSON file containing the frame are required. Both files should be named with the same name, which is the attribute that will be provided during the call of the API.
  - If you would like to request data on behalf of another user, you can provide the *access subject* of this user, i.e. their EORI. If it is not provided, the access subject is the ClientId of the current logged in user.
- In the second endpoint **/diac/RunSparqlQuery/{Profile}** you can provide a *profile*, *query* and *accept* header. 
  - In case you use the API outside swagger, provide the profile in the path and add the query as parameter and the accept header. Profile is a subset of data you have access to and query is any query you wish to execute on this data.
  - Depending on the query form (ask, select, construct, describe), a valid media type should be provided as accept header. After execution, the respective Linked Data will be received as response in the format requested by the accept header.
  - Triply returns only the first page back when executing a query, meaning that you can get up to 10,000 results. In case you want to receive all the data from Triply at once, you can set the *pagination* parameter to true.
  - If you would like to request data on behalf of another user, you can provide the *access subject* of this user, i.e. their EORI. If it is not provided, the access subject is the ClientId of the current logged in user.
> Note: You can find all the valid media types and result formats for all query forms on [TriplyDB] .

## Development
Want to apply authorization on Linked Data yourself? Great! This software is open source.
You can download this repo and use it as base to develop your own software.

## License
MIT

[iShare]: <https://ishare.eu/>
[here]: <https://dev.ishare.eu/demo-and-testing/test-certificates.html>
[Poort8]: <https://www.poort8.nl/>
[Triply]: <https://triply.cc/>
[/testing/generate-authorize-request]: <https://scheme.isharetest.net/swagger/index.html#/ServiceConsumer/post_testing_generate_authorize_request>
[Common]: <https://github.com/POORT8/Poort8.Ishare.Common>
[JWT]: <https://jwt.io/>
[iSHARE Test CA]: <https://dev.ishareworks.org/demo-and-testing/test-certificates.html#ishare-test-ca>
[this]: <https://raw.githubusercontent.com/POORT8/Poort8.Ishare.Common/master/ishare-test-ca-chain.txt>
[TriplyDB]: <https://triply.cc/docs/triply-api#sparql-query-result-formats>