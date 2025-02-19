using PX.Data;
using System;
using System.Collections.Generic;
using PX.Objects;
using PX.Objects.CR;
using MAKLONM;

namespace PX.Objects.CR
{
  public class MAKLCRLeadClassMaint_Extension : PXGraphExtension<PX.Objects.CR.CRLeadClassMaint>
  {
    [PXImport]
    public PXSelect<MAKLLeadClassActivityType, Where<MAKLLeadClassActivityType.classID, Equal<Current<CRLeadClass.classID>>>> ActivityTypes;

     #region Event Handlers

        protected void CRLeadClass_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            var row = (CRLeadClass)e.Row;
        }   

    #endregion
  }
}