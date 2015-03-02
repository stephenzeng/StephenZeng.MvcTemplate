using System;

namespace StephenZeng.MvcTemplate.Web.Models
{
    public class ContractViewModel
    {
        public DateTime ContractDate { get; set; }
        public int ContractNumber { get; set; }
        public string ContractType { get; set; }
        public string ContractStatus { get; set; }
        public string SecurityCode { get; set; }
        public string SecurityName { get; set; }
        public decimal ContractUnits { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Consideration { get; set; }
        public decimal Brokerage { get; set; }
        public decimal BrokerageRate { get; set; }
        public decimal Gst { get; set; }
        public decimal OtherCharges { get; set; }
        public decimal Nett { get; set; }
        public DateTime SettlementDate { get; set; }
        public string ExerciseOption { get; set; }
        public string Currency { get; set; }
    }
}