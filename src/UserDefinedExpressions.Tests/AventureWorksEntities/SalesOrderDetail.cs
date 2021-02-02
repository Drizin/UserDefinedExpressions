using System;
using System.Collections.Generic;
using System.Text;

namespace UserDefinedExpressions.Tests.AventureWorksEntities
{
    public class SalesOrderDetail
    {
        #region Members
        public int SalesOrderDetailId { get; set; }
        public string CarrierTrackingNumber { get; set; }
        public decimal LineTotal { get; set; }
        public DateTime ModifiedDate { get; set; }
        public short OrderQty { get; set; }
        public int ProductId { get; set; }
        public Guid Rowguid { get; set; }
        public int SpecialOfferId { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal UnitPriceDiscount { get; set; }
        #endregion Members
    }
}
