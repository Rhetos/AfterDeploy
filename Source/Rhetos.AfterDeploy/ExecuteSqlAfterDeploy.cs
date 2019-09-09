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
using System.Threading.Tasks;

namespace Rhetos.AfterDeploy
{
    [Export(typeof(IServerInitializer))]
    public class ExecuteSqlAfterDeploy : IServerInitializer
    {
        private readonly IInstalledPackages _installedPackages;
        private readonly SqlTransactionBatches _sqlTransactionBatches;
        private readonly ILogger _logger;
        private readonly ILogger _deployPackagesLogger;

        public ExecuteSqlAfterDeploy(IInstalledPackages installedPackages, SqlTransactionBatches sqlTransactionBatches, ILogProvider logProvider)
        {
            _installedPackages = installedPackages;
            _sqlTransactionBatches = sqlTransactionBatches;
            _logger = logProvider.GetLogger("AfterDeploy");
            _deployPackagesLogger = logProvider.GetLogger("DeployPackages");
        }

        public IEnumerable<string> Dependencies
        {
            get
            {
                return new[]
                {
                    "Rhetos.Dom.DefaultConcepts.ClaimGenerator",
                    "Rhetos.AspNetFormsAuth.AuthenticationDatabaseInitializer"
                };
            }
        }

        public void Initialize()
        {
            // The packages are sorted by their dependencies, so the sql scripts will be executed in the same order.
            var scripts = _installedPackages.Packages.SelectMany(GetScripts).ToList();
            foreach (var script in scripts)
                _logger.Trace(() => "Script " + script.Name);

            _sqlTransactionBatches.Execute(scripts);
            _deployPackagesLogger.Trace($"Executed {scripts.Count} after-deploy scripts.");
        }

        /// <summary>
        /// Returns after-deploy scripts, ordered by natural sort of file paths inside each package.
        /// </summary>
        private List<SqlTransactionBatches.SqlScript> GetScripts(InstalledPackage package)
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
                .Select(file => new SqlTransactionBatches.SqlScript
                {
                    Name = package.Id + ": " + file.InPackagePath.Substring(afterDeployFolderPrefix.Length),
                    Sql = File.ReadAllText(file.PhysicalPath, Encoding.UTF8),
                    IsBatch = true,
                })
                .ToList();
        }
    }
}
