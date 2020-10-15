# serverless-certs-and-secrets


## Requirements

1. What certificate a function app used to connect to other services?
    - Demonstrate and document how to upload the public certificate to the code and test the access to some azure service

2. What certificate a function app should use to provide an endpoint ?
    - Demonstrate and document the best way to add certificate to a function app as a endpoint. Available options are:
        - Create a free App Service Managed Certificate (Preview)
        - Purchase an App Service certificate
        - Import a certificate from Key Vault
        - Upload a private certificate


## Code Samples

In FunctionCertificate folder you will find some useful code samples. Each folder inside the root folder is a app that will provide a good example. You

- CreateSelfSignedCertificateConsole
    This code is useful to generate a self signed certificate so it can be used in your functionApp.

- FunctionCertificate
    The Function itself.

- FunctionCertificateConsoleClient



## Todo List

- Finish the sample code using some service connection (like storage account);
- Document ways to add certificate to a function app:
    - Create a free App Service Managed Certificate (Preview)
    - Purchase an App Service certificate
    - Import a certificate from Key Vault
    - Upload a private certificate


## Progress Work

20%
