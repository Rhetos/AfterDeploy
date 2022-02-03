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

using Rhetos.Utilities;
using System;
using Xunit;

namespace Rhetos.AfterDeploy.Test
{
    public class TransactionControlTest
    {
        [Fact]
        public void StandardScriptInTransaction()
        {
            using var scope = TestScope.Create();
            var sqlExecuter = scope.Resolve<ISqlExecuter>();

            int? trancount = null;
            int? x = null;
            sqlExecuter.ExecuteReader(
                "SELECT TRANCOUNT, X FROM dbo.A",
                reader => (trancount, x) = (reader.GetInt32(0), reader.GetInt32(1)));

            Assert.Equal("1, 123", $"{trancount}, {x}");
        }

        [Fact]
        public void NoTransactionScript()
        {
            using var scope = TestScope.Create();
            var sqlExecuter = scope.Resolve<ISqlExecuter>();

            int? trancount = null;
            int? y = null;
            sqlExecuter.ExecuteReader(
                "SELECT TRANCOUNT, Y FROM dbo.B",
                reader => (trancount, y) = (reader.GetInt32(0), reader.GetInt32(1)));

            Assert.Equal("0, 124", $"{trancount}, {y}");
        }
    }
}
