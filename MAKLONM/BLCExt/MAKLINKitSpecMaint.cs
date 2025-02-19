using PX.Data;

namespace PX.Objects.IN
{
  public class MAKLINKitSpecMaint_Extension : PXGraphExtension<PX.Objects.IN.INKitSpecMaint>
  {
    #region Event Handlers

    protected void INKitSpecHdr_RowPersisting(PXCache cache, PXRowPersistingEventArgs e, PXRowPersisting InvokeBaseHandler)
    {
     // if(InvokeBaseHandler != null)
     //   InvokeBaseHandler(cache, e);

    //removing logic to have only one revision for Non-Stock Item
      var row = (INKitSpecHdr)e.Row;
     // e.Cancel = true;
      

    }

    

    #endregion
  }
}