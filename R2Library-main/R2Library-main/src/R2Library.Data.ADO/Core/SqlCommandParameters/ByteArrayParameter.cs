#region

using System.Data.SqlClient;

#endregion

namespace R2Library.Data.ADO.Core.SqlCommandParameters
{
    public class ByteArrayParameter : ISqlCommandParameter
    {
        public ByteArrayParameter()
        {
        }

        public ByteArrayParameter(string name, byte[] value)
        {
            Name = name;
            Value = value;
        }

        public byte[] Value { get; set; }
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