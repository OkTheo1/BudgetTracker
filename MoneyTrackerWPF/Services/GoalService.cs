using Microsoft.EntityFrameworkCore;
using MoneyTracker2026.Data;
using MoneyTracker2026.Models;

namespace MoneyTracker2026.Services;

public class GoalService
{
    private readonly Func<AppDbContext> _contextFactory;

    public GoalService(Func<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<SavingsGoal>> GetAllGoalsAsync()
    {
        using var context = _contextFactory();
        return await context.SavingsGoals.OrderBy(g => g.TargetDate).ToListAsync();
    }

    public async Task<List<SavingsGoal>> GetActiveGoalsAsync()
    {
        using var context = _contextFactory();
        return await context.SavingsGoals
            .Where(g => g.CurrentAmount < g.TargetAmount)
            .OrderBy(g => g.TargetDate)
            .ToListAsync();
    }

    public async Task<SavingsGoal> GetGoalByIdAsync(int goalId)
    {
        using var context = _contextFactory();
        return await context.SavingsGoals.FindAsync(goalId) ?? new SavingsGoal();
    }

    public async Task<SavingsGoal> CreateGoalAsync(SavingsGoal goal)
    {
        using var context = _contextFactory();
        context.SavingsGoals.Add(goal);
        await context.SaveChangesAsync();
        return goal;
    }

    public async Task<SavingsGoal> UpdateGoalAsync(SavingsGoal goal)
    {
        using var context = _contextFactory();
        context.SavingsGoals.Update(goal);
        await context.SaveChangesAsync();
        return goal;
    }

    public async Task UpdateGoalProgressAsync(int goalId, decimal additionalAmount)
    {
        using var context = _contextFactory();
        var goal = await context.SavingsGoals.FindAsync(goalId);
        if (goal != null)
        {
            goal.CurrentAmount += additionalAmount;
            await context.SaveChangesAsync();
        }
    }

    public async Task DeleteGoalAsync(int goalId)
    {
        using var context = _contextFactory();
        var goal = await context.SavingsGoals.FindAsync(goalId);
        if (goal != null)
        {
            context.SavingsGoals.Remove(goal);
            await context.SaveChangesAsync();
        }
    }

    public async Task<decimal> GetTotalSavedAsync()
    {
        using var context = _contextFactory();
        return await context.SavingsGoals.SumAsync(g => g.CurrentAmount);
    }

    public async Task<decimal> GetTotalTargetAsync()
    {
        using var context = _contextFactory();
        return await context.SavingsGoals.SumAsync(g => g.TargetAmount);
    }

    public async Task<List<SavingsGoal>> GetCompletedGoalsAsync()
    {
        using var context = _contextFactory();
        return await context.SavingsGoals
            .Where(g => g.CurrentAmount >= g.TargetAmount)
            .ToListAsync();
    }

    public async Task<Dictionary<string, decimal>> GetGoalSummaryAsync()
    {
        using var context = _contextFactory();
        
        var goals = await context.SavingsGoals.ToListAsync();
        
        return new Dictionary<string, decimal>
        {
            { "TotalGoals", goals.Count },
            { "ActiveGoals", goals.Count(g => g.CurrentAmount < g.TargetAmount) },
            { "CompletedGoals", goals.Count(g => g.CurrentAmount >= g.TargetAmount) },
            { "TotalSaved", goals.Sum(g => g.CurrentAmount) },
            { "TotalTarget", goals.Sum(g => g.TargetAmount) }
        };
    }

    public async Task<double> GetOverallProgressPercentageAsync()
    {
        var summary = await GetGoalSummaryAsync();
        var totalTarget = summary["TotalTarget"];
        if (totalTarget == 0) return 0;
        return (double)(summary["TotalSaved"] / totalTarget * 100);
    }
}
