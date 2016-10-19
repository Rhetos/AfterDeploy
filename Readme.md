Rhetos.AfterDeploy is a DSL package for [Rhetos development platform](https://github.com/Rhetos/Rhetos).
It simplifies the handling of SQL scripts that need to be executed on each deployment.

Features:
* This plugin will activate inside `DeployPackages.exe`, after database is upgraded.
* It will execute all SQL scripts placed in the `AfterDeploy` subfolder of each deployed package.
* The ordering of SQL scripts will respect dependencies between packages, defined in their .nuspec files.
* The SQL scripts from one package will be executed in alphabetical order, using natural sort for numbers in folder and file names.
