using Microsoft.Azure.Documents;
using System;

namespace NCS.DSS.ChangeFeedSqlProcessor.Models
{
    public class ChangeFeedMessageModel
    {
        public Document Document { get; set; }
        public bool IsAction { get; set; }
        public bool IsActionPlan { get; set; }
        public bool IsAddress { get; set; }
        public bool IsAdviserDetail { get; set; }
        public bool IsContact { get; set; }
        public bool IsCustomer { get; set; }
        public bool IsDiversity { get; set; }
        public bool IsGoal { get; set; }
        public bool IsInteraction { get; set; }
        public bool IsOutcome { get; set; }
        public bool IsSession { get; set; }
        public bool IsTransfer { get; set; }
        public bool IsWebChat { get; set; }
    }
}
