using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinacialTransaction.Model
{
    public class Transaction
    {
        public string CorrelationId { get; set; }
        public string TenantId { get; set; }
        public string TransactionId { get; set; }
        public DateTime TransactionDate { get; set; }
        public string Direction { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string Description { get; set; }
        public AccountDetails SourceAccount { get; set; }
        public AccountDetails DestinationAccount { get; set; }
    }
}
