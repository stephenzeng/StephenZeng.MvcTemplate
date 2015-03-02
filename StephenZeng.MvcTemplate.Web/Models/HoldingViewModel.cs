namespace StephenZeng.MvcTemplate.Web.Models
{
    public class HoldingViewModel
    {
        public string AccountNumber { get; set; }
        public string SecurityCode { get; set; }
        public string SecurityName { get; set; }
        public string Market { get; set; }
        public decimal TotalHolding { get; set; }
        public decimal Percentage { get; set; }
        public decimal AverageCost { get; set; }
        public decimal LastClosePrice { get; set; }
        public decimal TotalCost { get; set; }
        public decimal MarketValue { get; set; }
        public decimal UnrealisedPl { get { return MarketValue - TotalCost; }}
        public decimal ChessHolding { get; set; }
        public decimal ChessRegistered { get; set; }
        public decimal CollaberalTakeover { get; set; }
        public decimal NomineeHolding { get; set; }
        public decimal NomineeRegistered { get; set; }
    }
}