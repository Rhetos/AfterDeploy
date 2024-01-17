﻿/*
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

using Autofac;
using Rhetos.Logging;
using Rhetos.Security;
using Rhetos.Utilities;
using System;
using System.IO;

namespace Rhetos.AfterDeploy.Test
{
    /// <summary>
    /// Helper class that manages Dependency Injection container for unit tests.
    /// The container can be customized for each unit test scope.
    /// </summary>
    public static class TestScope
    {
        /// <summary>
        /// Creates a thread-safe lifetime scope DI container (service provider)
        /// to isolate unit of work with a <b>separate database transaction</b>.
        /// To commit changes to database, call <see cref="IUnitOfWork.CommitAndClose"/> at the end of the 'using' block.
        /// </summary>
        /// <remarks>
        /// Use helper methods in <see cref="TestScopeContainerBuilderExtensions"/> to configuring components
        /// from the <paramref name="registerCustomComponents"/> delegate.
        /// </remarks>
        public static IUnitOfWorkScope Create(Action<ContainerBuilder> registerCustomComponents = null)
        {
            ConsoleLogger.MinLevel = EventType.Info; // Use EventType.Trace for more detailed log.
            return _rhetosHost.CreateScope(registerCustomComponents);
        }

        /// <summary>
        /// Reusing a single shared static DI container between tests, to reduce initialization time for each test.
        /// Each test should create a child scope with <see cref="Create"/> method to start a 'using' block.
        /// </summary>
        private static readonly RhetosHost _rhetosHost = RhetosHost.CreateFrom(Path.GetFullPath(@"..\..\..\..\TestApp\bin\Debug\net8.0\TestApp.dll"), ConfigureRhetosHostBuilder);

        private static void ConfigureRhetosHostBuilder(IRhetosHostBuilder rhetosHostBuilder)
        {
            rhetosHostBuilder.ConfigureContainer(builder =>
            {
                // Configuring standard Rhetos system services to work with unit tests:
                builder.RegisterType<ProcessUserInfo>().As<IUserInfo>();
                builder.RegisterType<ConsoleLogProvider>().As<ILogProvider>();
            });
        }
    }
}
