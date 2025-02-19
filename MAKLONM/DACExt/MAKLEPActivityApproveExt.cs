using PX.Data;
using PX.Objects.AR;

namespace PX.Objects.CR
{
    public class MAKLEPActivityApproveExt : PXCacheExtension<PX.Objects.EP.EPActivityApprove>
    {
        #region UsrCustomerID
        [PXString]
        [PXUIField(DisplayName = "Customer Name")]
       // [Customer(DescriptionField = typeof(Customer.acctName), Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        public virtual string UsrCustomerName { get; set; }
        public abstract class usrCustomerName : PX.Data.BQL.BqlString.Field<usrCustomerName> { }
        #endregion
    }
}