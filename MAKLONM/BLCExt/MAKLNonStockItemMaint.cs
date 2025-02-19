using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using static PX.Data.BQL.BqlPlaceholder;


namespace PX.Objects.IN
{
    public class MAKLNonStockItemMaint_Extension : PXGraphExtension<PX.Objects.IN.NonStockItemMaint>
    {
        #region Event Handlers

        public delegate void PersistDelegate();
        [PXOverride]
        public void Persist(PersistDelegate baseMethod)
        {
            InventoryItem row = Base.Item.Current;            
            if (row != null)
            {
                if (row.KitItem == true)
                {
                    INKitSpecHdr kitspec = PXSelect<
                        INKitSpecHdr,
                        Where<INKitSpecHdr.kitInventoryID, Equal<Required<INKitSpecHdr.kitInventoryID>>,
                        And<INKitSpecHdr.isActive, Equal<True>,
                            And<INKitSpecHdrExt.usrEffectiveDate, LessEqual<Required<INKitSpecHdrExt.usrEffectiveDate>>>>>,
                        OrderBy<Desc<INKitSpecHdrExt.usrEffectiveDate>>>
                        .Select(Base, row.InventoryID, Base.Accessinfo.BusinessDate).RowCast<INKitSpecHdr>().FirstOrDefault();

                    if (kitspec != null)
                    {
                        var kitspecdetails = PXSelect<
                        INKitSpecNonStkDet,
                        Where<INKitSpecNonStkDet.kitInventoryID, Equal<Required<INKitSpecNonStkDet.kitInventoryID>>,
                            And<INKitSpecNonStkDet.revisionID, LessEqual<Required<INKitSpecNonStkDet.revisionID>>>>>
                        .Select(Base, kitspec.KitInventoryID, kitspec.RevisionID);

                        foreach (INKitSpecNonStkDet component in kitspecdetails)
                        {
                            InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(Base, component.CompInventoryID);
                            if (item != null)
                            {
                                item.SalesSubID = row.SalesSubID;
                                Base.Item.Update(item);
                            }
                        }
                    }
                }
            }
            baseMethod();

        }
        #endregion
    }

    //protected void InventoryItem_SalesSubID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
    //{

    //    var row = (InventoryItem)e.Row;
    //    if (row != null)
    //    {
    //        if (row.KitItem == true)
    //        {
    //            INKitSpecHdr kitspec = PXSelect<
    //                INKitSpecHdr,
    //                Where<INKitSpecHdr.kitInventoryID, Equal<Required<INKitSpecHdr.kitInventoryID>>,
    //                And<INKitSpecHdr.isActive, Equal<True>,
    //                    And<INKitSpecHdrExt.usrEffectiveDate, LessEqual<Required<INKitSpecHdrExt.usrEffectiveDate>>>>>,
    //                OrderBy<Desc<INKitSpecHdrExt.usrEffectiveDate>>>
    //                .Select(Base, row.InventoryID, Base.Accessinfo.BusinessDate).RowCast<INKitSpecHdr>().FirstOrDefault();

    //            if (kitspec != null)
    //            {
    //                var kitspecdetails = PXSelect<
    //                INKitSpecNonStkDet,
    //                Where<INKitSpecNonStkDet.kitInventoryID, Equal<Required<INKitSpecNonStkDet.kitInventoryID>>,
    //                    And<INKitSpecNonStkDet.revisionID, LessEqual<Required<INKitSpecNonStkDet.revisionID>>>>>
    //                .Select(Base, kitspec.KitInventoryID, kitspec.RevisionID);

    //                foreach (INKitSpecNonStkDet component in kitspecdetails)
    //                {
    //                    InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(Base, component.CompInventoryID);
    //                    if (item != null)
    //                    {
    //                        item.SalesSubID = row.SalesSubID;                                
    //                    }
    //                }
    //            }
    //        }
    //    }

    //}


    // }
}