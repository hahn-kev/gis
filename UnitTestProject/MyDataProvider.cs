using System.Data;
using LinqToDB.Common;
using LinqToDB.DataProvider.SQLite;

namespace UnitTestProject
{
    public class MyDataProvider : SQLiteDataProvider
    {
        public override void SetParameter(IDbDataParameter parameter, string name, DbDataType dataType, object value)
        {
            if (value is string[] arr) value = string.Join(",", arr);
            base.SetParameter(parameter, name, dataType, value);
        }
    }
}