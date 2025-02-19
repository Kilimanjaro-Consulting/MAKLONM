using PX.Data;
using PX.Objects.CS;
using PX.Objects.IN;
using System;

namespace PX.Objects.AR
{
    public class MAKLARTranExt : PXCacheExtension<PX.Objects.AR.ARTran>
    {
        #region UsrKitInventoryID
        [Inventory(Visibility = PXUIVisibility.SelectorVisible, DisplayName = "Kit Inventory ID")]
        [PXRestrictor(typeof(Where<InventoryItem.kitItem, Equal<boolTrue>>), PX.Objects.IN.Messages.InventoryItemIsNotaKit)]
        public virtual int? UsrKitInventoryID { get; set; }
        public abstract class usrKitInventoryID : PX.Data.BQL.BqlInt.Field<usrKitInventoryID> { }
        #endregion

        #region UsrOrigLineNbr
        [PXDBInt]
        [PXUIField(DisplayName = "Original LineNbr")]
        public virtual int? UsrOrigLineNbr { get; set; }
        public abstract class usrOrigLineNbr : PX.Data.BQL.BqlInt.Field<usrOrigLineNbr> { }
        #endregion
    }

    [PXNonInstantiatedExtension]
    public class AR_ARTran_ExistingColumn : PXCacheExtension<PX.Objects.AR.ARTran>
    {
        #region Date  
        [PXMergeAttributes(Method = MergeMethod.Append)]
        [PXUIField(DisplayName = "Activity Date")]
        public DateTime? Date { get; set; }
        #endregion
    }
}