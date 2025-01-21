namespace Infrastructure.Persistence
{
    public class ApiResponse
    {
        public List<LoanData> Value { get; set; }
    }

    public class LoanData
    {
        public string? InstituicaoFinanceira { get; set; }
        public decimal TaxaJurosAoMes { get; set; }
        public decimal TaxaJurosAoAno { get; set; }
        //a api retorna os 8 primeiros digitos do cnpj
        public string? Cnpj8 { get; set; }
        public string? AnoMes { get; set; }
    }
}
