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

using Newtonsoft.Json;
using Rhetos.Logging;
using Rhetos.Utilities;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Rhetos.AfterDeploy
{
    internal class AfterDeployScriptsProvider
    {
        const string AfterDeployScriptsFileName = "AfterDeployScripts.json";

        private readonly ILogger _performanceLogger;
        private readonly RhetosAppEnvironment _rhetosAppEnvironment;

        public AfterDeployScriptsProvider(ILogProvider logProvider, RhetosAppEnvironment rhetosAppEnvironment)
        {
            _performanceLogger = logProvider.GetLogger("Performance");
            _rhetosAppEnvironment = rhetosAppEnvironment;
        }

        private string AfterDeployScriptsFilePath => Path.Combine(_rhetosAppEnvironment.AssetsFolder, AfterDeployScriptsFileName);

        /// <summary>
        /// The scripts are sorted by the intended execution order,
        /// respecting package dependencies and natural sort order by path.
        /// </summary>
        public AfterDeployScripts Load()
        {
            var stopwatch = Stopwatch.StartNew();
            if (!File.Exists(AfterDeployScriptsFilePath))
                throw new FrameworkException($@"The file {AfterDeployScriptsFilePath} that is used to execute the after deploy scripts is missing. Please check that the build has completed successfully before updating the database.");
            var serializedConcepts = File.ReadAllText(AfterDeployScriptsFilePath, Encoding.UTF8);
            var afterDeployScripts = JsonConvert.DeserializeObject<AfterDeployScripts>(serializedConcepts);
            _performanceLogger.Write(stopwatch, $@"AfterDeployScriptsProvider: Loaded {afterDeployScripts.Scripts.Count} scripts from generated file.");
            return afterDeployScripts;
        }

        public void Save(AfterDeployScripts afterDeployScripts)
        {
            var stopwatch = Stopwatch.StartNew();
            string serializedAfterDeployScripts = JsonConvert.SerializeObject(afterDeployScripts, Formatting.Indented);
            File.WriteAllText(AfterDeployScriptsFilePath, serializedAfterDeployScripts, Encoding.UTF8);
            _performanceLogger.Write(stopwatch, $@"AfterDeployScriptsProvider: Saved {afterDeployScripts.Scripts.Count} scripts to generated file.");
        }
    }
}
