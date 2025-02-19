using PX.Data;
using PX.Objects.AR;
using PX.Objects.CS;
using PX.Objects.IN;

namespace PX.Objects.PM
{
  public class MAKLPMQuoteTaskExt : PXCacheExtension<PX.Objects.PM.PMQuoteTask>
  {
    #region UsrMon
    [PXDBBool]
    [PXUIField(DisplayName="Mon")]

    public virtual bool? UsrMon { get; set; }
    public abstract class usrMon : PX.Data.BQL.BqlBool.Field<usrMon> { }
    #endregion

    #region UsrTue
    [PXDBBool]
    [PXUIField(DisplayName="Tue")]

    public virtual bool? UsrTue { get; set; }
    public abstract class usrTue : PX.Data.BQL.BqlBool.Field<usrTue> { }
    #endregion

    #region UsrWed
    [PXDBBool]
    [PXUIField(DisplayName="Wed")]

    public virtual bool? UsrWed { get; set; }
    public abstract class usrWed : PX.Data.BQL.BqlBool.Field<usrWed> { }
    #endregion

    #region UsrThu
    [PXDBBool]
    [PXUIField(DisplayName="Thu")]

    public virtual bool? UsrThu { get; set; }
    public abstract class usrThu : PX.Data.BQL.BqlBool.Field<usrThu> { }
    #endregion

    #region UsrFri
    [PXDBBool]
    [PXUIField(DisplayName="Fri")]

    public virtual bool? UsrFri { get; set; }
    public abstract class usrFri : PX.Data.BQL.BqlBool.Field<usrFri> { }
        #endregion

   #region UsrBillTo
   [CustomerActive(DescriptionField = typeof(Customer.acctName))]
   [PXUIField(DisplayName = "Billing Customer")] 
    public virtual int? UsrBillTo { get; set; }
    public abstract class usrBillTo : PX.Data.BQL.BqlInt.Field<usrBillTo> { }
    #endregion

    #region UsrItemClass
    [PXDBInt]        
    [PXUIField(DisplayName = "Item Class", Visibility = PXUIVisibility.SelectorVisible)]
    [PXDimensionSelector(INItemClass.Dimension, typeof(Search<INItemClass.itemClassID>), typeof(INItemClass.itemClassCD), DescriptionField = typeof(INItemClass.descr), CacheGlobal = true)]
    public virtual int? UsrItemClass { get; set; }
    public abstract class usrItemClass : PX.Data.BQL.BqlInt.Field<usrItemClass> { }
        #endregion

    #region UsrKitInventoryID
    [Inventory(Visibility = PXUIVisibility.SelectorVisible, DisplayName = "Kit Inventory ID")]
    [PXRestrictor(typeof(Where<InventoryItem.kitItem, Equal<boolTrue>>), PX.Objects.IN.Messages.InventoryItemIsNotaKit)]
    public virtual int? UsrKitInventoryID { get; set; }
    public abstract class usrKitInventoryID : PX.Data.BQL.BqlInt.Field<usrKitInventoryID> { }
    #endregion
  }
}