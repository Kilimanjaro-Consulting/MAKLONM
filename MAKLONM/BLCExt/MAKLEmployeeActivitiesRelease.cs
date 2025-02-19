using PX.Data;
using PX.Objects.PM;
using PX.Objects.AR;
using static PX.Objects.CS.TermsDueType;

namespace PX.Objects.EP
{
  public class EmployeeActivitiesRelease_Extension : PXGraphExtension<PX.Objects.EP.EmployeeActivitiesRelease>
  {
    #region Event Handlers

    protected void EPActivityApprove_UsrCustomerName_FieldSelecting(PXCache cache, PXFieldSelectingEventArgs e)
    {
      
      var row = (EPActivityApprove)e.Row;
        if (row != null )
            {
                if (row.ProjectID != null)
                {
                    PMProject proj = PXSelect<PMProject, Where<PMProject.contractID, Equal<Required<PMProject.contractID>>>>.Select(Base, row.ProjectID);
                    if (proj != null)
                    {
                        Customer cust = PXSelect<Customer, Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>.Select(Base, proj.CustomerID);
                        if (cust != null)
                        {
                            e.ReturnValue = cust.AcctName;
                        }
                        
                    }
                }
            }
      
    }

    

    #endregion
  }
}