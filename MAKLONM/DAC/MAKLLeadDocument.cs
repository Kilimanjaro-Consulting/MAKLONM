using System;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.CR;
using static PX.SM.AUStepField;

namespace MAKLONM
{
  [Serializable]
  [PXCacheName("MAKLLeadDocument")]
  public class MAKLLeadDocument : IBqlTable
  {
    #region RecordID
    [PXDBIdentity(IsKey = true)]
    [PXUIField(DisplayName = "Record ID", Enabled = false)]
    public virtual int? RecordID { get; set; }
    public abstract class recordID : PX.Data.BQL.BqlInt.Field<recordID> { }
    #endregion

    #region ContactID
    [PXDBInt()]
    [CRLeadSelector(typeof(
                SelectFrom<CRLead>
                .LeftJoin<BAccount>
                    .On<BAccount.bAccountID.IsEqual<CRLead.bAccountID>>
                .Where<
                    BAccount.bAccountID.IsNull
                    .Or<Match<BAccount, Current<AccessInfo.userName>>>
                >
                .SearchFor<CRLead.contactID>),
            fieldList: new[]
            {
                typeof(CRLead.memberName),
                typeof(CRLead.fullName),
                typeof(CRLead.salutation),
                typeof(CRLead.eMail),
                typeof(CRLead.phone1),
                typeof(CRLead.status),
                typeof(CRLead.duplicateStatus)
            },
            Headers = new[]
            {
                "Contact",
                "Account Name",
                "Job Title",
                "Email",
                "Phone 1",
                "Status",
                "Duplicate"
            },
            DescriptionField = typeof(CRLead.memberName),
            Filterable = true)]
    [PXUIField(DisplayName = "Lead")]
    [PXParent(typeof(Select<CRLead, Where<CRLead.contactID, Equal<Current<contactID>>>>))]  
    [PXDBDefault(typeof(CRLead.contactID))]
    public virtual int? ContactID { get; set; }
    public abstract class contactID : PX.Data.BQL.BqlInt.Field<contactID> { }
    #endregion

    #region Description
    [PXDBString(255, IsUnicode = true, InputMask = "")]
    [PXUIField(DisplayName = "Description")]
    public virtual string Description { get; set; }
    public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
    #endregion

    #region Web
   // [PXDBString(255, IsUnicode = true, InputMask = "")]
    [PXUIField(DisplayName = "Web")]
    [PXDBWeblink()]
    public virtual string Web { get; set; }
    public abstract class web : PX.Data.BQL.BqlString.Field<web> { }
    #endregion

    #region DueDate
    [PXDBDate()]
    [PXUIField(DisplayName = "Due Date")]
    public virtual DateTime? DueDate { get; set; }
    public abstract class dueDate : PX.Data.BQL.BqlDateTime.Field<dueDate> { }
    #endregion

    #region Tstamp
    [PXDBTimestamp()]
    [PXUIField(DisplayName = "Tstamp")]
    public virtual byte[] Tstamp { get; set; }
    public abstract class tstamp : PX.Data.BQL.BqlByteArray.Field<tstamp> { }
    #endregion

    #region Noteid
    [PXNote()]
    public virtual Guid? Noteid { get; set; }
    public abstract class noteid : PX.Data.BQL.BqlGuid.Field<noteid> { }
    #endregion

    #region CreatedByID
    [PXDBCreatedByID()]
    public virtual Guid? CreatedByID { get; set; }
    public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
    #endregion

    #region CreatedByScreenID
    [PXDBCreatedByScreenID()]
    public virtual string CreatedByScreenID { get; set; }
    public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
    #endregion

    #region CreatedDateTime
    [PXDBCreatedDateTime()]
    public virtual DateTime? CreatedDateTime { get; set; }
    public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
    #endregion

    #region LastModifiedByID
    [PXDBLastModifiedByID()]
    public virtual Guid? LastModifiedByID { get; set; }
    public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
    #endregion

    #region LastModifiedByScreenID
    [PXDBLastModifiedByScreenID()]
    public virtual string LastModifiedByScreenID { get; set; }
    public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
    #endregion

    #region LastModifiedDateTime
    [PXDBLastModifiedDateTime()]
    public virtual DateTime? LastModifiedDateTime { get; set; }
    public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
    #endregion
  }
}