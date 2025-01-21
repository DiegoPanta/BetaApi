namespace Domain.Entities
{
    public class LoanSimulationEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public decimal LoanAmount { get; set; }
        public int Installments { get; set; }
        public decimal MonthlyInstallment { get; set; }
        public decimal TotalCost { get; set; }
        public DateTime SimulationDate { get; set; } = DateTime.UtcNow;
    }
}
