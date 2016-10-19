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
        private readonly ISqlExecuter _sqlExecuter;
        private readonly ILogger _logger;

        public ExecuteSqlAfterDeploy(IInstalledPackages installedPackages, ISqlExecuter sqlExecuter, ILogProvider logProvider)
        {
            _installedPackages = installedPackages;
            _sqlExecuter = sqlExecuter;
            _logger = logProvider.GetLogger("AfterDeploy");
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
            var scripts = _installedPackages.Packages
                .SelectMany(p => GetScripts(p));

            foreach (var script in scripts)
            {
                _logger.Info("Executing script " + script.Package.Id + ": " + script.Name);
                string sql = File.ReadAllText(script.Path, Encoding.Default);
                _sqlExecuter.ExecuteSql(sql);
            }
        }

        class Script
        {
            public InstalledPackage Package;
            public string Path;
            public string Name;
        }

        /// <summary>Returns after-deploy scripts, ordered by natural sort of file paths inside each package.</summary>
        private List<Script> GetScripts(InstalledPackage package)
        {
            string afterDeployFolder = Path.GetFullPath(Path.Combine(package.Folder, "AfterDeploy"));
            if (!Directory.Exists(afterDeployFolder))
                return new List<Script> { };

            var files = Directory.GetFiles(afterDeployFolder, "*.*", SearchOption.AllDirectories)
                .OrderBy(path => CsUtility.GetNaturalSortString(path).Replace(@"\", @" \"));

            const string expectedExtension = ".sql";
            var badFile = files.FirstOrDefault(file => Path.GetExtension(file).ToLower() != expectedExtension);
            if (badFile != null)
                throw new FrameworkException("After-deploy script '" + badFile + "' does not have the expected extension '" + expectedExtension + "'.");

            return files.Select(path => new Script
                {
                    Package = package,
                    Path = path,
                    Name = GetSimpleName(path, afterDeployFolder)
                })
                .ToList();
        }

        private string GetSimpleName(string path, string folder)
        {
            string name = path.Substring(folder.Length);
            if (name.StartsWith(@"\"))
                name = name.Substring(1);
            return name;
        }
    }
}
