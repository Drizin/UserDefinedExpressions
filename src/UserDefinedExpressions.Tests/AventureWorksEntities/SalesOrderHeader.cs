using System;
using System.Collections.Generic;
using System.Text;

namespace UserDefinedExpressions.Tests.AventureWorksEntities
{
    public class SalesOrderHeader
    {
        #region Members
        public int SalesOrderId { get; set; }
        public string AccountNumber { get; set; }
        public int BillToAddressId { get; set; }
        public string Comment { get; set; }
        public string CreditCardApprovalCode { get; set; }
        public int? CreditCardId { get; set; }
        public int? CurrencyRateId { get; set; }
        //public int CustomerId { get; set; }
        public Customer Customer { get; set; }
        public DateTime DueDate { get; set; }
        public decimal Freight { get; set; }
        public DateTime ModifiedDate { get; set; }
        public bool OnlineOrderFlag { get; set; }
        public DateTime OrderDate { get; set; }
        public string PurchaseOrderNumber { get; set; }
        public byte RevisionNumber { get; set; }
        public Guid Rowguid { get; set; }
        public string SalesOrderNumber { get; set; }
        public int? SalesPersonId { get; set; }
        public DateTime? ShipDate { get; set; }
        public int ShipMethodId { get; set; }
        public int ShipToAddressId { get; set; }
        public byte Status { get; set; }
        public decimal SubTotal { get; set; }
        public decimal TaxAmt { get; set; }
        public int? TerritoryId { get; set; }
        public decimal TotalDue { get; set; }
        public List<SalesOrderDetail> SalesOrderDetails { get; set; } = new List<SalesOrderDetail>();
        #endregion Members
    }
}
