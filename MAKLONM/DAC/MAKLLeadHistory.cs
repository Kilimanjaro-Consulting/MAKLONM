using System;
using PX.Data;

namespace MAKLONM
{
  [Serializable]
  [PXCacheName("MAKLLeadHistory")]
  public class MAKLLeadHistory : IBqlTable
  {
    #region HistoryID
    [PXDBIdentity(IsKey = true)]
    public virtual int? HistoryID { get; set; }
    public abstract class historyID : PX.Data.BQL.BqlInt.Field<historyID> { }
    #endregion

    #region ContactID
    [PXDBInt()]
    [PXUIField(DisplayName = "Contact ID")]
    public virtual int? ContactID { get; set; }
    public abstract class contactID : PX.Data.BQL.BqlInt.Field<contactID> { }
    #endregion

    #region Status
    [PXDBString(1, IsFixed = true, InputMask = "")]
    [PXUIField(DisplayName = "Status")]
    public virtual string Status { get; set; }
    public abstract class status : PX.Data.BQL.BqlString.Field<status> { }
    #endregion

    #region PreviousStatus
    [PXDBString(1, IsFixed = true, InputMask = "")]
    [PXUIField(DisplayName = "Previous Status")]
    public virtual string PreviousStatus { get; set; }
    public abstract class previousStatus : PX.Data.BQL.BqlString.Field<previousStatus> { }
    #endregion

    #region Date
    [PXDBDate()]
    [PXUIField(DisplayName = "Date")]
    public virtual DateTime? Date { get; set; }
    public abstract class date : PX.Data.BQL.BqlDateTime.Field<date> { }
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

    #region Tstamp
    [PXDBTimestamp()]
    [PXUIField(DisplayName = "Tstamp")]
    public virtual byte[] Tstamp { get; set; }
    public abstract class tstamp : PX.Data.BQL.BqlByteArray.Field<tstamp> { }
    #endregion
  }
}