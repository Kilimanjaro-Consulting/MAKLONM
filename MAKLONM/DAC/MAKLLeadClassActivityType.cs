using System;
using PX.Data;
using PX.Objects.CR;
using PX.Objects.EP;

namespace MAKLONM
{
  [Serializable]
  [PXCacheName("MAKLLeadClassActivityType")]
  public class MAKLLeadClassActivityType : IBqlTable
  {
    #region RecordID
    [PXDBIdentity(IsKey = true)]   
    [PXUIField(DisplayName = "Record ID", Enabled = false)]
    public virtual int? RecordID { get; set; }   
    public abstract class recordID : PX.Data.BQL.BqlInt.Field<recordID> { }
    #endregion
      
    #region ClassID
    [PXDBString(10, IsUnicode = true, InputMask = "")]
    [PXUIField(DisplayName = "Class ID")]
    [PXSelector(typeof(CRLeadClass.classID), DescriptionField = typeof(CRLeadClass.description), CacheGlobal = true)]
    [PXParent(typeof(Select<CRLeadClass, Where<CRLeadClass.classID, Equal<Current<classID>>>>))]  
    [PXDBDefault(typeof(CRLeadClass.classID))]
    public virtual string ClassID { get; set; }
    public abstract class classID : PX.Data.BQL.BqlString.Field<classID> { }
    #endregion

    #region Type
    [PXDBString(5, IsFixed = true, InputMask = "")]
    [PXUIField(DisplayName = "Activity Type")]
    [PXSelector(typeof(EPActivityType.type), DescriptionField = typeof(EPActivityType.description))]
	[PXRestrictor(typeof(Where<EPActivityType.active, Equal<True>>), PX.Objects.CR.Messages.InactiveActivityType, typeof(EPActivityType.type))]	
    [PXRestrictor(typeof(Where<EPActivityType.isInternal, Equal<True>, Or<EPActivityType.isSystem, Equal<True>>>), PX.Objects.CR.Messages.ExternalActivityType, typeof(EPActivityType.type))]
    public virtual string Type { get; set; }
    public abstract class type : PX.Data.BQL.BqlString.Field<type> { }
    #endregion

    #region Description
    [PXDBString(255, IsUnicode = true, InputMask = "")]
    [PXUIField(DisplayName = "Description")]
    public virtual string Description { get; set; }
    public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
    #endregion

    #region IsActive
    [PXDBBool()]
    [PXUIField(DisplayName = "Active")]
    [PXDefault(true, PersistingCheck = PXPersistingCheck.Nothing)]
    public virtual bool? IsActive { get; set; }
    public abstract class isActive : PX.Data.BQL.BqlBool.Field<isActive> { }
    #endregion

    #region Tstamp
    [PXDBTimestamp()]
    [PXUIField(DisplayName = "Tstamp")]
    public virtual byte[] Tstamp { get; set; }
    public abstract class tstamp : PX.Data.BQL.BqlByteArray.Field<tstamp> { }
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