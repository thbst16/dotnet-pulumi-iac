# dotnet-pulumi-iac

The dotnet-pulumi-iac solution presents a comprehensive solution for complete infrastructure as code (IaC) provisioning of a non-trivial Azure application environment. The solution provisions all of the Azure infrastructure necessary to host all of the demo applications supporting my GitHub public projects. The project uses [Pulumi](https://www.pulumi.com/) as the Infrastructure as Code (IaC) framework with C# as the IaC language.

# Provisioning Architecture

Pulumi documentation and sample projects provide excellent examples of provisioning simple Azure AppService and Web App environments. These examples provide all the requisite details on provisioning the Azure Resource Groups, App Service and web apps necessary to get an Azure web-based app up and running.

However, there isn't an example of a non-trivial use case -- one that incorporates the reality of dynamic configuration and utilizes the power of Docker-based deployments that have been avaialble with Azure App Service for several years.

This solution addresses the following specific provisioning facets:

* 


### Provisioning Architecture
![Dotnet Pulumi Provisioning Architecture](https://s3.amazonaws.com/s3.beckshome.com/20220620-dotnet-pulumi-iac-provision.png)

# Deployment Architecture

Picture + explanation

### Deployment Architecture
![Dotnet Pulumi Deployment Architecture](https://s3.amazonaws.com/s3.beckshome.com/20220620-dotnet-pulumi-iac-deploy.png)

# Motivation and Credits 

Bulleted list of blogs