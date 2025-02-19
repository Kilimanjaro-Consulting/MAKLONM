using System;
using System.Linq;
using PX.Common;
using PX.Data;
using System.Collections;
using PX.Objects.CS;
using PX.Objects.CM.Extensions;
using PX.Objects.AR;
using PX.Objects.IN;
using PX.Objects.IN.Attributes;
using PX.Objects.TX;
using PX.Objects.SO;
using System.Collections.Generic;
using PX.Objects.GL;
using PX.Objects.Extensions.SalesPrice;
using PX.Objects.Extensions.Discount;
using PX.Objects.Extensions.SalesTax;
using Autofac;
using System.Web.Compilation;
using PX.Objects.Common.Discount;
using PX.Objects.EP;
using PX.Objects.CR.Standalone;
using PX.Objects.CR;
using PX.Objects.CT;
using PX.Objects.Extensions.MultiCurrency;
using PX.Api.Models;
using PX.Objects.CR.Extensions;
using PX.Objects.CR.Extensions.CROpportunityContactAddress;
using static PX.Objects.Common.Discount.DiscountEngine;
using static PX.Objects.PM.ProjectEntry;
using PX.Objects;
using PX.Objects.PM;

namespace PX.Objects.PM
{
  public class PMQuoteMaint_Extension : PXGraphExtension<PX.Objects.PM.PMQuoteMaint>
  {
    #region Event Handlers

    #endregion
  }
}