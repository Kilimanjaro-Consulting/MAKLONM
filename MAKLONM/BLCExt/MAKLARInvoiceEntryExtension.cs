using PX.Data;
using PX.Data.Wiki.Parser;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.SO;
using System.Linq;
using static PX.Objects.FA.FABookSettings.midMonthType;

namespace PX.Objects.AR
{
    public class MAKLARInvoiceEntry_Extension : PXGraphExtension<PX.Objects.PM.ARInvoiceEntryExt, PX.Objects.AR.ARInvoiceEntry>
    {


        #region Active
        public static bool IsActive() => PXAccess.FeatureInstalled<FeaturesSet.projectAccounting>();
        #endregion

        public delegate void PersistDelegate();
        [PXOverride]
        public void Persist(PersistDelegate baseMethod)
        {
            //Only for Project Billing
            if (Base.Accessinfo.ScreenID == "PM.30.10.00" || Base.Accessinfo.ScreenID == "PM.50.30.00")
                
            {
                foreach (ARTran line in Base.Transactions.Select())
                {
                    InsertComponents(line);
                    line.ManualPrice = false;
                }
                Base.recalculateDiscountsAction.Press();
            }
            
            baseMethod();
        }

        #region Event Handlers        

        protected void ARTran_InventoryID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            var row = (ARTran)e.Row;
            if (row != null)
            {
                if (Base.Accessinfo.ScreenID != "PM.30.10.00" && Base.Accessinfo.ScreenID != "PM.50.30.00")
                {
                    InsertComponents(row);
                    Base.Transactions.View.RequestRefresh();                   
                }
            }
        }

        //Update or Set Dates for all kit-related lines
        protected void ARTran_ExpenseDate_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            var row = (ARTran)e.Row;
            if (row != null)
            {
                if (Base.Accessinfo.ScreenID != "PM.30.10.00" && Base.Accessinfo.ScreenID != "PM.50.30.00")
                {
                    MAKLARTranExt tranExt = PXCache<ARTran>.GetExtension<MAKLARTranExt>(row);
                    if (tranExt.UsrKitInventoryID != null)
                    {
                        var fourItems = PXSelect<
                            ARTran,
                            Where<MAKLARTranExt.usrKitInventoryID, Equal<Required<MAKLARTranExt.usrKitInventoryID>>,
                                And<ARTran.tranType, Equal<Required<ARTran.tranType>>,
                                And<ARTran.refNbr, Equal<Required<ARTran.refNbr>>,
                                And<ARTran.lineNbr, NotEqual<Required<ARTran.lineNbr>>,
                                And<ARTran.expenseDate, NotEqual<Required<ARTran.expenseDate>>,
                                And<MAKLARTranExt.usrOrigLineNbr, Equal<Required<MAKLARTranExt.usrOrigLineNbr>>>>>>>>>
                            .Select(Base, tranExt.UsrKitInventoryID, row.TranType, row.RefNbr, row.LineNbr, row.ExpenseDate, tranExt.UsrOrigLineNbr);
                        // int updatedcount = 0;
                        foreach (ARTran item in fourItems)
                        {
                            item.ExpenseDate = row.ExpenseDate;
                            Base.Transactions.Update(item);
                            // cache.SetValue<ARTran.expenseDate>(item, row.ExpenseDate);  
                            // updatedcount++;                                                
                        }
                        //  if (updatedcount > 0)
                        //  {
                        //      Base.Transactions.Cache.IsDirty = true;
                        //  }
                        Base.Transactions.View.RequestRefresh();
                    }
                }
            }
        }

        //TODO - Delete all kit-related lines for Kit+Date combination
        protected void ARTran_RowDeleting(PXCache cache, PXRowDeletingEventArgs e, PXRowDeleting InvokeBaseHandler)
        {
            if (InvokeBaseHandler != null)
                InvokeBaseHandler(cache, e);
            var row = (ARTran)e.Row;
            if (row != null)
            {
                if (Base.Accessinfo.ScreenID != "PM.30.10.00" && Base.Accessinfo.ScreenID != "PM.50.30.00")
                {
                    MAKLARTranExt tranExt = PXCache<ARTran>.GetExtension<MAKLARTranExt>(row);
                    if (tranExt.UsrKitInventoryID != null)
                    {
                        var fourItems = PXSelect<
                            ARTran,
                            Where<MAKLARTranExt.usrKitInventoryID, Equal<Required<MAKLARTranExt.usrKitInventoryID>>,
                                And<ARTran.tranType, Equal<Required<ARTran.tranType>>,
                                And<ARTran.refNbr, Equal<Required<ARTran.refNbr>>,
                                And<ARTran.lineNbr, NotEqual<Required<ARTran.lineNbr>>,
                                And<MAKLARTranExt.usrOrigLineNbr, Equal<Required<MAKLARTranExt.usrOrigLineNbr>>>>>>>>
                            .Select(Base, tranExt.UsrKitInventoryID, row.TranType, row.RefNbr, row.LineNbr, tranExt.UsrOrigLineNbr);

                        foreach (ARTran item in fourItems)
                        {
                            Base.Transactions.Delete(item);
                        }
                        Base.Transactions.View.RequestRefresh();
                    }
                }
            }

        }

        #endregion

        public void InsertComponents(ARTran row)
        {
            InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(Base, row.InventoryID);
            if (item != null)
            {
                if (item.KitItem == true)
                {
                    INKitSpecHdr kitspec = PXSelect<
                        INKitSpecHdr,
                        Where<INKitSpecHdr.kitInventoryID, Equal<Required<INKitSpecHdr.kitInventoryID>>,
                        And<INKitSpecHdr.isActive, Equal<True>,
                            And<INKitSpecHdrExt.usrEffectiveDate, LessEqual<Required<INKitSpecHdrExt.usrEffectiveDate>>>>>,
                        OrderBy<Desc<INKitSpecHdrExt.usrEffectiveDate>>>
                        .Select(Base, row.InventoryID, Base.Document.Current.DocDate).RowCast<INKitSpecHdr>().FirstOrDefault();

                    if (kitspec != null)
                    {
                        var kitspecdetails = PXSelect<
                        INKitSpecNonStkDet,
                        Where<INKitSpecNonStkDet.kitInventoryID, Equal<Required<INKitSpecNonStkDet.kitInventoryID>>,
                            And<INKitSpecNonStkDet.revisionID, LessEqual<Required<INKitSpecNonStkDet.revisionID>>>>>
                        .Select(Base, kitspec.KitInventoryID, kitspec.RevisionID);

                        int origLineNbr = 0;

                        foreach (INKitSpecNonStkDet component in kitspecdetails)
                        {
                            INKitSpecNonStkDetExt componentExt = PXCache<INKitSpecNonStkDet>.GetExtension<INKitSpecNonStkDetExt>(component);

                            ARTran tran = new ARTran();
                            tran = Base.Transactions.Insert(tran);
                            MAKLARTranExt tranExt = PXCache<ARTran>.GetExtension<MAKLARTranExt>(tran);
                            tran.InventoryID = component.CompInventoryID;
                            tran.Qty = component.DfltCompQty;
                            tran.ManualPrice = true;
                            tran.CuryUnitPrice = componentExt.UsrRate;
                            tran.CuryTranAmt = componentExt.UsrAmount;
                            tran.Date = row.Date;
                            tranExt.UsrKitInventoryID = component.KitInventoryID;
                            tran.ProjectID = row.ProjectID;
                            tran.TaskID = row.TaskID;                           
                            tran.AccountID = row.AccountID;
                            tran.SubID = row.SubID;

                            if (origLineNbr == 0)
                            {
                                origLineNbr = tran.LineNbr ?? 0;
                            }
                            tranExt.UsrOrigLineNbr = origLineNbr;

                            Base.Transactions.Update(tran);

                        }
                        origLineNbr = 0;
                        Base.Transactions.Delete(row);
                    }
                }
            }
        }

        public delegate void SubtractValuesToInvoiceDelegate(ARTran line, int? revenueAccountGroup, int mult);
        [PXOverride]
        public void SubtractValuesToInvoice(ARTran line, int? revenueAccountGroup, int mult, SubtractValuesToInvoiceDelegate baseMethod)
        {            
            InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(Base, line.InventoryID);
            MAKLARTranExt tranExt = PXCache<ARTran>.GetExtension<MAKLARTranExt>(line);
            if (item != null)
            {
                if (item.KitItem == false && tranExt.UsrOrigLineNbr == null)
                {                    
                        //processing the standard logic only for the lines which are not added via the custom component billing
                        baseMethod(line, revenueAccountGroup, mult);                    
                }
            }
            else
            {
                baseMethod(line, revenueAccountGroup, mult);
            }

        }


        // Ability to select Kits in AR Invoice
        [PXRemoveBaseAttribute(typeof(ARTranInventoryItemAttribute))]
        [PXMergeAttributes(Method = MergeMethod.Append)]
        [SOLineInventoryItem(Filterable = true)]
        protected virtual void ARTran_InventoryID_CacheAttached(PXCache cache)
        {

        }
    }
}