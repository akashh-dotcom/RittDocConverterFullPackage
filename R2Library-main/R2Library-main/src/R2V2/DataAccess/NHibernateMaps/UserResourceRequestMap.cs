#region

using R2V2.Core.Authentication;

#endregion

namespace R2V2.DataAccess.NHibernateMaps
{
    public class UserResourceRequestMap : BaseMap<UserResourceRequest>
    {
        ////[iUserResourceRequestId] [int] IDENTITY(1,1) NOT NULL,
        ////[iUserId] [int] NULL,
        ////[iInstitutionId] [int] NOT NULL,
        ////[vchFirstName] [varchar] (50) NULL,
        ////[vchLastName] [varchar] (100) NULL,
        ////[vchTitle] [varchar] (250) NULL,
        ////[iResourceId] [int] NOT NULL,
        ////[vchCreatorId] [varchar] (50) NOT NULL,
        ////[dtCreationDate] [smalldatetime]//NOT NULL,
        ////[vchUpdaterId] [varchar] (50) NULL,
        ////[dtLastUpdate] [smalldatetime] NULL,
        ////[tiRecordStatus] [tinyint] NOT NULL,
        //public virtual int UserId { get; set; }
        //public virtual int InstitutionId { get; set; }
        //public virtual int ResourceId { get; set; }
        //public virtual string FirstName { get; set; }
        //public virtual string LastName { get; set; }
        //public virtual string Title { get; set; }
        //public virtual bool RecordStatus { get; set; }
        public UserResourceRequestMap()
        {
            Table("dbo.tUserResourceRequest");
            Id(x => x.Id, "iUserResourceRequestId").GeneratedBy.Identity();
            Map(x => x.UserId, "iUserId");
            Map(x => x.InstitutionId, "iInstitutionId");
            Map(x => x.Name, "vchName");
            Map(x => x.Title, "vchTitle");
            Map(x => x.Comment, "vchComment");
            Map(x => x.ResourceId, "iResourceId");
        }
    }
}