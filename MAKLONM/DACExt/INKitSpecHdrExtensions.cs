using PX.Data.ReferentialIntegrity.Attributes;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects;
using System.Collections.Generic;
using System;

namespace PX.Objects.IN
{
  public class INKitSpecHdrExt : PXCacheExtension<PX.Objects.IN.INKitSpecHdr>
  {
    #region Active
    public static bool IsActive() => PXAccess.FeatureInstalled<FeaturesSet.projectAccounting>();       
    #endregion
    //#region UsrNDISDescription
    //[PXDBString(256)]
    //[PXUIField(DisplayName="NDIS Description")]

    //public virtual string UsrNDISDescription { get; set; }
    //public abstract class usrNDISDescription : PX.Data.BQL.BqlString.Field<usrNDISDescription> { }
    //#endregion    

    #region UsrAmount
    [PXDBDecimal]
    [PXUIField(DisplayName="Total Amount", Enabled = false)]

    public virtual Decimal? UsrAmount { get; set; }
    public abstract class usrAmount : PX.Data.BQL.BqlDecimal.Field<usrAmount> { }
    #endregion

    #region UsrEffectiveDate
    [PXDBDate]
    [PXUIField(DisplayName="Effective Date")]

    public virtual DateTime? UsrEffectiveDate { get; set; }
    public abstract class usrEffectiveDate : PX.Data.BQL.BqlDateTime.Field<usrEffectiveDate> { }
    #endregion
  }
}