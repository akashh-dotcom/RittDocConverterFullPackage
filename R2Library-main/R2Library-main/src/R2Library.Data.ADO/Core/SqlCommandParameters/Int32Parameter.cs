#region

using System.Data.SqlClient;

#endregion

namespace R2Library.Data.ADO.Core.SqlCommandParameters
{
    public class Int32Parameter : ISqlCommandParameter
    {
        public Int32Parameter()
        {
        }

        public Int32Parameter(string name, int value)
        {
            Name = name;
            Value = value;
        }

        public int Value { get; set; }
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