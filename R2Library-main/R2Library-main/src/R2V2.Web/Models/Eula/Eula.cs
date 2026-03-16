namespace R2V2.Web.Models.Eula
{
    //[PageDefinition]
    public class Eula : BaseModel
    {
        //[EditableFileUpload(Title = "EULA Download")]
        public virtual string EulaDownload { get; set; }

        // [EditableFreeTextArea(Title = "EULA Text")]
        public virtual string EulaText { get; set; }
    }
}