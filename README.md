# Bug Bounty Self-Service Portal

This is a solution to help Bug Bounty team in providing credentials to the researchers to test new services added to the Bug Bounty program.

As an administrator, you can see the number of unclaimed credentials for each service, and receive a warning when one is dangerously low.

You can import a new set of credentials from a CSV file, add new service, view credentials for a service, view credentials assigned to a researcher.

A credential set can have 1 or more rows, for example a user and password for a role and another user/password for another role.

When number of available credentials are below a threshold it will email admins to add more.


As a researcher, you can see previous credentials assigned to you and also require new set of credentials.


Solution consists of 2 projects:
- VismaBugBountySelfServicePortal: .net Core MVC application using EF Core for database
- VismaBugBountySelfServicePortal.Infrastructure: ARM template to deploy infrastructure on Microsoft Azure

# VismaBugBountySelfServicePortal
A .net Core MVC application
It uses EF Core code first for database.

Secrets are stored on KeyVault.
For local development they are stored in secrets.json. 

List of secrets:
```sh
{
  "EmailConfiguration": {
    "Password": ""
  },
  "ClientSecret": "",
  "DatabasePassword": "",
  "AdminDatabasePassword": "",
  "PrivateProgramPassword": "",
  "PublicProgramPassword": "",
  "ApiKey": ""
}
```
