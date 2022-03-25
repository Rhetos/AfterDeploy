# AfterDeploy release notes

## 5.0.0 (2022-03-25)

* Migrated from .NET Framework to .NET 5 and Rhetos 5.

## 1.2.0 (2019-09-09)

### Internal improvements

* Changed default file encoding from ANSI to UTF-8.
* Bugfix: AfterDeploy scripts are ignored when deploying a package directly from source folder
  ([Rhetos #173](https://github.com/Rhetos/Rhetos/issues/173)).

## 1.1.0 (2017-02-13)

## New features

* Supports the `GO` command in SQL scripts, and `/*DatabaseGenerator:NoTransaction*/` tag.

## 1.0.0 (2016-10-19)

Features:

* Executing all SQL scripts placed in the `AfterDeploy` subfolder of each deployed package.

See [Readme.md](Readme.md) for more info.
