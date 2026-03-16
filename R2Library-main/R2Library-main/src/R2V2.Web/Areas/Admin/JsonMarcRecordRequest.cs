#region

using System;
using System.Collections.Generic;

#endregion

namespace R2V2.Web.Areas.Admin
{
    [Serializable]
    public class JsonMarcRecordRequest
    {
        public string Format { get; set; }
        public string AccountNumber { get; set; }
        public List<JsonIsbnAndCustomerFields> IsbnAndCustomerFields { get; set; }
        public bool IsDeleteFile { get; set; }
        public MarcFtpCredientials FtpCredientials { get; set; }


        public List<JsonCustomMarcField> CustomMarcFields { get; set; }

        public bool IsR2Request => true;
    }

    [Serializable]
    public class JsonIsbnAndCustomerFields
    {
        public string IsbnOrSku { get; set; }
        public List<JsonCustomMarcField> CustomMarcFields { get; set; }
    }

    [Serializable]
    public class JsonCustomMarcField
    {
        public int FieldNumber { get; set; }
        public string FieldIndicator1 { get; set; }
        public string FieldIndicator2 { get; set; }
        public string FieldValue { get; set; }
        public List<JsonCustomMarcSubfields> MarcSubfields { get; set; }
    }

    [Serializable]
    public class JsonCustomMarcSubfields
    {
        public string Subfield { get; set; }
        public string SubfieldValue { get; set; }
    }

    [Serializable]
    public class MarcFtpCredientials
    {
        public string Host { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}