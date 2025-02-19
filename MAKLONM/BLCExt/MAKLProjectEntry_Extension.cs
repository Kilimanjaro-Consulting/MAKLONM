using PX.Common;
using PX.Data;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.PM
{
    public class MAKLProjectEntry_Extension : PXGraphExtension<PX.Objects.PM.ProjectEntry>
    {

        public PXSelect<PMBilling, Where<PMBilling.isActive, Equal<True>>> BillingRulesActive;

        #region Event Handlers        
        protected void PMTask_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
        {

            var row = (PMTask)e.Row;
            MAKLPMTaskExt rowExt = PXCache<PMTask>.GetExtension<MAKLPMTaskExt>(row);
            if (row != null)
            {
                PMProject project = Base.Project.Current;

                if (project != null)
                {
                    PMProject templ = PXSelect<PMProject, Where<PMProject.contractID, Equal<Required<PMProject.contractID>>>>.Select(Base, project.TemplateID);
                    if (templ != null)
                    {
                        PMTask templatetask = PXSelect<PMTask, Where<PMTask.projectID, Equal<Required<PMTask.projectID>>, And<PMTask.taskCD, Equal<Required<PMTask.taskCD>>>>>.Select(Base, templ.ContractID, row.TaskCD);

                        if (templatetask != null)
                        {
                            MAKLPMTaskExt templatetaskext = PXCache<PMTask>.GetExtension<MAKLPMTaskExt>(templatetask);

                            rowExt.UsrItemClass = templatetaskext.UsrItemClass;
                            rowExt.UsrKitInventoryID = templatetaskext.UsrKitInventoryID;
                        }
                    }
                }
            }

        }

        protected void PMTask_UsrBillTo_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            var row = (PMTask)e.Row;
            MAKLPMTaskExt rowext = PXCache<PMTask>.GetExtension<MAKLPMTaskExt>(row);
            PMProject proj = PXSelect<PMProject, Where<PMProject.contractID, Equal<Required<PMProject.contractID>>>>.Select(Base, row.ProjectID);
            if (rowext.UsrBillTo != null)
            {
                PMTask otherTasks = PXSelect<PMTask, Where<PMTask.projectID, Equal<Required<PMTask.projectID>>, And<MAKLPMTaskExt.usrBillTo, Equal<Required<MAKLPMTaskExt.usrBillTo>>,
                    And<PMTask.taskID,NotEqual<Required<PMTask.taskID>>>>>>.Select(Base, row.ProjectID, rowext.UsrBillTo, row.TaskID).RowCast<PMTask>().FirstOrDefault();

                if (otherTasks != null)
                {
                    //assign Billing from other task with the same BillTo, assuming that logic has worked correctly previously
                    row.BillingID = otherTasks.BillingID;
                }
                else 
                {
                    var tasks = Base.Tasks.Select().RowCast<PMTask>().Where(x => x.BillingID != null).ToList();
                    List<string> taskbillingRules = tasks.Select(x => x.BillingID).Distinct().OrderBy(x => x).ToList();

                    var billingRules = BillingRulesActive.Select().RowCast<PMBilling>().OrderBy(x => x.BillingID).ToList();

                    if (taskbillingRules.Count() > 0)
                    {
                        foreach (PMBilling rule in billingRules)
                        {
                            if (rule.BillingID == proj.BillingID) 
                                continue;
                            var matchingRule = taskbillingRules.FirstOrDefault(x => x.Contains(rule.BillingID));
                            if (matchingRule != null)
                            {
                                continue;
                            }
                            else
                            { 
                                row.BillingID = rule.BillingID;
                                break;
                            }
                        }                                       
                    }
                }
            }
            //use Project Billing Rule if Bill To is not overriden
            else
            { 
                row.BillingID = proj.BillingID;
            }

        }

        protected void PMTask_Status_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            var row = (PMTask)e.Row;

            if (row != null)
            {
                if (e.OldValue.ToString() == "D" && row.Status == "A")
                {
                    row.StartDate = row.StartDate ?? row.PlannedStartDate ?? Base.Accessinfo.BusinessDate; ;
                    row.EndDate = row.EndDate ?? row.PlannedEndDate ?? Base.Accessinfo.BusinessDate;
                }
            }
        }
    

        [PXDBBool()]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Group by Task")]
        protected virtual void RevenueBudgetFilter_GroupByTask_CacheAttached(PXCache cache)
        {

        }

        #endregion
    }
}