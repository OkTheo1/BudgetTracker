using Microsoft.EntityFrameworkCore;
using MoneyTracker2026.Data;
using MoneyTracker2026.Models;

namespace MoneyTracker2026.Services;

public class DebtPayoffPlan
{
    public string DebtName { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public decimal InterestRate { get; set; }
    public decimal MinPayment { get; set; }
    public int MonthsToPayoff { get; set; }
    public decimal TotalInterestPaid { get; set; }
    public decimal MonthlyPayment { get; set; }
}

public class DebtComparisonResult
{
    public List<DebtPayoffPlan> SnowballPlan { get; set; } = new();
    public List<DebtPayoffPlan> AvalanchePlan { get; set; } = new();
    public int SnowballTotalMonths { get; set; }
    public int AvalancheTotalMonths { get; set; }
    public decimal SnowballTotalInterest { get; set; }
    public decimal AvalancheTotalInterest { get; set; }
    public decimal InterestSavings { get; set; }
    public string RecommendedMethod { get; set; } = string.Empty;
}

public class DebtService
{
    private readonly Func<AppDbContext> _contextFactory;
    private readonly SettingsService? _settingsService;

    public DebtService(Func<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public DebtService(Func<AppDbContext> contextFactory, SettingsService settingsService)
    {
        _contextFactory = contextFactory;
        _settingsService = settingsService;
    }

    public async Task<List<Debt>> GetAllDebtsAsync()
    {
        using var context = _contextFactory();
        return await context.Debts.ToListAsync();
    }

    public async Task<Debt> AddDebtAsync(Debt debt)
    {
        using var context = _contextFactory();
        context.Debts.Add(debt);
        await context.SaveChangesAsync();
        return debt;
    }

    public async Task<Debt> UpdateDebtAsync(Debt debt)
    {
        using var context = _contextFactory();
        context.Debts.Update(debt);
        await context.SaveChangesAsync();
        return debt;
    }

    public async Task DeleteDebtAsync(int debtId)
    {
        using var context = _contextFactory();
        var debt = await context.Debts.FindAsync(debtId);
        if (debt != null)
        {
            context.Debts.Remove(debt);
            await context.SaveChangesAsync();
        }
    }

    public async Task<decimal> GetTotalDebtAsync()
    {
        using var context = _contextFactory();
        return await context.Debts.SumAsync(d => d.Balance);
    }

    public DebtComparisonResult CalculatePayoffPlans(decimal extraMonthlyPayment = 0)
    {
        List<Debt> debts;
        using (var context = _contextFactory())
        {
            debts = context.Debts.ToList();
        }

        if (debts.Count == 0)
        {
            return new DebtComparisonResult();
        }

        // Calculate Snowball (lowest balance first)
        var snowballDebts = debts.OrderBy(d => d.Balance).ToList();
        var snowballPlan = CalculateDebtPayoff(snowballDebts, extraMonthlyPayment);
        
        // Calculate Avalanche (highest interest first)
        var avalancheDebts = debts.OrderByDescending(d => d.InterestRatePercent).ToList();
        var avalanchePlan = CalculateDebtPayoff(avalancheDebts, extraMonthlyPayment);

        var snowballTotalInterest = snowballPlan.Sum(p => p.TotalInterestPaid);
        var avalancheTotalInterest = avalanchePlan.Sum(p => p.TotalInterestPaid);

        var result = new DebtComparisonResult
        {
            SnowballPlan = snowballPlan,
            AvalanchePlan = avalanchePlan,
            SnowballTotalMonths = snowballPlan.Max(p => p.MonthsToPayoff),
            AvalancheTotalMonths = avalanchePlan.Max(p => p.MonthsToPayoff),
            SnowballTotalInterest = snowballTotalInterest,
            AvalancheTotalInterest = avalancheTotalInterest,
            InterestSavings = snowballTotalInterest - avalancheTotalInterest,
            RecommendedMethod = avalancheTotalInterest < snowballTotalInterest ? "Avalanche" : "Snowball"
        };

        return result;
    }

    private List<DebtPayoffPlan> CalculateDebtPayoff(List<Debt> debts, decimal extraPayment)
    {
        var plan = new List<DebtPayoffPlan>();

        if (debts.Count == 0)
            return plan;

        // Create working copies
        var workingDebts = debts.Select(d => new DebtWorkingCopy
        {
            Name = d.Name,
            Balance = d.Balance,
            InterestRatePercent = d.InterestRatePercent,
            MinPayment = d.MinPayment,
            TotalInterestPaid = 0,
            MonthsToPayoff = 0
        }).ToList();

        decimal minTotalPayment = workingDebts.Sum(d => d.MinPayment);
        decimal totalPayment = minTotalPayment + extraPayment;

        int maxMonths = 360;
        int currentMonth = 0;

        while (workingDebts.Any(d => d.Balance > 0) && currentMonth < maxMonths)
        {
            currentMonth++;

            // Apply interest and payments
            foreach (var debt in workingDebts.Where(d => d.Balance > 0))
            {
                // Calculate monthly interest
                decimal monthlyRate = debt.InterestRatePercent / 100 / 12;
                decimal interest = debt.Balance * monthlyRate;
                debt.TotalInterestPaid += interest;
                debt.Balance += interest;

                // Apply minimum payment
                decimal payment = Math.Min(debt.MinPayment, debt.Balance);
                debt.Balance -= payment;
            }

            // Apply extra payment to first debt with balance
            var targetDebt = workingDebts.FirstOrDefault(d => d.Balance > 0);
            if (targetDebt != null && extraPayment > 0)
            {
                decimal extraToApply = Math.Min(extraPayment, targetDebt.Balance);
                targetDebt.Balance -= extraToApply;
            }

            // Update months for debts being paid off
            foreach (var debt in workingDebts.Where(d => d.Balance > 0 && d.MonthsToPayoff == 0))
            {
                debt.MonthsToPayoff = currentMonth;
            }
        }

        // Build the plan
        foreach (var debt in workingDebts)
        {
            plan.Add(new DebtPayoffPlan
            {
                DebtName = debt.Name,
                Balance = debt.Balance,
                InterestRate = debt.InterestRatePercent,
                MinPayment = debt.MinPayment,
                MonthsToPayoff = debt.MonthsToPayoff > 0 ? debt.MonthsToPayoff : currentMonth,
                TotalInterestPaid = debt.TotalInterestPaid,
                MonthlyPayment = debt.MinPayment + (debt == workingDebts.First() ? extraPayment : 0)
            });
        }

        return plan;
    }

    private class DebtWorkingCopy
    {
        public string Name { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public decimal InterestRatePercent { get; set; }
        public decimal MinPayment { get; set; }
        public decimal TotalInterestPaid { get; set; }
        public int MonthsToPayoff { get; set; }
    }

    public async Task<DebtComparisonResult> GetPayoffComparisonAsync(decimal extraMonthlyPayment = 0)
    {
        return await Task.Run(() => CalculatePayoffPlans(extraMonthlyPayment));
    }
}
