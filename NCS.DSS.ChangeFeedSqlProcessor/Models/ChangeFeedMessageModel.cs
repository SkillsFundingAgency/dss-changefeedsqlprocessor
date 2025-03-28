﻿namespace NCS.DSS.ChangeFeedSqlProcessor.Models
{
    public class ChangeFeedMessageModel
    {
        public bool IsAction { get; set; }
        public bool IsActionPlan { get; set; }
        public bool IsAddress { get; set; }
        public bool IsAdviserDetail { get; set; }
        public bool IsCollection { get; set; }
        public bool IsContact { get; set; }
        public bool IsCustomer { get; set; }
        public bool IsDiversity { get; set; }
        public bool IsEmploymentProgression { get; set; }
        public bool IsGoal { get; set; }
        public bool IsInteraction { get; set; }
        public bool IsLearningProgression { get; set; }
        public bool IsOutcome { get; set; }
        public bool IsSession { get; set; }
        public bool IsSubscription { get; set; }
        public bool IsTransfer { get; set; }
        public bool IsWebChat { get; set; }
        public bool IsDigitalIdentity { get; set; }
    }
}
