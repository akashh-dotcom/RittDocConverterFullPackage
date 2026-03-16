#region

using System.Data.SqlClient;

#endregion

namespace R2Library.Data.ADO.Core.SqlCommandParameters
{
    public class StringParameter : ISqlCommandParameter
    {
        public StringParameter()
        {
        }

        public StringParameter(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public string Value { get; set; }
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