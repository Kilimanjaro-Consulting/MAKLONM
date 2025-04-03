using PX.Data;
using MAKLONM;
using System.Collections;
using PX.Common;
using System;
using PX.Objects.EP;
using System.Linq;
using static PX.Objects.CR.CRActivity.FK;
using System.Collections.Generic;
using static PX.SM.EMailAccount;
using PX.CS;
using PX.Objects.Common.Extensions;
using PX.SM;
using static PX.Objects.CR.ContactTypesAttribute;

namespace PX.Objects.CR
{
    public class MAKLLeadMaint_Extension : PXGraphExtension<PX.Objects.CR.LeadMaint>
    {
        [PXImport]
        public PXSelect<MAKLLeadDocument, Where<MAKLLeadDocument.contactID, Equal<Current<CRLead.contactID>>>> LeadDocuments;

        public PXSelect<MAKLLeadClassActivityType, Where<MAKLLeadClassActivityType.classID, Equal<Current<CRLead.classID>>>> LeadClassActivityTypes;

        public PXSelect<MAKLLeadHistory, Where<MAKLLeadHistory.contactID, Equal<Current<CRLead.contactID>>>> LeadHistory;

        public PXAction<CRLead> generateActivities;
        [PXMassAction]
        [PXUIField(DisplayName = "Generate Activities", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton(Category = PX.Objects.CS.ActionCategories.Other)]

        

        public IEnumerable GenerateActivities(PXAdapter adapter)
        {
            if (!(adapter.View.Cache.Current is CRLead row))
                return adapter.Get();

            //Base.Actions.PressSave();
            PXLongOperation.StartOperation(Base, () =>
            {
                ProcessActivities();
                Base.Actions.PressSave();
            });          

            return adapter.Get();
        }

        public void ProcessActivities()
        {
            var leadClassActivities = LeadClassActivityTypes.Select().RowCast<MAKLLeadClassActivityType>();
            if (leadClassActivities.Count() > 0)
            {
                foreach (MAKLLeadClassActivityType actType in leadClassActivities)
                {
                    if (actType != null)
                    {
                        ProcessActivity(actType);
                    }
                }
            }
        }

        public void ProcessActivity(MAKLLeadClassActivityType actType)
        {
            EPActivityType activityType = PXSelect<EPActivityType, Where<EPActivityType.type, Equal<Required<EPActivityType.type>>>>.Select(Base, actType.Type);
            switch (activityType.ClassID)
            {
                case CRActivityClass.Activity:
                    CreateActivity(actType, activityType);
                    break;               

                case CRActivityClass.Task:
                    CreateTask(actType, activityType);
                    break;

                case CRActivityClass.Event:
                    CreateEvent(actType, activityType);
                    break;

                case CRActivityClass.Email:
                    CreateEmail(actType, activityType);
                    break;
            }
        }

        public void CreateTask(MAKLLeadClassActivityType actType, EPActivityType activityType)
        {
            CRTaskMaint graph = PXGraph.CreateInstance<CRTaskMaint>();           

            CRActivity act = new CRActivity();
            act = graph.Tasks.Insert(act);
            UpdateBaseActivity(act, actType, activityType);
            graph.Tasks.Update(act);
            graph.Actions.PressSave();

        }

        public void CreateEvent(MAKLLeadClassActivityType actType, EPActivityType activityType)
        {
            EPEventMaint graph = PXGraph.CreateInstance<EPEventMaint>();
            CRActivity act = new CRActivity();

            act = graph.Events.Insert(act);
            UpdateBaseActivity(act, actType, activityType);
            graph.Events.Update(act);
            graph.Actions.PressSave();

        }       

        public void CreateActivity(MAKLLeadClassActivityType actType, EPActivityType activityType)
        {
            CRActivityMaint graph = PXGraph.CreateInstance<CRActivityMaint>();

            CRActivity act = new CRActivity();
            act = graph.Activities.Insert(act);
            UpdateBaseActivity(act, actType, activityType);
            graph.Activities.Update(act);

            if (activityType.RequireTimeByDefault == true)
            {
                PMTimeActivity tAct = (PMTimeActivity)graph.TimeActivitiesOld.Cache.CreateInstance();
                tAct = graph.TimeActivitiesOld.Insert(tAct);
                tAct.TrackTime = false;                
                tAct = graph.TimeActivitiesOld.Update(tAct);
            }
            graph.Actions.PressSave();
        }

        public void UpdateBaseActivity(CRActivity act, MAKLLeadClassActivityType actType, EPActivityType activityType)
        {  
            act.Subject = String.Concat(actType.Description);
            act.ClassID = activityType.ClassID;
            act.BAccountID = Base.Lead.Current.BAccountID;
            act.RefNoteID = Base.Lead.Current.NoteID;
            act.RefNoteIDType = Base.Lead.Current.GetType().FullName;
            act.OwnerID = Base.Accessinfo.ContactID;
            act.Type = actType.Type;
           // act.UIStatus = ActivityStatusAttribute.Open;
            act.StartDate = Base.Accessinfo.BusinessDate ?? DateTime.Now;
        }

        public void CreateEmail(MAKLLeadClassActivityType actType, EPActivityType activityType)
        {
            CREmailActivityMaint graph = PXGraph.CreateInstance<CREmailActivityMaint>();
            CRSMEmail email = new CRSMEmail();

            email = graph.Message.Insert(email);

            email.MailTo = Base.Lead.Current.EMail;
            email.Subject = String.Concat(actType.Description);
            email.ClassID = CRActivityClass.Activity;
            email.BAccountID = Base.Lead.Current.BAccountID;
            email.RefNoteID = Base.Lead.Current.NoteID;
            email.RefNoteIDType = Base.Lead.Current.GetType().FullName;
            email.OwnerID = Base.Accessinfo.ContactID;
            email.Type = actType.Type;
            email.StartDate = Base.Accessinfo.BusinessDate ?? DateTime.Now;
            graph.Message.Update(email);

            graph.Actions.PressSave();
        }


        #region Event Handlers

        protected void CRLead_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            var row = (CRLead)e.Row;
            if (row != null)
            {
                generateActivities.SetEnabled(row.tstamp != null);

              //  bool enabledsq = row.Status == "L";
              //  PXUIFieldAttribute.SetEnabled<MAKLContactExt.usrDsqStage>(cache, row, enabledsq);
              //  PXUIFieldAttribute.SetEnabled<MAKLContactExt.usrDsqReason>(cache, row, enabledsq);                
            }
        }

        //public delegate void PersistDelegate();
        //[PXOverride]
        //public void Persist(PersistDelegate baseMethod)
        //{
        //    if (Base.Lead.Current != null)
        //    {
        //        if (Base.Lead.Current.tstamp == null)
        //        {
        //          //  GenerateActivities(Base);
        //        }
        //    }
        //    baseMethod();
        //}

        //protected void CRLead_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
        //{

        //    var row = (CRLead)e.Row;
        //    if (row != null)
        //    {
        //        var status = cache.GetStatus(row);
        //        if (status == PXEntryStatus.Inserted || status == PXEntryStatus.Updated)
        //        {
        //            PreviousStatus = task?.GetExtension<MYPEPMTask>().UsrMYPExWorkflowStageID;
        //        }
        //    }


        //}

        protected void CRLead_Status_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
           var row = (CRLead)e.Row;
            {
                var newLeadHistory = new MAKLLeadHistory()
                {
                    ContactID = row.ContactID,
                    PreviousStatus = e.OldValue.ToString(),
                    Status = row.Status,                    
                    Date = PXTimeZoneInfo.Now
                };
                newLeadHistory = LeadHistory.Insert(newLeadHistory);
            }
        }


            protected void CRLead_UsrDsqStage_FieldSelecting(PXCache cache, PXFieldSelectingEventArgs e)
        {

            var row = (CRLead)e.Row;
            if (row != null)
            {               
                List<string> stagevalues = new List<string>();
                List<string> stagelabels = new List<string>();               

                var attributeStages = PXSelect<CSAttributeDetail,
                         Where<CSAttributeDetail.attributeID, Equal<Required<CSAttributeDetail.attributeID>>>>.Select(Base, "DSQSTAGE");

                if (attributeStages?.Count > 0)
                {
                    foreach (CSAttributeDetail stage in attributeStages)
                    {
                        stagevalues.Add(stage.ValueID);
                        stagelabels.Add(stage.Description);
                    }                     
                }                

                PXStringListAttribute.SetList<MAKLContactExt.usrDsqStage>(cache, row, stagevalues.ToArray(), stagelabels.ToArray());
            }
        }

        protected void CRLead_UsrDsqStage_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {

            var row = (CRLead)e.Row;
            if (row != null)
            {
                MAKLContactExt leadExt = PXCache<Contact>.GetExtension<MAKLContactExt>(row);
                leadExt.UsrDsqReason = null;
            }
        }

        protected void CRLead_UsrDsqReason_FieldSelecting(PXCache cache, PXFieldSelectingEventArgs e)
        {
            var row = (CRLead)e.Row;

            if (row != null)
            {
                MAKLContactExt leadExt = PXCache<Contact>.GetExtension<MAKLContactExt>(row);

                List<string> reasonvalues = new List<string>();
                List<string> reasonlabels = new List<string>();

                if (leadExt.UsrDsqStage != null)
                {
                    //List<string> reasonvalues = new List<string>();
                    //List<string> reasonlabels = new List<string>();

                    CSAttributeDetail attributeStageValue = PXSelect<CSAttributeDetail,
                         Where<CSAttributeDetail.attributeID, Equal<Required<CSAttributeDetail.attributeID>>,
                         And<CSAttributeDetail.valueID, Equal<Required<CSAttributeDetail.valueID>>>>>.Select(Base, "DSQSTAGE", leadExt.UsrDsqStage).RowCast<CSAttributeDetail>().FirstOrDefault();

                    if (attributeStageValue != null)
                    {
                        var attributeReasons = PXSelect<CSAttributeDetail,
                                 Where<CSAttributeDetail.attributeID, Equal<Required<CSAttributeDetail.attributeID>>>>.Select(Base, attributeStageValue.ValueID);

                        if (attributeReasons?.Count > 0)
                        {
                            foreach (CSAttributeDetail reason in attributeReasons)
                            {
                                reasonvalues.Add(reason.ValueID);
                                reasonlabels.Add(reason.Description);
                            }
                        }
                    }
                    PXStringListAttribute.SetList<MAKLContactExt.usrDsqReason>(cache, row, reasonvalues.ToArray(), reasonlabels.ToArray());
                }
                else
                {
                    string attributeID = null;
                    switch (row.Resolution)
                    {
                        case "QM":
                            attributeID = "QUALIFYLD";
                            break;
                        case "AS":
                            attributeID = "ACCEPTLEAD";
                            break;
                        case "PI":
                            attributeID = "OPENLEAD";
                            break;                        
                    }

                    if (!String.IsNullOrEmpty(attributeID))
                    {
                        var attributeReasons = PXSelect<CSAttributeDetail,
                                 Where<CSAttributeDetail.attributeID, Equal<Required<CSAttributeDetail.attributeID>>>>.Select(Base, attributeID);

                        if (attributeReasons?.Count > 0)
                        {
                            foreach (CSAttributeDetail stage in attributeReasons)
                            {
                                reasonvalues.Add(stage.ValueID);
                                reasonlabels.Add(stage.Description);
                            }
                        }

                        PXStringListAttribute.SetList<MAKLContactExt.usrDsqReason>(cache, row, reasonvalues.ToArray(), reasonlabels.ToArray());
                    }
                }
            }
        }

        #endregion
    }
}