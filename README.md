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

As explained in the provisioning section, the cloud infrastructure for the entire suite of applications I host can be deployed via a single command (pulumi up) and removed via another command (pulumi down). Since the provisioning includes the deployment from Docker Hub, the result is a set of running web applications. Re-executing the "pulumi up" command will result in any cloud infrastructure changes in MyStack.cs being applied but will not materially change the application.

The deployment architecture below reflects how the Azure DevOps CI/CD pipeline interacts with the Pulumi-deployed cloud architecture to effectuate changes to the application. As shown in the picture, this involves an application build and dockerization (steps 2 and 3). The interaction with the cloud architecture occurs in steps 4 and 5 where the CI/CD pipeline pushes the new Docker image to Docker Hub (step 4) and then restarts the web application created by the Pulumi script (step 5). 

Pulumi scripts can be integrated into Azure DevOps pipelines, effectively rebuilding the entire cloud architecture with each build and deployment cycle. This is not done in this case since this project provisions the cloud architecture for a suite of Azure web applications, not a single application.

### Deployment Architecture
![Dotnet Pulumi Deployment Architecture](https://s3.amazonaws.com/s3.beckshome.com/20220620-dotnet-pulumi-iac-deploy.png)

# Motivation and Credits 

There is no single site that had all the answers I was looking for but there were pieces of the puzzle that came together for this solution. These pieces started with Pulumi's sample code to deploy an Azure App Service runing containers on Linux. Two second two articles provided excellent and more detailed examples illustrating the nuances of dynamically configurable, Pulumi-based deployments. the final link is to the Pulumi Azure task extension for Azure Pipelines, in case you want to include Pulumi deployments as part of your pipeline execution.

* [Azure App Service Running Docker Containers on Linux](https://www.pulumi.com/registry/packages/azure-native/how-to-guides/azure-cs-appservice-docker/)
* [Infrastructure as Code - An Intro - Part 6 - Using Pulumi](https://www.fearofoblivion.com/iac-an-intro-part-6)
* [Deploying .NET to Azure App Service with Pulumi](https://martink.me/articles/deploying-.net-to-azure-app-service-with-pulumi)
* [Pulumi Azure Task Extenison for Azure Pipelines](https://marketplace.visualstudio.com/items?itemName=pulumi.build-and-release-task)