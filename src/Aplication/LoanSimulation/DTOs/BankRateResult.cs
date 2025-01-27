namespace Aplication.LoanSimulation.DTOs
{
    public class BankRateResult
    {
        //a api retorna os 8 primeiros digitos do cnpj
        public string? Cnpj { get; set; }
        public string? BankName { get; set; }
        public double MonthlyRate { get; set; }
        public double AnnualRate { get; set; }
        public string? YearMonth { get; set; }
    }
}
