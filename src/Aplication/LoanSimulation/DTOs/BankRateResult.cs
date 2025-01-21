namespace Aplication.LoanSimulation.DTOs
{
    public class BankRateResult
    {
        //a api retorna os 8 primeiros digitos do cnpj
        public string? Cnpj { get; set; }
        public string? BankName { get; set; }
        public decimal MonthlyRate { get; set; }
        public decimal AnnualRate { get; set; }
    }
}
