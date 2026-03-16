#region

using System;
using System.Data.SqlClient;

#endregion

namespace R2Library.Data.ADO.Core.SqlCommandParameters
{
    public class DateTimeParameter : ISqlCommandParameter
    {
        public DateTimeParameter()
        {
        }

        public DateTimeParameter(string name, DateTime value)
        {
            Name = name;
            Value = value;
        }

        public DateTime Value { get; set; }
        public string Name { get; set; }

        public void SetCommandParmater(SqlCommand command)
        {
            DbCommandHelper.SetCommandParmater(command, Name, Value);
        }

        public override string ToString()
        {
            return $"[Name = {Name}, Value = {Value}]";
        }
    }
}