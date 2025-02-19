using PX.Data;
using PX.Objects.CS;


namespace PX.Objects.IN
{
  public class InventoryItemExt : PXCacheExtension<PX.Objects.IN.InventoryItem>
  {
    #region Active
    public static bool IsActive() => PXAccess.FeatureInstalled<FeaturesSet.projectAccounting>();       
    #endregion

    #region UsrNDISCode
    [PXDBString(50)]
    [PXUIField(DisplayName="NDIS Code")]
    public virtual string UsrNDISCode { get; set; }
    public abstract class usrNDISCode : PX.Data.BQL.BqlString.Field<usrNDISCode> { }
    #endregion

    #region UsrNDISDescr
    [PXDBString(256)]
    [PXUIField(DisplayName="NDIS Description")]
    public virtual string UsrNDISDescr { get; set; }
    public abstract class usrNDISDescr : PX.Data.BQL.BqlString.Field<usrNDISDescr> { }
    #endregion
  }
}