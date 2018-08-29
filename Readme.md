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
