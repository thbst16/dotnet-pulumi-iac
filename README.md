# dotnet-pulumi-iac

The dotnet-pulumi-iac solution presents a comprehensive solution for complete infrastructure as code (IaC) provisioning of a non-trivial Azure application environment. The solution provisions all of the Azure infrastructure necessary to host all of the demo applications supporting my GitHub public projects. The project uses [Pulumi](https://www.pulumi.com/) as the Infrastructure as Code (IaC) framework with C# as the IaC language.

# Provisioning Architecture

Pulumi documentation and sample projects provide excellent examples of provisioning simple Azure AppService and Web App environments. These examples provide all the requisite details on provisioning the Azure Resource Groups, App Service and web apps necessary to get an Azure web-based app up and running.

However, there isn't an example of a non-trivial use case -- one that incorporates the reality of dynamic configuration and utilizes the power of Docker-based deployments that have been avaialble with Azure App Service for several years.

This solution addresses the following specific provisioning facets:

* Docker-based deployments to Azure App Services using App Service Linux-based hosting.
* Specifically, Docker container deployments using Docker Compose, enabling multi-container deployments and the specification of mount points in the compose file.
* Externalizing secrets from the Docker image and specifying these at run time by dynamically mouting configuration files stored in Azure Blob storage.
* Applying dynamic configuration changes at the time of IaC execution to inejct custom cloud resource settings into the application configuration that could not be known until the cloud resource is provisioned (e.g. the Azure Blob secret key)

The figure below reflects the specific provsioning architecture applied through this project. The Azure Resource Group and all of the cloud objects within the Resource Group are all provsioned dynamically via Pulumi and are all defined explicitly in the MyStack.cs file in this project.


### Provisioning Architecture
![Dotnet Pulumi Provisioning Architecture](https://s3.amazonaws.com/s3.beckshome.com/20220620-dotnet-pulumi-iac-provision.png)

# Deployment Architecture

Picture + explanation

### Deployment Architecture
![Dotnet Pulumi Deployment Architecture](https://s3.amazonaws.com/s3.beckshome.com/20220620-dotnet-pulumi-iac-deploy.png)

# Motivation and Credits 

Bulleted list of blogs