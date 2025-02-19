using PX.Data;
using PX.Objects;
using PX.SM;
using System;

namespace PX.SM
{
  public class MAKLEMailAccountExt : PXCacheExtension<PX.SM.EMailAccount>
  {
    #region UsrCreateNewLeadEnhanced
    [PXDBBool]
    [PXUIField(DisplayName="Create New Lead Enhanced")]
    public virtual bool? UsrCreateNewLeadEnhanced { get; set; }
    public abstract class usrCreateNewLeadEnhanced : PX.Data.BQL.BqlBool.Field<usrCreateNewLeadEnhanced> { }
    #endregion
  }
}