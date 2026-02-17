using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using System.Linq;
using MoneyTracker2026.Data;
using MoneyTracker2026.Models;
using MoneyTracker2026.Services;

namespace MoneyTracker2026.Views
{
    public sealed partial class GoalsPage : Page
    {
        public ObservableCollection<GoalDisplayItem> Goals { get; } = new();

        public GoalsPage()
        {
            this.InitializeComponent();
            LoadGoals();
        }

        private void LoadGoals()
        {
            var profileName = ProfileManager.CurrentProfile?.Name ?? "Default";
            using var context = new AppDbContext(profileName);

            var goals = context.SavingsGoals.ToList();

            Goals.Clear();
            foreach (var goal in goals)
            {
                var percentComplete = goal.TargetAmount > 0 
                    ? (double)(goal.CurrentAmount / goal.TargetAmount) * 100 
                    : 0;

                Goals.Add(new GoalDisplayItem
                {
                    Name = goal.Name,
                    Icon = goal.Icon,
                    CurrentAmount = goal.CurrentAmount,
                    TargetAmount = goal.TargetAmount,
                    PercentComplete = percentComplete,
                    CurrentText = $"£{goal.CurrentAmount:N2}",
                    TargetText = $"of £{goal.TargetAmount:N2}",
                    TargetDateText = goal.TargetDate.HasValue 
                        ? $"Target: {goal.TargetDate:MMM dd, yyyy}" 
                        : "No target date"
                });
            }

            GoalsGrid.ItemsSource = Goals;
        }

        private void AddGoal_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            var dialog = new ContentDialog
            {
                Title = "Add Savings Goal",
                Content = "Goal dialog coming soon!",
                CloseButtonText = "OK"
            };
            dialog.ShowAsync();
        }
    }

    public class GoalDisplayItem
    {
        public string Name { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public decimal CurrentAmount { get; set; }
        public decimal TargetAmount { get; set; }
        public double PercentComplete { get; set; }
        public string CurrentText { get; set; } = string.Empty;
        public string TargetText { get; set; } = string.Empty;
        public string TargetDateText { get; set; } = string.Empty;
    }
}
