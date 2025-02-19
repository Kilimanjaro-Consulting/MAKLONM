using PX.Data;

namespace PX.Objects.EP
{
    public class MAKLEmployeeActivitiesApprove_Extension : PXGraphExtension<PX.Objects.EP.EmployeeActivitiesApprove>
    {
        #region Event Handlers

        protected void EPActivityApprove_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {

            var row = (EPActivityApprove)e.Row;
            PXUIFieldAttribute.SetEnabled<EPActivityApprove.isBillable>(cache, row, true);

        }



        #endregion
    }
}