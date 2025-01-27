namespace Domain.Entities
{
    public class LoanSimulationEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public double LoanAmount { get; set; }
        public int Installments { get; set; }
        public double MonthlyInstallment { get; set; }
        public double TotalAnnualCost { get; set; }

        public double FinalCostYears { get; set; }
        public double TotalCostMonth { get; set; }
        public DateTime SimulationDate { get; set; } = DateTime.UtcNow;
    }
}
