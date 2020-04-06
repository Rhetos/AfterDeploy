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

        public AfterDeployGenerator(InstalledPackages installedPackages, ILogProvider logProvider, RhetosBuildEnvironment rhetosBuildEnvironment)
        {
            _installedPackages = installedPackages;
            _afterDeployScriptsProvider = new AfterDeployScriptsProvider(logProvider, rhetosBuildEnvironment);
        }

        public IEnumerable<string> Dependencies { get { return Enumerable.Empty<string>(); } }

        public void Generate()
        {
            // The packages are sorted by their dependencies, so the sql scripts will be executed in the same order.
            var scripts = _installedPackages.Packages.SelectMany(GetScripts).ToList();
            _afterDeployScriptsProvider.Save(new AfterDeployScripts { Scripts = scripts });
        }

        /// <summary>
        /// Returns after-deploy scripts, ordered by natural sort of file paths inside each package.
        /// </summary>
        private List<AfterDeployScript> GetScripts(InstalledPackage package)
        {
            const string afterDeployFolderPrefix = @"AfterDeploy\";

            var files = package.ContentFiles.Where(file => file.InPackagePath.StartsWith(afterDeployFolderPrefix, StringComparison.OrdinalIgnoreCase))
                .OrderBy(file => CsUtility.GetNaturalSortString(file.InPackagePath).Replace(@"\", @" \"))
                .ToList();

            const string expectedExtension = ".sql";
            var badFile = files.FirstOrDefault(file => !string.Equals(Path.GetExtension(file.InPackagePath), expectedExtension, StringComparison.OrdinalIgnoreCase));
            if (badFile != null)
                throw new FrameworkException("After-deploy script '" + badFile.PhysicalPath + "' does not have expected extension '" + expectedExtension + "'.");

            return files
                .Select(file => new AfterDeployScript
                {
                    Name = package.Id + ": " + file.InPackagePath.Substring(afterDeployFolderPrefix.Length),
                    Script = File.ReadAllText(file.PhysicalPath, Encoding.UTF8)
                })
                .ToList();
        }
    }
}
