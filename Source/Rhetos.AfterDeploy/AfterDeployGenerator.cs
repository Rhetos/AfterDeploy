/*
    Copyright (C) 2014 Omega software d.o.o.

    This file is part of Rhetos.

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as
    published by the Free Software Foundation, either version 3 of the
    License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using Rhetos.Deployment;
using Rhetos.Extensibility;
using Rhetos.Logging;
using Rhetos.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;

namespace Rhetos.AfterDeploy
{
    [Export(typeof(IGenerator))]
    public class AfterDeployGenerator : IGenerator
    {
        private readonly InstalledPackages _installedPackages;
        private readonly AfterDeployScriptsProvider _afterDeployScriptsProvider;
        private readonly FilesUtility _filesUtility;

        private const string AfterDeploySubfolder = "AfterDeploy";
        private static readonly string AfterDeploySubfolderPrefix = AfterDeploySubfolder + Path.DirectorySeparatorChar;
        private static readonly string AfterDeploySubfolderPattern = Path.DirectorySeparatorChar + AfterDeploySubfolder + Path.DirectorySeparatorChar;

        public AfterDeployGenerator(InstalledPackages installedPackages, ILogProvider logProvider, RhetosBuildEnvironment rhetosBuildEnvironment, FilesUtility filesUtility)
        {
            _installedPackages = installedPackages;
            _afterDeployScriptsProvider = new AfterDeployScriptsProvider(logProvider, rhetosBuildEnvironment);
            _filesUtility = filesUtility;
        }

        public IEnumerable<string> Dependencies { get { return Enumerable.Empty<string>(); } }

        public void Generate()
        {
            // The packages are sorted by their dependencies, so the SQL scripts will be executed in the same order.
            var scripts = _installedPackages.Packages.SelectMany(GetScripts).ToList();
            _afterDeployScriptsProvider.Save(new AfterDeployScripts { Scripts = scripts });
        }

        /// <summary>
        /// Returns after-deploy scripts, ordered by natural sort of file paths inside each package.
        /// </summary>
        private List<AfterDeployScript> GetScripts(InstalledPackage package)
        {
            var packageSqlFiles =
                    (from file in package.ContentFiles
                     let afterDeploySimplifiedPath = GetScriptSimplifiedPath(file)
                     where afterDeploySimplifiedPath != null // Use only files in AfterDeploy subfolder.
                     where CheckFileExtension(file)
                     orderby CsUtility.GetNaturalSortString(file.InPackagePath).Replace(@"\", @" \").Replace(@"/", @" /")
                     select new AfterDeployScript
                     {
                         Name = package.Id + ": " + afterDeploySimplifiedPath,
                         Script = _filesUtility.ReadAllText(file.PhysicalPath)
                     })
                    .ToList();

            return packageSqlFiles;
        }

        /// <summary>
        /// If file is located in an AfterDeploy folder, returns the simplified file path, otherwise returns <see langword="null"/>.
        /// </summary>
        private string GetScriptSimplifiedPath(ContentFile file)
        {
            if (file.InPackagePath.StartsWith(AfterDeploySubfolderPrefix, StringComparison.OrdinalIgnoreCase))
                return file.InPackagePath[AfterDeploySubfolderPrefix.Length..];

            var startPostion = file.InPackagePath.IndexOf(AfterDeploySubfolderPattern, StringComparison.OrdinalIgnoreCase);
            if (startPostion != -1)
                return file.InPackagePath[..startPostion] + file.InPackagePath[(startPostion + AfterDeploySubfolderPattern.Length - 1)..];

            return null;
        }

        /// <summary>
        /// Throws an exception if the AfterDeploy file does not have '.sql' extension.
        /// </summary>
        private bool CheckFileExtension(ContentFile file)
        {
            const string expectedExtension = ".sql";
            if (!string.Equals(Path.GetExtension(file.InPackagePath), expectedExtension, StringComparison.OrdinalIgnoreCase))
                throw new FrameworkException($"After-deploy script '{file.PhysicalPath}' does not have expected extension '{expectedExtension}'.");
            return true;
        }
    }
}
