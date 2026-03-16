#region

using System.Data.SqlClient;

#endregion

namespace R2Library.Data.ADO.Core.SqlCommandParameters
{
    public class BooleanParameter : ISqlCommandParameter
    {
        public BooleanParameter()
        {
        }

        public BooleanParameter(string name, bool value)
        {
            Name = name;
            Value = value;
        }

        public bool Value { get; set; }
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