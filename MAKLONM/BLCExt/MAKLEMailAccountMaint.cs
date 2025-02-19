using PX.Data;

namespace PX.SM
{
    public class MAKLEMailAccountMaint_Extension : PXGraphExtension<PX.SM.EMailAccountMaint>
    {
        #region Event Handlers

        protected void EMailAccount_RowSelected(PXCache cache, PXRowSelectedEventArgs e, PXRowSelected InvokeBaseHandler)
        {
            if (InvokeBaseHandler != null)
                InvokeBaseHandler(cache, e);
            var row = (EMailAccount)e.Row;
            if (row != null)
            {
                MAKLEMailAccountExt accountExt = PXCache<EMailAccount>.GetExtension<MAKLEMailAccountExt>(row);
                PXUIFieldAttribute.SetEnabled<EMailAccount.createLeadClassID>(cache, row, row.IncomingProcessing == true && accountExt.UsrCreateNewLeadEnhanced == true);
            }
        }

        #endregion
    }
}