using PX.Data;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.SM;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace MAKLONM
{    

    [PXOverride]
    //public class MAKLLeadEmailProcessor : NewLeadEmailProcessor
    public class MAKLLeadEmailProcessor : BasicEmailProcessor
    {
        public PXSelectJoin<CRSMEmail,
            InnerJoin<CRMassMailMessage,
                On<CRMassMailMessage.messageID, Equal<CRSMEmail.imcUID>>>,
            Where<CRMassMailMessage.massMailID, Equal<Required<CRMassMail.massMailID>>>> CampaignEmails;
        protected override bool Process(Package package)
        {
            var account = package.Account;
            
            MAKLEMailAccountExt accountExt = PXCache<EMailAccount>.GetExtension<MAKLEMailAccountExt>(account);

            if (account.IncomingProcessing != true || 
                accountExt.UsrCreateNewLeadEnhanced != true)
            {
                return false;
            }

            var message = package.Message;
            if (!string.IsNullOrEmpty(message.Exception)
                || message.IsIncome != true
                || message.RefNoteID != null
                || message.ClassID == CRActivityClass.EmailRouting)
                return false;

            var copy = package.Graph.Caches[typeof(CRSMEmail)].CreateCopy(message);
            try
            {

                LeadEmail parsedEmail = ParseEmailBody(message, out string emailContent);

                LeadMaint graph = PXGraph.CreateInstance<LeadMaint>();
                var dedupExt = graph.GetExtension<LeadMaint.CRDuplicateEntitiesForLeadGraphExt>();
                if (dedupExt != null)
                {
                    dedupExt.HardBlockOnly = true;
                }
                var leadCache = graph.Lead.Cache;
                var lead = (CRLead)leadCache.Insert();
                lead = PXCache<CRLead>.CreateCopy(graph.Lead.Search<CRLead.contactID>(lead.ContactID));

                lead.EMail = package.Address;
                lead.LastName = package.Description;
                lead.RefContactID = message.ContactID;

                lead.OverrideRefContact = true;

                CREmailActivityMaint.EmailAddress address = CREmailActivityMaint.ParseNames(message.MailFrom);

                lead.FirstName = address.FirstName;
                lead.LastName = string.IsNullOrEmpty(address.LastName) ? address.Email : address.LastName;                
                if (account.CreateLeadClassID != null)
                    lead.ClassID = account.CreateLeadClassID;  

                //populte from email body
                lead.FullName = parsedEmail.ParticipantFirstName + parsedEmail.ParticipantLastName;
                lead.FirstName = parsedEmail.EnquirerFirstName;
                lead.LastName = parsedEmail.EnquirerLastName;
                lead.Salutation = parsedEmail.EnquirerRelationshipToParticipant;
                lead.Phone1 = parsedEmail.EnquirerMobileNumber;
                lead.EMail = parsedEmail.EnquirerEmailAddress ?? package.Address;

                lead = (CRLead)leadCache.Update(lead);

                if (lead.ClassID != null)
                {
                    CRLeadClass cls = PXSelect<
                            CRLeadClass,
                        Where<
                            CRLeadClass.classID, Equal<Required<CRLeadClass.classID>>>>
                        .SelectSingleBound(graph, null, lead.ClassID);

                    if (cls?.DefaultOwner == CRDefaultOwnerAttribute.Source)
                    {
                        lead.WorkgroupID = message.WorkgroupID;
                        lead.OwnerID = message.OwnerID;
                    }                   

                    //AGE
                    if (!String.IsNullOrEmpty(parsedEmail.Age))
                    {                       
                        CSAnswers ageAtr = PXSelect<
                            CSAnswers, 
                            Where<CSAnswers.attributeID, Equal<Required<CSAnswers.attributeID>>,
                                And<CSAnswers.refNoteID, Equal<Required<CSAnswers.refNoteID>>>>>
                            .Select(graph, "AGE", lead.NoteID);                       
                        ageAtr.Value = parsedEmail.Age;
                        graph.Answers.Cache.Update(ageAtr);
                    }

                    //DOB
                    if (!String.IsNullOrEmpty(parsedEmail.DateofBirth))
                    {
                        CSAnswers dobAtr = PXSelect<
                            CSAnswers,
                            Where<CSAnswers.attributeID, Equal<Required<CSAnswers.attributeID>>,
                                And<CSAnswers.refNoteID, Equal<Required<CSAnswers.refNoteID>>>>>
                            .Select(graph, "DOB", lead.NoteID);                        
                        dobAtr.Value = parsedEmail.DateofBirth;
                        graph.Answers.Cache.Update(dobAtr);
                    }

                    //FUNDTYPE
                    if (!String.IsNullOrEmpty(parsedEmail.FundingType))
                    {
                        CSAnswers fundtypeAtr = PXSelect<
                            CSAnswers,
                            Where<CSAnswers.attributeID, Equal<Required<CSAnswers.attributeID>>,
                                And<CSAnswers.refNoteID, Equal<Required<CSAnswers.refNoteID>>>>>
                            .Select(graph, "FUNDTYPE", lead.NoteID); 
                        fundtypeAtr.Value = parsedEmail.FundingType;
                        graph.Answers.Cache.Update(fundtypeAtr);
                    }

                    //INTSERVICE
                    if (!String.IsNullOrEmpty(parsedEmail.InterestedServices))
                    {
                        CSAnswers intservAtr = PXSelect<
                            CSAnswers,
                            Where<CSAnswers.attributeID, Equal<Required<CSAnswers.attributeID>>,
                                And<CSAnswers.refNoteID, Equal<Required<CSAnswers.refNoteID>>>>>
                            .Select(graph, "INTSERVICE", lead.NoteID); 
                        intservAtr.Value = parsedEmail.InterestedServices;
                        graph.Answers.Cache.Update(intservAtr);
                    }

                    //INTGSADAYS
                    if (!String.IsNullOrEmpty(parsedEmail.InterestInGroupServiceAttendanceDays))
                    {
                        CSAnswers intdaysAtr = PXSelect<
                            CSAnswers,
                            Where<CSAnswers.attributeID, Equal<Required<CSAnswers.attributeID>>,
                                And<CSAnswers.refNoteID, Equal<Required<CSAnswers.refNoteID>>>>>
                            .Select(graph, "INTGSADAYS", lead.NoteID);                        
                        intdaysAtr.Value = parsedEmail.InterestInGroupServiceAttendanceDays;
                        graph.Answers.Cache.Update(intdaysAtr);
                    }

                    //INTTHERAPY
                    if (!String.IsNullOrEmpty(parsedEmail.InterestInTherapyServices))
                    {
                        CSAnswers intterAtr = PXSelect<
                            CSAnswers,
                            Where<CSAnswers.attributeID, Equal<Required<CSAnswers.attributeID>>,
                                And<CSAnswers.refNoteID, Equal<Required<CSAnswers.refNoteID>>>>>
                            .Select(graph, "INTTHERAPY", lead.NoteID);                        
                        intterAtr.Value = parsedEmail.InterestInTherapyServices;
                        graph.Answers.Cache.Update(intterAtr);
                    }

                    //RQTSITE
                    if (!String.IsNullOrEmpty(parsedEmail.RequestedSite))
                    {
                        CSAnswers rqsAtr = PXSelect<
                            CSAnswers,
                            Where<CSAnswers.attributeID, Equal<Required<CSAnswers.attributeID>>,
                                And<CSAnswers.refNoteID, Equal<Required<CSAnswers.refNoteID>>>>>
                            .Select(graph, "RQTSITE", lead.NoteID);                       
                        rqsAtr.Value = parsedEmail.RequestedSite;
                        graph.Answers.Cache.Update(rqsAtr);
                    }

                    //PRIDSABLTY
                    if (!String.IsNullOrEmpty(parsedEmail.PrimaryDisability))
                    {
                        CSAnswers disabAtr = PXSelect<
                            CSAnswers,
                            Where<CSAnswers.attributeID, Equal<Required<CSAnswers.attributeID>>,
                                And<CSAnswers.refNoteID, Equal<Required<CSAnswers.refNoteID>>>>>
                            .Select(graph, "PRIDSABLTY", lead.NoteID);                       
                        disabAtr.Value = parsedEmail.PrimaryDisability;
                        graph.Answers.Cache.Update(disabAtr);
                    }

                    //OTHDSABLTY
                    if (!String.IsNullOrEmpty(parsedEmail.OtherDisability))
                    {
                        CSAnswers othdisabAtr = PXSelect<
                            CSAnswers,
                            Where<CSAnswers.attributeID, Equal<Required<CSAnswers.attributeID>>,
                                And<CSAnswers.refNoteID, Equal<Required<CSAnswers.refNoteID>>>>>
                            .Select(graph, "OTHDSABLTY", lead.NoteID);                        
                        othdisabAtr.Value = parsedEmail.OtherDisability;
                        graph.Answers.Cache.Update(othdisabAtr);
                    }

                    //INTERPRET
                    if (!String.IsNullOrEmpty(parsedEmail.InterpreterRequired))
                    {
                        CSAnswers interpAtr = PXSelect<
                            CSAnswers,
                            Where<CSAnswers.attributeID, Equal<Required<CSAnswers.attributeID>>,
                                And<CSAnswers.refNoteID, Equal<Required<CSAnswers.refNoteID>>>>>
                            .Select(graph, "INTERPRET", lead.NoteID);                       
                        interpAtr.Value = (parsedEmail.InterpreterRequired == "Yes") ? "1" : "0";
                        graph.Answers.Cache.Update(interpAtr);
                    }

                    //ABORIGINAL
                    if (!String.IsNullOrEmpty(parsedEmail.AboriginalOrTorresStrait))
                    {
                        CSAnswers aboAtr = PXSelect<
                            CSAnswers,
                            Where<CSAnswers.attributeID, Equal<Required<CSAnswers.attributeID>>,
                                And<CSAnswers.refNoteID, Equal<Required<CSAnswers.refNoteID>>>>>
                            .Select(graph, "ABORIGINAL", lead.NoteID);                        
                        aboAtr.Value = (parsedEmail.AboriginalOrTorresStrait == "Yes") ? "1" : "0"; ;
                        graph.Answers.Cache.Update(aboAtr);
                    }

                    //CAMPAIGN
                    if (!String.IsNullOrEmpty(parsedEmail.Campaign))
                    {
                        CRCampaign campaign = PXSelect<
                            CRCampaign,
                            Where<CRCampaign.campaignName, Equal<Required<CRCampaign.campaignName>>,
                                And<CRCampaign.isActive, Equal<True>>>>
                            .Select(graph, parsedEmail.Campaign);
                       if (campaign != null)
                        {
                            lead.CampaignID = campaign.CampaignID;
                        }                       
                    }
                }

                message.RefNoteID = PXNoteAttribute.GetNoteID<CRLead.noteID>(leadCache, lead);
                PXNoteAttribute.SetNote(leadCache, lead, emailContent);
                graph.GetExtension<MAKLLeadMaint_Extension>().ProcessActivities();                
                graph.Actions.PressSave();
            }
            catch (Exception e)
            {
                package.Graph.Caches[typeof(CRSMEmail)].RestoreCopy(message, copy);
                throw new PXException(PX.Objects.CR.Messages.CreateLeadException, e is PXOuterException ? ("\r\n" + String.Join("\r\n", ((PXOuterException)e).InnerMessages)) : e.Message);
            }

            return true;
        }

        private LeadEmail ParseEmailBody(CRSMEmail message, out string parsedcontent)
        {
            LeadEmail parsed = new LeadEmail();
            string result0 = Regex.Replace(message.Body, "<.*?>", string.Empty); //generic html remove
            string result1 = Regex.Replace(result0, @"<[^>]+>|&nbsp;", "").Trim(); //remove nbsp
            string result = Regex.Replace(result1, @"\s{2,}", " "); //remove multiple spaces

            parsedcontent = result;

            string enquirerFirstNameLabel = "Enquirer First Name:";            
            string enquirerLastNameLabel = "Enquirer Last Name:";
            string enquirerRelationshipToParticipantLabel = "Enquirer Relationship to Participant:";
            string enquirerMobileNumberLabel = "Enquirer Mobile Number:";
            string enquirerEmailAddressLabel = "Enquirer Email Address:";
            string participantFirstNameLabel = "Participant First Name:";
            string participantLastNameLabel = "Participant Last Name:";
            string ageLabel = "Age:";
            string dateofBirthLabel = "Date of Birth:";
            string fundingTypeLabel = "Funding Type:";
            string interestedServicesLabel = "Interested Services:";
            string interestInGroupServiceAttendanceDaysLabel = "Interest in Group Service Attendance Days:";
            string interestInTherapyServicesLabel = "Interest in Therapy Services:";
            string requestedSiteLabel = "Requested Site:";
            string primaryDisabilityLabel = "Primary Disability:";
            string otherDisabilityLabel = "Other Disability:";
            string interpreterRequiredLabel = "Interpreter Required?:";
            string aboriginalOrTorresStraitLabel = "Aboriginal or Torres Strait:";
            string campaignLabel = "Campaign:";

            int enquirerFirstNameStart = result.IndexOf(enquirerFirstNameLabel);
            int enquirerFirstNameEnd = enquirerFirstNameStart + enquirerFirstNameLabel.Length;
            int totallength = result.Length;

            int enquirerLastNameStart = result.IndexOf(enquirerLastNameLabel);
            int enquirerLastNameEnd = enquirerLastNameStart + enquirerLastNameLabel.Length;

            int enquirerRelationshipToParticipantStart = result.IndexOf(enquirerRelationshipToParticipantLabel);
            int enquirerRelationshipToParticipantEnd = enquirerRelationshipToParticipantStart + enquirerRelationshipToParticipantLabel.Length;

            int enquirerMobileNumberStart = result.IndexOf(enquirerMobileNumberLabel);
            int enquirerMobileNumberEnd = enquirerMobileNumberStart + enquirerMobileNumberLabel.Length;

            int enquirerEmailAddressStart = result.IndexOf(enquirerEmailAddressLabel);
            int enquirerEmailAddressEnd = enquirerEmailAddressStart + enquirerEmailAddressLabel.Length;

            int participantFirstNameStart = result.IndexOf(participantFirstNameLabel);
            int participantFirstNameEnd = participantFirstNameStart + participantFirstNameLabel.Length;

            int participantLastNameStart = result.IndexOf(participantLastNameLabel);
            int participantLastNameEnd = participantLastNameStart + participantLastNameLabel.Length;

            int ageStart = result.IndexOf(ageLabel);
            int ageEnd = ageStart + ageLabel.Length;

            int dateofBirthStart = result.IndexOf(dateofBirthLabel);
            int dateofBirthEnd = dateofBirthStart + dateofBirthLabel.Length;

            int fundingTypeStart = result.IndexOf(fundingTypeLabel);
            int fundingTypeEnd = fundingTypeStart + fundingTypeLabel.Length;

            int interestedServicesStart = result.IndexOf(interestedServicesLabel);
            int interestedServicesEnd = interestedServicesStart + interestedServicesLabel.Length;

            int interestInGroupServiceAttendanceDaysStart = result.IndexOf(interestInGroupServiceAttendanceDaysLabel);
            int interestInGroupServiceAttendanceDaysEnd = interestInGroupServiceAttendanceDaysStart + interestInGroupServiceAttendanceDaysLabel.Length;

            int interestInTherapyServicesStart = result.IndexOf(interestInTherapyServicesLabel);
            int interestInTherapyServicesEnd = interestInTherapyServicesStart + interestInTherapyServicesLabel.Length;

            int requestedSiteStart = result.IndexOf(requestedSiteLabel);
            int requestedSiteEnd = requestedSiteStart + requestedSiteLabel.Length;

            int primaryDisabilityStart = result.IndexOf(primaryDisabilityLabel);
            int primaryDisabilityEnd = primaryDisabilityStart + primaryDisabilityLabel.Length;

            int otherDisabilityStart = result.IndexOf(otherDisabilityLabel);
            int otherDisabilityEnd = otherDisabilityStart + otherDisabilityLabel.Length;

            int interpreterRequiredStart = result.IndexOf(interpreterRequiredLabel);
            int interpreterRequiredEnd = interpreterRequiredStart + interpreterRequiredLabel.Length;

            int aboriginalOrTorresStraitStart = result.IndexOf(aboriginalOrTorresStraitLabel);
            int aboriginalOrTorresStraitEnd = aboriginalOrTorresStraitStart + aboriginalOrTorresStraitLabel.Length;

            int campaignStart = result.IndexOf(campaignLabel);
            int campaignEnd = campaignStart + campaignLabel.Length;


            parsed.EnquirerFirstName = result.Substring(enquirerFirstNameEnd, (enquirerLastNameStart - enquirerFirstNameEnd)).Trim();
            parsed.EnquirerLastName = result.Substring(enquirerLastNameEnd, (enquirerRelationshipToParticipantStart - enquirerLastNameEnd)).Trim();
            parsed.EnquirerRelationshipToParticipant = result.Substring(enquirerRelationshipToParticipantEnd, (enquirerMobileNumberStart - enquirerRelationshipToParticipantEnd)).Trim();
            parsed.EnquirerMobileNumber = result.Substring(enquirerMobileNumberEnd, (enquirerEmailAddressStart - enquirerMobileNumberEnd)).Trim();
            parsed.EnquirerEmailAddress = result.Substring(enquirerEmailAddressEnd, (participantFirstNameStart - enquirerEmailAddressEnd)).Trim();
            parsed.ParticipantFirstName = result.Substring(participantFirstNameEnd, (participantLastNameStart - participantFirstNameEnd)).Trim();
            parsed.ParticipantLastName = result.Substring(participantLastNameEnd, (ageStart - participantLastNameEnd)).Trim();
            parsed.Age = result.Substring(ageEnd, (dateofBirthStart - ageEnd)).Trim();

            var datevalue = DateTime.Parse(result.Substring(dateofBirthEnd, (fundingTypeStart - dateofBirthEnd)).Trim());
            parsed.DateofBirth = datevalue.ToString("yyyy-MM-dd");

            parsed.FundingType = result.Substring(fundingTypeEnd, (interestedServicesStart - fundingTypeEnd)).Trim();
            parsed.InterestedServices = result.Substring(interestedServicesEnd, (interestInGroupServiceAttendanceDaysStart - interestedServicesEnd)).Trim();
            parsed.InterestInGroupServiceAttendanceDays = result.Substring(interestInGroupServiceAttendanceDaysEnd, (interestInTherapyServicesStart - interestInGroupServiceAttendanceDaysEnd)).Trim();
            parsed.InterestInTherapyServices = result.Substring(interestInTherapyServicesEnd, (requestedSiteStart - interestInTherapyServicesEnd)).Trim();
            parsed.RequestedSite = result.Substring(requestedSiteEnd, (primaryDisabilityStart - requestedSiteEnd)).Trim();
            parsed.PrimaryDisability = result.Substring(primaryDisabilityEnd, (otherDisabilityStart - primaryDisabilityEnd)).Trim();
            parsed.OtherDisability = result.Substring(otherDisabilityEnd, (interpreterRequiredStart - otherDisabilityEnd)).Trim();
            parsed.InterpreterRequired = result.Substring(interpreterRequiredEnd, (aboriginalOrTorresStraitStart - interpreterRequiredEnd)).Trim();
            if (campaignStart > 0)
            {
                parsed.AboriginalOrTorresStrait = result.Substring(aboriginalOrTorresStraitEnd, (campaignStart - aboriginalOrTorresStraitEnd)).Trim();
                parsed.Campaign = result.Substring(campaignEnd, (totallength - campaignEnd)).Trim();
            }
            else
            {
                parsed.AboriginalOrTorresStrait = result.Substring(aboriginalOrTorresStraitEnd, (totallength - aboriginalOrTorresStraitEnd)).Trim();
            }           

            return parsed;
        }

        public class LeadEmail
        { 
            public string EnquirerFirstName { get; set; }
            public string EnquirerLastName { get; set; }
            public string EnquirerRelationshipToParticipant { get; set; }
            public string EnquirerMobileNumber { get; set; }
            public string EnquirerEmailAddress { get; set; }
            public string ParticipantFirstName { get; set; }
            public string ParticipantLastName { get; set; }
            public string Age { get; set; }
            public string DateofBirth { get; set; }
            public string FundingType { get; set; }
            public string InterestedServices { get; set; }
            public string InterestInGroupServiceAttendanceDays { get; set; }
            public string InterestInTherapyServices { get; set; }
            public string RequestedSite { get; set; }
            public string PrimaryDisability { get; set; }
            public string OtherDisability { get; set; }
            public string InterpreterRequired { get; set; }
            public string AboriginalOrTorresStrait { get; set; }
            public string Campaign { get; set; }
        }
    }
}
