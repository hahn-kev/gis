using System.Data;
using LinqToDB;
using LinqToDB.DataProvider.SQLite;

namespace UnitTestProject
{
    public class MyDataProvider : SQLiteDataProvider
    {
        public override void SetParameter(IDbDataParameter parameter, string name, DataType dataType, object value)
        {
            if (value is string[] arr) value = string.Join(",", arr);
            base.SetParameter(parameter, name, dataType, value);
        }
    }
}