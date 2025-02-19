using System;
using System.Collections;
using PX.Api.Models;
using PX.Data;
using PX.Data.BQL;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.PM;
using PX.Objects.SO;
using PX.Objects.TX;

namespace MAKLONM
{
    public class MAKLCalendarBillingProcess : PXGraph<MAKLCalendarBillingProcess>
    {

        public PXCancel<ActionFilter> Cancel;
        public PXFilter<ActionFilter> Filter;

        [PXFilterable]
        public PXFilteredProcessing<PMTask, ActionFilter> TasksToProcess;

        protected bool _ActionChanged = false;

        public virtual IEnumerable tasksToProcess()
        {
            if (Filter.Current.Action == "UNDF")
            {
                yield break;
            }

            ActionFilter filter = Filter.Current;


            string actionID = filter.Action;

            if (_ActionChanged)
            {
                TasksToProcess.Cache.Clear();
            }

            PXSelectBase<PMTask> cmd = new PXSelect<PMTask,
            Where<PMTask.isActive, Equal<True>,
            And<PMTask.startDate, LessEqual<Current<ActionFilter.toDate>>,
            And<PMTask.endDate, GreaterEqual<Current<ActionFilter.fromDate>>,
            And<Where2<Where<MAKLPMTaskExt.usrLastActivityDate, Less<Current<ActionFilter.toDate>>, Or<MAKLPMTaskExt.usrLastActivityDate, IsNull>>,
            And<Where<MAKLPMTaskExt.usrMon, Equal<True>,
            Or<MAKLPMTaskExt.usrTue, Equal<True>,
            Or<MAKLPMTaskExt.usrWed, Equal<True>,
            Or<MAKLPMTaskExt.usrThu, Equal<True>,
            Or<MAKLPMTaskExt.usrFri, Equal<True>>>>>>>>>>>>>(this);

            WorkdayCount daycount = GetWorkdayCount(this, filter.FromDate ?? DateTime.Now, filter.ToDate?.AddDays(1).AddTicks(-1) ?? DateTime.Now);

            int startRow = PXView.StartRow;
            int totalRows = 0;

            var resulttest = cmd.View.Select(PXView.Currents, null, PXView.Searches, PXView.SortColumns, PXView.Descendings, PXView.Filters, ref startRow, PXView.MaximumRows, ref totalRows);

            foreach (PMTask res in cmd.View.Select(PXView.Currents, null, PXView.Searches, PXView.SortColumns, PXView.Descendings, PXView.Filters, ref startRow, PXView.MaximumRows, ref totalRows))
            {
                PMTask item = res;
                MAKLPMTaskExt itemext = PXCache<PMTask>.GetExtension<MAKLPMTaskExt>(item);

                PMProject project = PXSelect<PMProject, Where<PMProject.contractID, Equal<Required<PMProject.contractID>>>>.Select(this, item.ProjectID);

                PMTask cached = (PMTask)TasksToProcess.Cache.Locate(item);
                if (cached != null)
                {
                    item.Selected = cached.Selected;
                }

                if (project.IsActive == true &&
                    project.IsCompleted != true &&
                    project.IsCancelled != true &&
                    (daycount.MonCount > 0 && itemext.UsrMon == true ||
                    daycount.TueCount > 0 && itemext.UsrTue == true ||
                    daycount.WedCount > 0 && itemext.UsrWed == true ||
                    daycount.ThuCount > 0 && itemext.UsrThu == true ||
                    daycount.FriCount > 0 && itemext.UsrFri == true))
                {
                    yield return item;
                }
                PXView.StartRow = 0;
            }
        }
        public MAKLCalendarBillingProcess()
        {
            TasksToProcess.SetSelected<PMTask.selected>();
        }

        public virtual void ActionFilter_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
            _ActionChanged = !sender.ObjectsEqual<ActionFilter.action>(e.Row, e.OldRow) || !sender.ObjectsEqual<ActionFilter.fromDate>(e.Row, e.OldRow) || !sender.ObjectsEqual<ActionFilter.toDate>(e.Row, e.OldRow);

        }

        protected virtual void ActionFilter_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            ActionFilter filter = Filter.Current;
            if (filter != null)
            {
                string actionID = filter.Action;

                TasksToProcess.SetProcessDelegate<ProjectTaskEntry>(delegate (ProjectTaskEntry graph, PMTask task)
                {
                    if (actionID == "BILL")
                    {
                        graph.GetExtension<MAKLProjectTaskEntry_Extension>().BillTask(task, filter.FromDate ?? DateTime.Now, filter.ToDate?.AddDays(1).AddTicks(-1) ?? DateTime.Now);
                    }

                });
            }
        }


        public class WorkdayCount
        {
            public int MonCount { get; private set; }
            public int TueCount { get; private set; }
            public int WedCount { get; private set; }
            public int ThuCount { get; private set; }
            public int FriCount { get; private set; }


            public WorkdayCount(PXGraph graph, DateTime from, DateTime to)
            {
                for (var day = from; day < to; day = day.AddDays(1))
                {
                    switch (day.DayOfWeek)
                    {
                        case DayOfWeek.Monday:
                            MonCount += 1;
                            break;
                        case DayOfWeek.Tuesday:
                            TueCount += 1;
                            break;
                        case DayOfWeek.Wednesday:
                            WedCount += 1;
                            break;
                        case DayOfWeek.Thursday:
                            ThuCount += 1;
                            break;
                        case DayOfWeek.Friday:
                            FriCount += 1;
                            break;
                    }
                }
            }
        }  

        public WorkdayCount GetWorkdayCount(PXGraph graph, DateTime from, DateTime to)
        {
            return new WorkdayCount(graph, from, to);
        }

        [Serializable]
        public class ActionFilter : IBqlTable
        {
            #region Action
            public abstract class action : PX.Data.BQL.BqlString.Field<action> { }
            protected string _Action;

            [PXDBString(4)]
            [PXUIField(DisplayName = "Action")]
            [PXStringList
               (
                  new[] { "UNDF", "BILL" },
                  new[] { "<SELECT>", "BILL" }
               )]
            [PXDefault("UNDF")]
            public virtual string Action
            {
                get
                {
                    return this._Action;
                }
                set
                {
                    this._Action = value;
                }
            }
            #endregion

            #region StartDate
            [PXDate]
            [PXUIField(DisplayName = "From Date")]
            [PXDefault(typeof(Current<AccessInfo.businessDate>), PersistingCheck = PXPersistingCheck.Nothing)]
            public virtual DateTime? FromDate { get; set; }
            public abstract class fromDate : PX.Data.BQL.BqlDateTime.Field<fromDate>
            {
            }
            #endregion

            #region EndDate
            [PXDate]
            [PXUIField(DisplayName = "To Date")]
            [PXDefault(typeof(Current<AccessInfo.businessDate>), PersistingCheck = PXPersistingCheck.Nothing)]
            public virtual DateTime? ToDate { get; set; }
            public abstract class toDate : PX.Data.BQL.BqlDateTime.Field<toDate>
            {
            }
            #endregion            
        }
    }
}
