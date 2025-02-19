using PX.Data.ReferentialIntegrity.Attributes;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.CT;
using PX.Objects.EP;
using PX.Objects.IN;
using PX.Objects.PM;
using PX.Objects;
using System.Collections.Generic;
using System;
using PX.TM;

namespace PX.Objects.PM
{
  public class MAKLPMSetupExt : PXCacheExtension<PX.Objects.PM.PMSetup>
  {
    #region Active
    public static bool IsActive() => PXAccess.FeatureInstalled<FeaturesSet.projectAccounting>();       
    #endregion

    #region UsrCalendar
    [PXDBString(10, IsUnicode = true)]
    [PXSelector(typeof(Search<CSCalendar.calendarID>), DescriptionField = typeof(CSCalendar.description))]
    [PXUIField(DisplayName="Default Calendar")]
    public virtual string UsrCalendar { get; set; }
    public abstract class usrCalendar : PX.Data.BQL.BqlString.Field<usrCalendar> { }
        #endregion

    #region UsrDefaultOwnerID
    [Owner(DisplayName = "Default Activity Owner")]
    public virtual int? UsrDefaultOwnerID { get; set; }
    public abstract class usrDefaultOwnerID : PX.Data.BQL.BqlInt.Field<usrDefaultOwnerID> { }
    #endregion

     #region UsrDefaultActivityType
    [PXDBString(5, IsFixed = true, IsUnicode = false)]
    [PXUIField(DisplayName="Default Activity Type")]
    [PXSelector(typeof(EPActivityType.type), DescriptionField = typeof(EPActivityType.description))]
    [PXRestrictor(typeof(Where<EPActivityType.active, Equal<True>>), PX.Objects.CR.Messages.InactiveActivityType, typeof(EPActivityType.type))]    
    [PXRestrictor(typeof(Where<EPActivityType.requireTimeByDefault, Equal<True>, And<EPActivityType.isInternal, Equal<True>>>), PX.Objects.CR.Messages.ExternalActivityType, typeof(EPActivityType.type))]
    public virtual string UsrDefaultActivityType { get; set; }
    public abstract class usrDefaultActivityType : PX.Data.BQL.BqlString.Field<usrDefaultActivityType> { }
    #endregion


    }
}