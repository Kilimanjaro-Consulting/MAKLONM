using PX.Objects.CR;
using PX.Data;
using PX.Objects.GL;
using PX.Objects.PM;
using PX.Objects.IN;
using PX.Objects.CM;
using System;
using System.Collections;
using System.Linq;
using PX.Objects.AR;
using static PX.SM.EMailAccount;

namespace PX.Objects.EP
{
  public class MAKLCRActivityMaint_Extension : PXGraphExtension<PX.Objects.EP.CRActivityMaint>
  {
        public PXFilter<TaskBudgetList> ProjectTaskBudget;
        public PXSelect<MAKLProjectIncBudgetTotalsPerTaskProjection> ProjectTaskBudgetAndActual;        
        #region Event Handlers   

        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXRestrictor(typeof(Where<Exists<Select<
            PMTask,
            Where<PMTask.taskID, Equal<Current<PMTimeActivity.projectTaskID>>,
                And<InventoryItem.itemClassID, Equal<MAKLPMTaskExt.usrItemClass>>>>>>), "Item must have the same item class which is assigned to Project Task", typeof(InventoryItem.inventoryCD))]
        protected virtual void PMTimeActivity_LabourItemID_CacheAttached(PXCache cache)
        {

        }

        protected virtual IEnumerable projectTaskBudget()
        {
            PMTimeActivity activity = Base.TimeActivitiesOld.Current;
            if (activity == null)
            {
                yield break;
            }
            if (activity.ProjectID == null || activity.TrackTime != true)
            {
                yield break;
            }


            bool found = false;
            foreach (TaskBudgetList item in ProjectTaskBudget.Cache.Inserted)
            {
                if (item.ProjectID == activity.ProjectID)
                {
                    found = true;
                    yield return item;
                }
            }
            if (found)
                yield break;
                        
            PXSelectBase<MAKLProjectIncBudgetTotalsPerTaskProjection> selectBudgetAndActual = ProjectTaskBudgetAndActual;            

            using (new PXFieldScope(selectBudgetAndActual.View,
                    typeof(MAKLProjectIncBudgetTotalsPerTaskProjection.projectID),
                    typeof(MAKLProjectIncBudgetTotalsPerTaskProjection.projectTaskID),                   
                    typeof(MAKLProjectIncBudgetTotalsPerTaskProjection.amount),
                    typeof(MAKLProjectIncBudgetTotalsPerTaskProjection.actualAmount),
                    typeof(MAKLProjectIncBudgetTotalsPerTaskProjection.amountToInvoice),
                    typeof(MAKLProjectIncBudgetTotalsPerTaskProjection.varianceAmount)))
            {
                foreach (PXResult item in selectBudgetAndActual.Select())
                {
                    var result = CreateListItem(item);

                    if (ProjectTaskBudget.Locate(result) == null)
                        yield return ProjectTaskBudget.Insert(result);
                }
            }               

            ProjectTaskBudget.Cache.IsDirty = false;
        }

        protected virtual TaskBudgetList CreateListItem(PXResult item)
        {
            MAKLProjectIncBudgetTotalsPerTaskProjection projectBudget = PXResult.Unwrap<MAKLProjectIncBudgetTotalsPerTaskProjection>(item);
           
            TaskBudgetList result = new TaskBudgetList();
            result.ProjectID = projectBudget.ProjectID;
            result.TaskID = projectBudget.ProjectTaskID;
            result.ActualAmt = projectBudget.ActualAmount;
            result.BudgetAmt = projectBudget.Amount;

            PMProject project = PXSelect<PMProject, Where<PMProject.contractID, Equal<Required<PMProject.contractID>>>>.Select(Base, projectBudget.ProjectID);

            var activities = PXSelectJoin<PMTimeActivity,
                LeftJoin<PMTran, On<PMTran.origRefID, Equal<PMTimeActivity.noteID>>>,          
                Where2<Where<PMTimeActivity.projectID, Equal<Required<PMTimeActivity.projectID>>,
                    And<PMTimeActivity.projectTaskID, Equal<Required<PMTimeActivity.projectTaskID>>>>,
                    And<Where<PMTran.tranID, IsNull,Or<Where<PMTran.billed, Equal<False>,And<PMTran.billable,Equal<True>,And<PMTran.excludedFromBilling, Equal<False>>>>>>>>>              
                .Select(Base, projectBudget.ProjectID, projectBudget.ProjectTaskID);

            decimal unbilledAmt = 0m;
            foreach (PMTimeActivity act in activities)
            {
                InventoryItem labouritem = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(Base, act.LabourItemID);
                
                if (labouritem != null)
                {
                    decimal qty = (act.TimeSpent ?? 0m) / 60m;
                    decimal price = 0m;

                    if (labouritem.KitItem == true)
                    {
                        INKitSpecHdr kitspec = PXSelect<
                            INKitSpecHdr,
                            Where<INKitSpecHdr.kitInventoryID, Equal<Required<INKitSpecHdr.kitInventoryID>>,
                            And<INKitSpecHdr.isActive, Equal<True>,
                                And<INKitSpecHdrExt.usrEffectiveDate, LessEqual<Required<INKitSpecHdrExt.usrEffectiveDate>>>>>,
                            OrderBy<Desc<INKitSpecHdrExt.usrEffectiveDate>>>
                            .Select(Base, labouritem.InventoryID, act.Date).RowCast<INKitSpecHdr>().FirstOrDefault();

                        if (kitspec != null)
                        {
                            INKitSpecHdrExt componentExt = PXCache<INKitSpecHdr>.GetExtension<INKitSpecHdrExt>(kitspec);

                            price = componentExt.UsrAmount ?? 0m;
                            qty = 1;
                        }
                        else
                        {
                            price = FindSalesPrice(Base.Activities.Cache, act, labouritem, project);
                         //   qty = act.TimeSpent ?? 0 / 60;
                        }
                    }
                    else
                    {
                        price = FindSalesPrice(Base.Activities.Cache, act, labouritem, project);
                       // qty = (act.TimeSpent ?? 0m) / 60m;
                    }

                    unbilledAmt += qty * price;
                }
            }
            result.PendingAmt = projectBudget.AmountToInvoice + unbilledAmt;
            result.VarianceAmt = projectBudget.Amount - projectBudget.ActualAmount - projectBudget.AmountToInvoice - unbilledAmt;

            return result;
        }

        public decimal FindSalesPrice(PXCache sender, PMTimeActivity activity, InventoryItem item, PMProject project)
        {
            decimal price = 0m;

            CurrencyInfo currencyInfo = PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Required<CurrencyInfo.curyInfoID>>>>.Select(Base, project.CuryInfoID);
            price = ARSalesPriceMaint.CalculateSalesPrice(sender,
                            null,
                            item.InventoryID,
                            currencyInfo,
                            item.BaseUnit,
                            activity.Date ?? DateTime.Now
                           ) ?? 0m;
            return price;
        }
               

        protected virtual void _(Events.FieldUpdated<PMTimeActivity, PMTimeActivity.projectID> e)
        {
            PMTimeActivity row = e.Row;
            if (row == null) return;
            ProjectTaskBudget.View.RequestRefresh();
        }

        [PXHidden]
        [Serializable]
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public partial class TaskBudgetList : IBqlTable
        {
            #region Selected
            public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
            protected bool? _Selected = false;
            [PXBool]
            [PXUnboundDefault(false)]
            [PXUIField(DisplayName = "Selected")]
            public bool? Selected
            {
                get
                {
                    return _Selected;
                }
                set
                {
                    _Selected = value;
                }
            }
            #endregion

            #region ProjectID
            public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
            protected int? _ProjectID;            
            [PXInt(IsKey = true)]
            public virtual int? ProjectID
            {
                get
                {
                    return this._ProjectID;
                }
                set
                {
                    this._ProjectID = value;
                }
            }
            #endregion

            #region TaskID
            public abstract class taskID : PX.Data.BQL.BqlInt.Field<taskID> { }
            protected int? _TaskID;
            [ProjectTask(typeof(projectID), IsKey = true, BqlField = typeof(PMBudget.projectTaskID))]
            //[PXDBInt(IsKey = true)]
            public virtual int? TaskID
            {
                get
                {
                    return this._TaskID;
                }
                set
                {
                    this._TaskID = value;
                }
            }


            #endregion
                        
            #region BudgetAmt
            public abstract class budgetAmt : PX.Data.BQL.BqlDecimal.Field<budgetAmt> { }
            protected decimal? _BudgetAmt;
            [PXDecimal(2)]
            [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
            [PXUIField(DisplayName = "Budget Amount", Visibility = PXUIVisibility.SelectorVisible)]
            public virtual decimal? BudgetAmt
            {
                get
                {
                    return this._BudgetAmt;
                }
                set
                {
                    this._BudgetAmt = value;
                }
            }
            #endregion

            #region ActualAmt
            public abstract class actualAmt : PX.Data.BQL.BqlDecimal.Field<actualAmt> { }
            protected decimal? _ActualAmt;
            [PXDecimal(2)]
            [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
            [PXUIField(DisplayName = "Actual Amount", Visibility = PXUIVisibility.SelectorVisible)]
            public virtual decimal? ActualAmt
            {
                get
                {
                    return this._ActualAmt;
                }
                set
                {
                    this._ActualAmt = value;
                }
            }
            #endregion

            #region PendingAmt
            public abstract class pendingAmt : PX.Data.BQL.BqlDecimal.Field<pendingAmt> { }
            protected decimal? _PendingAmt;
            [PXDecimal(2)]
            [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
            [PXUIField(DisplayName = "Pending Invoice Amount", Visibility = PXUIVisibility.SelectorVisible)]
            public virtual decimal? PendingAmt
            {
                get
                {
                    return this._PendingAmt;
                }
                set
                {
                    this._PendingAmt = value;
                }
            }
            #endregion

            #region VarianceAmt
            public abstract class varianceAmt : PX.Data.BQL.BqlDecimal.Field<varianceAmt> { }
            protected decimal? _VarianceAmt;
            [PXDecimal(2)]
            [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
            [PXUIField(DisplayName = "Variance", Visibility = PXUIVisibility.SelectorVisible)]
            public virtual decimal? VarianceAmt
            {
                get
                {
                    return this._VarianceAmt;
                }
                set
                {
                    this._VarianceAmt = value;
                }
            }
            #endregion

        }

        [System.SerializableAttribute()]
        [PXProjection(typeof(Select4<
            PMBudget,
            Where<PMBudget.projectID, Equal<CurrentValue<PMTimeActivity.projectID>>,
                And<PMBudget.type, Equal<AccountType.income>>>,
            Aggregate<
                GroupBy<PMBudget.projectTaskID,
                    Sum<PMBudget.amount,
                    Sum<PMBudget.actualAmount,
                    Sum<PMBudget.invoicedAmount,
                    Sum<PMBudget.varianceAmount>>>>>>>), Persistent = false)]
        [PXCacheName("MAKLProjectIncBudgetTotalsPerTask")]
        public partial class MAKLProjectIncBudgetTotalsPerTaskProjection : IBqlTable
        {
            #region ProjectID
           // [PXDBInt(IsKey = true, BqlField = typeof(PMBudget.projectID))]
            [Project(IsKey = true, BqlField = typeof(PMBudget.projectID))]
          //  [PXUIField(DisplayName = "Project ID")]
            public virtual int? ProjectID { get; set; }
            public abstract class projectID : IBqlField, IBqlOperand { }
            #endregion

            #region ProjectTaskID
          //  [PXDBInt(IsKey = true, BqlField = typeof(PMBudget.projectTaskID))]
            [ProjectTask(typeof(projectID), IsKey = true, BqlField = typeof(PMBudget.projectTaskID))]
           // [PXUIField(DisplayName = "Project Task ID")]
            public virtual int? ProjectTaskID { get; set; }
            public abstract class projectTaskID : IBqlField, IBqlOperand { }
            #endregion           

            #region BudgetAmt
            [PXDBDecimal(BqlField = typeof(PMBudget.amount))]
            [PXUIField(DisplayName = "Budget Amount")]
            public virtual decimal? Amount { get; set; }
            public abstract class amount : IBqlField, IBqlOperand { }
            #endregion

            #region ActualAmt
            [PXDBDecimal(BqlField = typeof(PMBudget.actualAmount))]
            [PXUIField(DisplayName = "Actual Amount")]
            public virtual decimal? ActualAmount { get; set; }
            public abstract class actualAmount : IBqlField, IBqlOperand { }
            #endregion

            #region PendingAmt
            [PXDBDecimal(BqlField = typeof(PMBudget.invoicedAmount))]
            [PXUIField(DisplayName = "Draft Invoice Amount")]
            public virtual decimal? AmountToInvoice { get; set; }
            public abstract class amountToInvoice : IBqlField, IBqlOperand { }
            #endregion

            #region VarianceAmt
            [PXDBDecimal(BqlField = typeof(PMBudget.varianceAmount))]
            [PXUIField(DisplayName = "Variance Amount")]
            public virtual decimal? VarianceAmount { get; set; }
            public abstract class varianceAmount : IBqlField, IBqlOperand { }
            #endregion

        }

       
        [EPStartDate(AllDayField = typeof(CRActivity.allDay), DisplayName = "Start Date", DisplayNameDate = "Start Date", DisplayNameTime = "Start Time")]        
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        protected virtual void _(Events.CacheAttached<CRActivity.startDate> e) { }
                
        [PXUIField(DisplayName = "End Date")]
        [PXMergeAttributes(Method = MergeMethod.Append)]               
        protected virtual void _(Events.CacheAttached<CRActivity.endDate> e) { }
    }
    #endregion
}