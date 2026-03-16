#region

using System.Data.SqlClient;

#endregion

namespace R2Library.Data.ADO.Core.SqlCommandParameters
{
    public class Int64Parameter : ISqlCommandParameter
    {
        public Int64Parameter()
        {
        }

        public Int64Parameter(string name, long value)
        {
            Name = name;
            Value = value;
        }

        public long Value { get; set; }
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