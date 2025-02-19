using PX.Data;


namespace PX.Objects.CR
{
  public class MAKLContactExt : PXCacheExtension<PX.Objects.CR.Contact>
  {
    #region UsrDsqStage
    [PXDBString(10)]
    [PXUIField(DisplayName="Disqualify Stage")]
    [PXStringList()]
    [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
    public virtual string UsrDsqStage { get; set; }
    public abstract class usrDsqStage : PX.Data.BQL.BqlString.Field<usrDsqStage> { }
    #endregion

    #region UsrDsqReason
    [PXDBString(10)]
    [PXUIField(DisplayName="Disqualify Reason")]
    [PXStringList()]
    [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
    public virtual string UsrDsqReason { get; set; }
    public abstract class usrDsqReason : PX.Data.BQL.BqlString.Field<usrDsqReason> { }
    #endregion
  }
}