using System.Windows;

namespace MoneyTracker2026.Services;

public class NotificationService
{
    public void ShowSuccess(string message)
    {
        ShowToast("Success", message);
    }

    public void ShowError(string message)
    {
        ShowToast("Error", message);
    }

    public void ShowWarning(string message)
    {
        ShowToast("Warning", message);
    }

    public void ShowInfo(string message)
    {
        ShowToast("Info", message);
    }

    public void ShowBudgetWarning(string categoryName, decimal spent, decimal budget)
    {
        var percent = (spent / budget) * 100;
        ShowWarning($"Budget Alert: {categoryName} is at {percent:F0}% (£{spent:F2} of £{budget:F2})");
    }

    public void ShowGoalAchieved(string goalName, decimal amount)
    {
        ShowSuccess($"Goal Achieved! {goalName} - £{amount:F2}");
    }

    public void ShowDebtSavings(string method, decimal savings)
    {
        ShowInfo($"Debt Update: {method} saves £{savings:F2} in interest!");
    }

    public void ShowImportComplete(int count, string currency)
    {
        ShowSuccess($"Import Complete: {count} transactions imported (mostly {currency})");
    }

    private void ShowToast(string title, string message)
    {
        // For now, we'll use a simple MessageBox
        // In a real app, you'd use Windows Toast Notifications
        Application.Current.Dispatcher.Invoke(() =>
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, 
                title == "Error" ? MessageBoxImage.Error :
                title == "Warning" ? MessageBoxImage.Warning :
                title == "Success" ? MessageBoxImage.Information :
                MessageBoxImage.Information);
        });
    }

    public void ShowBackupComplete(string path)
    {
        ShowSuccess($"Backup saved to: {path}");
    }

    public void ShowBackupRestoreComplete(string path)
    {
        ShowSuccess($"Backup restored from: {path}");
    }
}
