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

using Rhetos.Extensibility;
using Rhetos.Logging;
using Rhetos.Utilities;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

namespace Rhetos.AfterDeploy
{
    [Export(typeof(IServerInitializer))]
    public class AfterDeployExecuter : IServerInitializer
    {
        private readonly SqlTransactionBatches _sqlTransactionBatches;
        private readonly ILogger _logger;
        private readonly AfterDeployScriptsProvider _afterDeployScriptsProvider;

        public AfterDeployExecuter(SqlTransactionBatches sqlTransactionBatches, ILogProvider logProvider, IAssetsOptions assetsOptions)
        {
            _sqlTransactionBatches = sqlTransactionBatches;
            _logger = logProvider.GetLogger("AfterDeploy");
            _afterDeployScriptsProvider = new AfterDeployScriptsProvider(logProvider, assetsOptions);
        }

        public IEnumerable<string> Dependencies
        {
            get
            {
                return new[]
                {
                    // These dependencies allow CommonConcepts and AspNetFormsAuth plugins to generate standard security claims and roles,
                    // before executing AfterDeploy script, so that the scripts could be used for generating built-in permission.
                    "Rhetos.Dom.DefaultConcepts.ClaimGenerator",
                    "Rhetos.AspNetFormsAuth.AuthenticationDatabaseInitializer"
                };
            }
        }

        public void Initialize()
        {
            // The packages are sorted by their dependencies, so the SQL scripts will be executed in the same order.
            var scripts = _afterDeployScriptsProvider.Load().Scripts
                .Select(x => new SqlBatchScript { Name = x.Name, Sql = x.Script, IsBatch = true})
                .ToList();
            foreach (var script in scripts)
                _logger.Trace(() => $"Script {script.Name}");

            _sqlTransactionBatches.Execute(scripts);
            _logger.Info($"Executed {scripts.Count} after-deploy scripts.");
        }
    }
}
