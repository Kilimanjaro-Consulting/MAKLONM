using PX.Data;
using PX.Objects.AR;
using System;
using System.Collections.Generic;
using System.Linq;
using static PX.Objects.PM.PMBillEngine;

namespace PX.Objects.PM
{
    public class PMBillEngine_Extension : PXGraphExtension<PX.Objects.PM.PMBillEngine>
    {
        #region Event Handlers

        public delegate void InsertTransactionsInInvoiceDelegate(PMProject project, List<BillingData> unbilled, Int32 mult);
        [PXOverride]
        public void InsertTransactionsInInvoice(PMProject project, List<BillingData> unbilled, Int32 mult, InsertTransactionsInInvoiceDelegate baseMethod)
        {
            if (Base.InvoiceEntry.Transactions.Select().Count() == 0)
            {
                int? firsttask = unbilled.Select(f => f.Tran.TaskID).FirstOrDefault();
                if (firsttask != null)
                {
                    PMTask task = PXSelect<PMTask, Where<PMTask.taskID, Equal<Required<PMTask.taskID>>>>.Select(Base, firsttask);
                    if (task != null)
                    {
                        MAKLPMTaskExt taskext = PXCache<PMTask>.GetExtension<MAKLPMTaskExt>(task);
                        if (taskext.UsrBillTo != null)
                        {
                            Customer billingCustomer = PXSelect<Customer, Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>.Select(Base, taskext.UsrBillTo);

                            Base.InvoiceEntry.Document.Current.CustomerID = billingCustomer.BAccountID;
                            Base.InvoiceEntry.Document.Current.CustomerLocationID = billingCustomer.DefLocationID;

                            Base.InvoiceEntry.Document.SetValueExt<ARInvoice.customerID>(Base.InvoiceEntry.Document.Current, billingCustomer.BAccountID);//SetValueExt needed for correct BaseCuryID (MBC) defaulting.

                            Base.InvoiceEntry.Document.Update(Base.InvoiceEntry.Document.Current);
                        }
                    }
                }

            }

            baseMethod(project, unbilled, mult);

            #endregion
        }
    }
}