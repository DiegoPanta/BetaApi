namespace Shared.Exceptions
{
    public static class ErrorMessages
    {
        public static string MissingSQSQueueUrl => "A URL da fila SQS está ausente na configuração.";
        public static string MissingLoanSimulationApiUrl => "A URL da API de consulta de taxa está ausente na configuração.";
        public static string InvalidApiResponse => "A resposta da API é inválida ou não pôde ser deserializada.";
        public static string GeneralError => "Erro ao processar os dados da simulação de empréstimo:";
        public static string ErrorSendingToSQS => "Erro ao enviar a mensagem para a fila SQS:";
        public static string InvalidLoanAmount => "O valor do empréstimo deve ser maior que zero.";
        public static string InvalidInstallments => "O número de parcelas deve ser maior que zero.";
        public static string InvalidMonthlyRate => "A taxa de juros mensal deve ser maior que zero.";
        public static string NoBankRatesFound => "Nenhuma taxa de banco foi encontrada.";
        public static string AnnualInterestRateMustBePositive => "The annual interest rate must be positive";
        public static string SimulationNotFound => "Simulação não encontrada.";
        public static string SimulationRequired => "A simulação não pode ser nula.";
        public static string SimulationError => "Erro ao excluir a simulação.";
        public static string MonthlyRateNotFound => "Monthly Rate not found.";
        public static string ErrorSendSQS => "Erro ao enviar mensagem para SQS.";
    }
}
