# GenericDeploymentsScripter

This package allows to generate a deployment script using DacFx. To get only the changes from the current user it uses the the table DDLChanges.

## How it works

Install the package on your computer with the command:

```
dotnet tool install -g GenericDeploymentScripter --version 1.0.0
```

The package is for the moment only on GitHub. So you have to add GitHub as a nuget source:

```
dotnet tool install -g GenericDeploymentScripter --version 1.0.0
```

dotnet nuget add source "https://nuget.pkg.github.com/M-Patrone/index.json" --name "GitHub" --username YOUR_USERNAME --password GITHUB_PAT

```
Here a list of the supported arguments:
```

--sourceconnectionstring Required. Argument to hold the connection string
from the dev DB

--targetconnectionstring Required. Argument to hold the connection string
from the target DB

--outputpath Argument to hold the output path with the filename

--username Argument to hold the username (with domain)

--help Display this help screen.

```

```
