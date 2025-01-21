using Shared.Exceptions;

namespace Domain.Business
{
    public class LoanCalculator
    {
        public decimal MonthlyInstallment { get; private set; }
        public decimal TotalCost { get; private set; }

        public void CalculateSimulation(decimal loanAmount, int installments, decimal monthlyRate)
        {
            if (loanAmount <= 0) throw new ArgumentException(ErrorMessages.InvalidLoanAmount);
            if (installments <= 0) throw new ArgumentException(ErrorMessages.InvalidInstallments);
            if (monthlyRate <= 0) throw new ArgumentException(ErrorMessages.InvalidMonthlyRate);

            double loanAmountDouble = (double)loanAmount;
            double rate = (double)monthlyRate;

            MonthlyInstallment = (decimal)(loanAmountDouble * rate) /
                                 (decimal)(1 - Math.Pow(1 + rate, -installments));

            TotalCost = MonthlyInstallment * installments;
        }

    }
}
