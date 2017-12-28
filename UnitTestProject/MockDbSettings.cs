using System.Collections.Generic;
using Backend;
using LinqToDB;
using LinqToDB.Configuration;

namespace UnitTestProject
{
    public class MockDbSettings : ILinqToDBSettings
    {
        public IEnumerable<IDataProviderSettings> DataProviders
        {
            get { yield break; }
        }

        public string DefaultConfiguration => "SqlLite";

        public string DefaultDataProvider => ProviderName.SQLiteMS;

        public IEnumerable<IConnectionStringSettings> ConnectionStrings
        {
            get
            {
                yield return new ConnectionStringSettings()
                {
                    Name = DefaultConfiguration,
                    ProviderName = DefaultDataProvider,
                    ConnectionString = "DataSource=:memory:"
                };
            }
        }
    }
}