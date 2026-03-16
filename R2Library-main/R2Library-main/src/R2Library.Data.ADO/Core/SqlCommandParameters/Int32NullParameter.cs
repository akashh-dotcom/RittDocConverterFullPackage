#region

using System;
using System.Data.SqlClient;

#endregion

namespace R2Library.Data.ADO.Core.SqlCommandParameters
{
    public class Int32NullParameter : ISqlCommandParameter
    {
        public Int32NullParameter()
        {
        }

        public Int32NullParameter(string name, int? value)
        {
            Name = name;
            Value = value;
        }

        public int? Value { get; set; }
        public string Name { get; set; }

        public void SetCommandParmater(SqlCommand command)
        {
            DbCommandHelper.SetCommandParmater(command, Name, Value);
        }

        public override string ToString()
        {
            return
                Value == null
                    ? $"[Name = {Name}, Value = {DBNull.Value}]"
                    : $"[Name = {Name}, Value = {Value}]";
        }
    }
}