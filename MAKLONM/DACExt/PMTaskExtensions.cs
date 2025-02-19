using PX.Data.BQL;
using PX.Data.EP;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CN.ProjectAccounting.Descriptor;
using PX.Objects.CN.ProjectAccounting.PM.Descriptor;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.GL;
using PX.Objects.PM;
using PX.Objects.TX;
using PX.Objects;
using System.Collections.Generic;
using System;
using PX.Objects.IN;

namespace PX.Objects.PM
{
    public class MAKLPMTaskExt : PXCacheExtension<PX.Objects.PM.PMTask>
    {
        #region Active
        public static bool IsActive() => PXAccess.FeatureInstalled<FeaturesSet.projectAccounting>();       
        #endregion

        #region UsrMon
        [PXDBBool]
        [PXUIField(DisplayName = "Mon")]
        public virtual bool? UsrMon { get; set; }
        public abstract class usrMon : PX.Data.BQL.BqlBool.Field<usrMon> { }
        #endregion

        #region UsrTue
        [PXDBBool]
        [PXUIField(DisplayName = "Tue")]
        public virtual bool? UsrTue { get; set; }
        public abstract class usrTue : PX.Data.BQL.BqlBool.Field<usrTue> { }
        #endregion

        #region UsrWed
        [PXDBBool]
        [PXUIField(DisplayName = "Wed")]

        public virtual bool? UsrWed { get; set; }
        public abstract class usrWed : PX.Data.BQL.BqlBool.Field<usrWed> { }
        #endregion

        #region UsrThu
        [PXDBBool]
        [PXUIField(DisplayName = "Thu")]

        public virtual bool? UsrThu { get; set; }
        public abstract class usrThu : PX.Data.BQL.BqlBool.Field<usrThu> { }
        #endregion

        #region UsrFri
        [PXDBBool]
        [PXUIField(DisplayName = "Fri")]

        public virtual bool? UsrFri { get; set; }
        public abstract class usrFri : PX.Data.BQL.BqlBool.Field<usrFri> { }
        #endregion

        #region UsrBillTo
        //[PXDBInt]
        [CustomerActive(DescriptionField = typeof(Customer.acctName))]
        [PXUIField(DisplayName = "Billing Customer")]

        public virtual int? UsrBillTo { get; set; }
        public abstract class usrBillTo : PX.Data.BQL.BqlInt.Field<usrBillTo> { }
        #endregion

        #region UsrLastActivityDate
        [PXDBDate]
        [PXUIField(DisplayName = "Last Activity Date")]
        public virtual DateTime? UsrLastActivityDate { get; set; }
        public abstract class usrLastActivityDate : PX.Data.BQL.BqlDateTime.Field<usrLastActivityDate> { }
        #endregion

        #region UsrKitInventoryID
        [Inventory(Visibility = PXUIVisibility.SelectorVisible, DisplayName = "Kit Inventory ID")]
        [PXRestrictor(typeof(Where<InventoryItem.kitItem, Equal<boolTrue>>), PX.Objects.IN.Messages.InventoryItemIsNotaKit)]
        [PXRestrictor(typeof(Where<InventoryItem.itemClassID, Equal<Current<MAKLPMTaskExt.usrItemClass>>>), "Item must have the same item class which is assigned to Task Template", typeof(InventoryItem.inventoryCD))]
        public virtual int? UsrKitInventoryID { get; set; }
        public abstract class usrKitInventoryID : PX.Data.BQL.BqlInt.Field<usrKitInventoryID> { }
        #endregion

        #region UsrItemClass
        [PXDBInt]        
        [PXUIField(DisplayName = "Item Class", Visibility = PXUIVisibility.SelectorVisible)]
        [PXDimensionSelector(INItemClass.Dimension, typeof(Search<INItemClass.itemClassID>), typeof(INItemClass.itemClassCD), DescriptionField = typeof(INItemClass.descr), CacheGlobal = true)]        
        public virtual int? UsrItemClass { get; set; }
        public abstract class usrItemClass : PX.Data.BQL.BqlInt.Field<usrItemClass> { }
        #endregion


    }
}