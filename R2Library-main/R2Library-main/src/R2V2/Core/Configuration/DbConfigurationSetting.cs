#region

using System.Text;
using R2V2.Core.SuperType;

#endregion

namespace R2V2.Core.Configuration
{
    public class DbConfigurationSetting : IDebugInfo, IEntity
    {
        public virtual string Configuration { get; set; }
        public virtual string Setting { get; set; }
        public virtual string Key { get; set; }
        public virtual string Value { get; set; }
        public virtual string Description { get; set; }

        public virtual string ToDebugString()
        {
            return new StringBuilder("DbConfigurationSetting = [")
                .AppendFormat("Id: {0}", Id)
                .AppendFormat(", Configuration: {0}", Configuration)
                .AppendFormat(", Setting: {0}", Setting)
                .AppendFormat(", Key: {0}", Key)
                .AppendFormat(", Value: {0}", Value)
                .AppendFormat(", Instructions: {0}", Description)
                .Append("]").ToString();
        }

        public virtual int Id { get; set; }

        public virtual string ToInsertString()
        {
            var sb = new StringBuilder()
                    .Append(
                        "INSERT INTO tConfigurationSetting (vchConfiguration, vchSetting, vchKey, vchValue, vchInstructions)")
                    .Append(
                        $"VALUES ('{Configuration?.Replace("'", "''")}', '{Setting?.Replace("'", "''")}', '{Key?.Replace("'", "''")}', '{Value?.Replace("'", "''")}', '{Description?.Replace("'", "''")}');")
                ;
            return sb.ToString();
        }
    }
}