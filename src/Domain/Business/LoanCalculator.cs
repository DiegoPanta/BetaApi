using Shared.Exceptions;
using System;
using System.Diagnostics.Metrics;

namespace Domain.Business
{
    public class LoanCalculator
    {
        public double MonthlyInstallmentAmount { get; private set; }
        public double TotalCostMonth { get; private set; }
        public double AnnualAmount { get; private set; }
        public double FinalAmountWithAnnualInterest { get; private set; }

        public void CalculateMonthlySimulation(double loanAmount, int installments, double monthlyRate)
        {
            if (loanAmount <= 0) throw new ArgumentException(ErrorMessages.InvalidLoanAmount);
            if (installments <= 0) throw new ArgumentException(ErrorMessages.InvalidInstallments);
            if (monthlyRate <= 0) throw new ArgumentException(ErrorMessages.InvalidMonthlyRate);

            // Calculando o valor da parcela mensal
            MonthlyInstallmentAmount = (loanAmount * monthlyRate) / (1 - Math.Pow(1 + monthlyRate, -installments));
            //Calculando o valor total a ser pago com taxa de juros mensal
            TotalCostMonth = MonthlyInstallmentAmount * installments;
        }

        public double CalculateInterestRate(double interestRateDecimal)
        {
            return interestRateDecimal / 100;
        }

        public void CalculateAnnualSimulation(double loanAmount, double annualInterestRate, int months)
        {
            var monthsInAYear = 12;
            if (loanAmount <= 0) throw new ArgumentException(ErrorMessages.InvalidLoanAmount);
            if (annualInterestRate <= 0) throw new ArgumentException(ErrorMessages.AnnualInterestRateMustBePositive);

            int years = months / monthsInAYear;
            double totalInterest = loanAmount * annualInterestRate * years;
            // Cálculo do valor total a ser pago com taxa de juros anual
            FinalAmountWithAnnualInterest = loanAmount + totalInterest;

            // Cálculo do valor anual a ser pago
            AnnualAmount = FinalAmountWithAnnualInterest / years;
        }

    }
}
