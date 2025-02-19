using PX.Data.ReferentialIntegrity.Attributes;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects;
using System.Collections.Generic;
using System;
using PX.Objects.GL;

namespace PX.Objects.IN
{
  public class INKitSpecNonStkDetExt : PXCacheExtension<PX.Objects.IN.INKitSpecNonStkDet>
  {
    #region Active
    public static bool IsActive() => PXAccess.FeatureInstalled<FeaturesSet.projectAccounting>();       
    #endregion

    #region UsrRate
    [PXDBDecimal]
    [PXUIField(DisplayName="Rate")]

    public virtual Decimal? UsrRate { get; set; }
    public abstract class usrRate : PX.Data.BQL.BqlDecimal.Field<usrRate> { }
    #endregion

    #region UsrAmount
    [PXDBDecimal]
    [PXFormula(typeof(Mult<INKitSpecNonStkDet.dfltCompQty, INKitSpecNonStkDetExt.usrRate>), typeof(SumCalc<INKitSpecHdrExt.usrAmount>))]   
    [PXUIField(DisplayName="Charge Amount", Enabled = false)]
    public virtual Decimal? UsrAmount { get; set; }
    public abstract class usrAmount : PX.Data.BQL.BqlDecimal.Field<usrAmount> { }
    #endregion
  }
}