using System;
using PX.Data;
using System.Collections.Generic;
using PX.Objects.CR;
using static PX.Objects.PM.PMQuoteMaint;
using PX.Objects.IN;

namespace PX.Objects.PM
{
    public class MAKLPMQuoteMaint_Extension : PXGraphExtension<PX.Objects.PM.PMQuoteMaint>
    {
        public delegate void ConvertQuoteToProjectDelegate(PMQuote row, ConvertToProjectFilter settings);
        [PXOverride]
        public void ConvertQuoteToProject(PMQuote row, ConvertToProjectFilter settings, ConvertQuoteToProjectDelegate baseMethod)
        {
            baseMethod(row, settings);            
        }

        #region Event Handlers
        public delegate void AddingTasksToProjectDelegate(PMQuote row, ProjectEntry projectEntry, Dictionary<String, Int32> taskMap, Nullable<Boolean> copyNotes, Nullable<Boolean> copyFiles);
        [PXOverride]
        public void AddingTasksToProject(PMQuote row, ProjectEntry projectEntry, Dictionary<String, Int32> taskMap, Nullable<Boolean> copyNotes, Nullable<Boolean> copyFiles, AddingTasksToProjectDelegate baseMethod)
        {
            baseMethod(row, projectEntry, taskMap, copyNotes, copyFiles);

            List<PMTask> taskList = new List<PMTask>(projectEntry.Tasks.Select().RowCast<PMTask>());

            foreach (PMTask task in taskList)
            {
                PMQuoteTask templateQuoteTask = PXSelect<PMQuoteTask, Where<PMQuoteTask.taskCD, Equal<Required<PMQuoteTask.taskCD>>,
                    And<PMQuoteTask.quoteID, Equal<Required<PMQuoteTask.quoteID>>>>>.Select(Base, task.TaskCD, row.QuoteID);
                if (templateQuoteTask != null)
                {
                    MAKLPMQuoteTaskExt templateQuoteTaskExt = PXCache<PMQuoteTask>.GetExtension<MAKLPMQuoteTaskExt>(templateQuoteTask);
                    MAKLPMTaskExt taskExt = PXCache<PMTask>.GetExtension<MAKLPMTaskExt>(task);
                    taskExt.UsrMon = templateQuoteTaskExt.UsrMon;
                    taskExt.UsrTue = templateQuoteTaskExt.UsrTue;
                    taskExt.UsrWed = templateQuoteTaskExt.UsrWed;
                    taskExt.UsrThu = templateQuoteTaskExt.UsrThu;
                    taskExt.UsrFri = templateQuoteTaskExt.UsrFri;
                    taskExt.UsrBillTo = templateQuoteTaskExt.UsrBillTo;
                    taskExt.UsrKitInventoryID = templateQuoteTaskExt.UsrKitInventoryID;
                    taskExt.UsrItemClass = templateQuoteTaskExt.UsrItemClass;

                    task.StartDate = task.PlannedStartDate;
                    task.EndDate = task.PlannedEndDate;

                    projectEntry.Tasks.Update(task);
                }               
            }
        }

        protected void PMQuoteTask_UsrKitInventoryID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            var row = (PMQuoteTask)e.Row;
            if (row != null )
            {
                MAKLPMQuoteTaskExt rowExt = PXCache<PMQuoteTask>.GetExtension<MAKLPMQuoteTaskExt>(row);
                InventoryItem kitItem = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(Base, rowExt.UsrKitInventoryID ?? 0);
                if (kitItem != null )
                {
                    rowExt.UsrItemClass = kitItem.ItemClassID;
                }                
            }
        }
        #endregion
        
    }
}