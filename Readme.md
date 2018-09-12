# AfterDeploy

Rhetos.AfterDeploy is a plugin package for [Rhetos development platform](https://github.com/Rhetos/Rhetos).
It simplifies the handling of SQL scripts that need to be executed on each deployment.

## Features

* This plugin will activate inside `DeployPackages.exe`, after database is upgraded.
* It will execute all SQL scripts placed in the `AfterDeploy` subfolder of each deployed package.
  * Note that these SQL scripts may directly use all tables, views and other database objects.
    There no need for special formatting or limitations that are present in the [DataMigration](https://github.com/Rhetos/Rhetos/wiki/Data-migration) scripts.
* Order of execution:
  * The SQL scripts from one package will be executed in alphabetical order, using natural sort for numbers in folder and file names.
  * The ordering of SQL scripts will respect dependencies between packages, defined in their .nuspec files.

### AfterDeploy vs DataMigration scripts

**AfterDeploy** scripts are useful for data that is not an integral part of the business application (is not referenced by other entities) and should be refreshed on each deployment.

* **Roles and permissions** are a good example. There can be one script that contains predefined roles or permissions. On each deployment the script should compare the existing data to the expected, and update the database. See the chapter "[Inserting the permissions on deployment](https://github.com/Rhetos/Rhetos/wiki/Basic-permissions#inserting-the-permissions-on-deployment)".
* AfterDeploy scripts are executed at the end of the deployment process, **after** the database structure is upgraded.

**DataMigration** scripts are useful for data that needs to be changed when the business objects are changed in new versions of the business applications.

* **Hard-coded data** tables should be initialized in data-migration scripts. These records could be modified with new versions of the applications. More importantly, these records are usually referenced from other tables, so the data-migrations scripts for the other tables might need the hard-coded data already inserted into the database.
* DataMigration scripts are executed at the beginning of the deployment process, **before** the database structure is upgraded.

## Build

**Note:** This package is already available at the [NuGet.org](https://www.nuget.org/) online gallery.
You don't need to build it from source in order to use it in your application.

To build the package from source, run `Build.bat`.
The script will pause in case of an error.
The build output is a NuGet package in the "Install" subfolder.

## Installation

To install this package to a Rhetos server, add it to the Rhetos server's *RhetosPackages.config* file
and make sure the NuGet package location is listed in the *RhetosPackageSources.config* file.

* The package ID is "**Rhetos.AfterDeploy**".
  This package is available at the [NuGet.org](https://www.nuget.org/) online gallery.
  It can be downloaded or installed directly from there.
* For more information, see [Installing plugin packages](https://github.com/Rhetos/Rhetos/wiki/Installing-plugin-packages).
