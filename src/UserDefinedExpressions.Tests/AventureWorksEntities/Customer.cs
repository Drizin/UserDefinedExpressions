using System;
using System.Collections.Generic;
using System.Text;

namespace UserDefinedExpressions.Tests.AventureWorksEntities
{
    public class Customer
    {
        #region Members
        //public int CustomerId { get; set; }
        public string AccountNumber { get; set; }
        public DateTime ModifiedDate { get; set; }
        public int? PersonId { get; set; }
        public Guid Rowguid { get; set; }
        public int? StoreId { get; set; }
        public int? TerritoryId { get; set; }
        #endregion Members
    }
}
