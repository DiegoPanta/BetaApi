namespace Aplication.LoanSimulation.DTOs
{
    public class LoanSimulationResult
    {
        public Guid Id { get; set; }
        public int Installments { get; set; }
        public double LoanAmount { get; set; }
        public double MonthlyInstallment { get; set; }
        public double TotalCostMonth { get; set; }
        public double TotalAnnualCost { get; set; }
        public double FinalCostYears { get; set;}
    }
}
