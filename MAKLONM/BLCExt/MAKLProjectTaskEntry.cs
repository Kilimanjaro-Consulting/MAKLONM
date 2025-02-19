using PX.Data;
using PX.Objects.CR;
using PX.Objects.IN;
using System;
using System.Collections;
using PX.Objects.EP;
using System.Collections.Generic;
using PX.Objects.CS;

namespace PX.Objects.PM
{
  public class MAKLProjectTaskEntry_Extension : PXGraphExtension<PX.Objects.PM.ProjectTaskEntry>
  {
    #region Event Handlers

    #endregion
  
        #region Actions       


public PXAction<PMTask> runCalendarBilling;

[PXUIField(DisplayName = "Run Calendar Billing", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]   
[PXButton]

public IEnumerable RunCalendarBilling(PXAdapter adapter)
{
    if (!(adapter.View.Cache.Current is PMTask row))
        return adapter.Get();

            PXLongOperation.StartOperation(Base, delegate ()
            {
                try
                {
                    DateTime billingDate = Base.Accessinfo.BusinessDate ?? DateTime.Now;
                    BillTask(Base.Task.Current, billingDate, billingDate);
                }
                catch (PXSetPropertyException ex)
                {
                    throw new PXException(String.Format(
                                 "Failed to bill Project Task: {0}", ex.Message));
                }


            });

            //do something

            if (Base.IsImport && Base.IsContractBasedAPI)
    {
        Base.Actions.PressSave();
    }

    return adapter.Get();
}

        public void BillTask(PMTask task, DateTime fromDate, DateTime toDate)
        {
            Base.Actions.PressSave();

            List<DateTime> listofdates = new List<DateTime>();
            MAKLPMTaskExt taskext = PXCache<PMTask>.GetExtension<MAKLPMTaskExt>(task);

            PMSetup setup = Base.Setup.SelectSingle();
            MAKLPMSetupExt setupext = PXCache<PMSetup>.GetExtension<MAKLPMSetupExt>(setup);

            for (var day = fromDate; day < toDate; day = day.AddDays(1))
            {
                if (day.Date >= task.StartDate && day.Date <= task.EndDate)
                {
                    CSCalendarExceptions ph = (CSCalendarExceptions)PXSelect<
                        CSCalendarExceptions,
                        Where<CSCalendarExceptions.calendarID, Equal<Required<CSCalendarExceptions.calendarID>>,
                            And<CSCalendarExceptions.date, Equal<Required<CSCalendarExceptions.date>>>>>.Select(Base, setupext.UsrCalendar, day.Date);

                    if (ph == null)
                    {
                        switch (day.DayOfWeek)
                        {
                            case DayOfWeek.Monday:
                                if (taskext.UsrMon == true)
                                    listofdates.Add(day);
                                break;
                            case DayOfWeek.Tuesday:
                                if (taskext.UsrTue == true)
                                    listofdates.Add(day);
                                break;
                            case DayOfWeek.Wednesday:
                                if (taskext.UsrWed == true)
                                    listofdates.Add(day);
                                break;
                            case DayOfWeek.Thursday:
                                if (taskext.UsrThu == true)
                                    listofdates.Add(day);
                                break;
                            case DayOfWeek.Friday:
                                if (taskext.UsrFri == true)
                                    listofdates.Add(day);
                                break;
                        }
                    }
                }
            }

            if (listofdates.Count > 0)
            {
                CRActivityMaint graph = PXGraph.CreateInstance<CRActivityMaint>();
                

                foreach (DateTime day in listofdates)
                {
                    AddCalendarEvent(graph, task, day, setupext);

                    taskext.UsrLastActivityDate = day;
                    Base.Task.Update(task);
                    Base.Persist();
                }
            }
        } 

        protected void AddCalendarEvent(CRActivityMaint graph, PMTask task, DateTime day, MAKLPMSetupExt setupext, bool isMassProcess = false)
        {            

           CRActivity exists = (CRActivity)PXSelect<
                    CRActivity,
                    Where<CRActivity.refNoteID, Equal<Required<CRActivity.noteID>>,
                        And<CRActivity.startDate, Equal<Required<CRActivity.startDate>>,And<CRActivity.type,Equal<Required<CRActivity.type >>>>>>.Select(Base, task.NoteID ,day.Date.AddHours(8), setupext.UsrDefaultActivityType);
            if (exists != null)
                return;

            MAKLPMTaskExt taskext = PXCache<PMTask>.GetExtension<MAKLPMTaskExt>(task);            

            if (taskext.UsrKitInventoryID == null)
            {
                throw new PXException(String.Format(
                                 "Kit Item is not linked to Project Task: {0}", task.TaskCD));
            }

            if (task.IsActive != true)
            {
                throw new PXException(String.Format(
                                 "Task {0} is not active", task.TaskCD));
            }

            PMProject project = PXSelect<PMProject, Where<PMProject.contractID, Equal<Required<PMProject.contractID>>>>.Select(Base, task.ProjectID);
           
            InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(Base, taskext.UsrKitInventoryID);

            int ownertoassign = project.OwnerID ?? setupext.UsrDefaultOwnerID ?? 0;
            EPEmployee employee = PXSelect<EPEmployee, Where<EPEmployee.defContactID, Equal<Required<EPEmployee.defContactID>>>>.Select(Base, ownertoassign);

            CRActivity act = new CRActivity();
            act = graph.Activities.Insert(act);
            act.Subject = String.Concat(item.Descr, " - ", day.DayOfWeek);
            act.ClassID = CRActivityClass.Activity;
            act.BAccountID = project.CustomerID;
            act.RefNoteID = task.NoteID;
            act.RefNoteIDType = task.GetType().FullName;
            act.OwnerID = ownertoassign;            
            act.Type = setupext.UsrDefaultActivityType;           
            //act.CategoryID = 5;
            // act.ShowAsID = 7;

            DateTime startdate = day.Date;
            DateTime enddate = day.Date;

            act.StartDate = startdate.AddHours(8);
            act.EndDate = enddate.AddHours(16);
            
            graph.Activities.Update(act);

            PMTimeActivity tAct = (PMTimeActivity)graph.TimeActivitiesOld.Cache.CreateInstance();
            tAct = graph.TimeActivitiesOld.Insert(tAct);
            tAct.TrackTime = true;
            tAct.TimeSpent = 60;
            tAct.IsBillable = true;
            tAct.Summary = item.Descr;
            tAct.ProjectID = task.ProjectID;
            tAct.LabourItemID = item.InventoryID;
            tAct.ApprovalStatus = ApprovalStatusAttribute.Completed;
            if (employee != null)
            {
                tAct.ApproverID = employee.BAccountID;
            }
            graph.TimeActivitiesOld.Cache.SetValue<PMTimeActivity.projectTaskID>(tAct, task.TaskID);
            
            tAct = graph.TimeActivitiesOld.Update(tAct);
            
            graph.Actions.PressSave();            
        }
        #endregion Actions
    }
}