#region

using System.Data.SqlClient;

#endregion

namespace R2Library.Data.ADO.Core.SqlCommandParameters
{
    public class Int16Parameter : ISqlCommandParameter
    {
        public Int16Parameter()
        {
        }

        public Int16Parameter(string name, short value)
        {
            Name = name;
            Value = value;
        }

        public short Value { get; set; }
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