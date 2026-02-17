using System.IO;
using MoneyTracker2026.Data;
using MoneyTracker2026.Models;

namespace MoneyTracker2026.Services;

public class CategoryService
{
    private readonly Func<AppDbContext> _contextFactory;

    public CategoryService(Func<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<Category>> GetAllCategoriesAsync()
    {
        using var context = _contextFactory();
        return await Task.FromResult(context.Categories.OrderBy(c => c.Name).ToList());
    }

    public async Task<List<Category>> GetCategoriesByTypeAsync(TransactionType type)
    {
        using var context = _contextFactory();
        var categories = await Task.FromResult(context.Categories.ToList());
        
        // Filter based on typical income/expense categories
        return type == TransactionType.Income 
            ? categories.Where(c => c.Name.Contains("Income") || c.Name.Contains("Salary") || c.Name.Contains("Investment")).ToList()
            : categories.Where(c => !c.Name.Contains("Income") && !c.Name.Contains("Salary") && !c.Name.Contains("Investment")).ToList();
    }

    public async Task<Category?> GetCategoryByIdAsync(int id)
    {
        using var context = _contextFactory();
        return await Task.FromResult(context.Categories.Find(id));
    }

    public async Task<Category> CreateCategoryAsync(Category category)
    {
        using var context = _contextFactory();
        context.Categories.Add(category);
        await context.SaveChangesAsync();
        return category;
    }

    public async Task<Category> UpdateCategoryAsync(Category category)
    {
        using var context = _contextFactory();
        context.Categories.Update(category);
        await context.SaveChangesAsync();
        return category;
    }

    public async Task DeleteCategoryAsync(int categoryId)
    {
        using var context = _contextFactory();
        var category = await context.Categories.FindAsync(categoryId);
        if (category != null && !category.IsSystem)
        {
            context.Categories.Remove(category);
            await context.SaveChangesAsync();
        }
    }

    public async Task SeedDefaultCategoriesAsync()
    {
        using var context = _contextFactory();
        
        if (context.Categories.Any())
            return;

        var categories = new List<Category>
        {
            new() { Name = "Food & Dining", IconGlyph = "🍕", ColorHex = "#FF6B6B", IsSystem = true },
            new() { Name = "Transportation", IconGlyph = "🚗", ColorHex = "#4ECDC4", IsSystem = true },
            new() { Name = "Shopping", IconGlyph = "🛍️", ColorHex = "#45B7D1", IsSystem = true },
            new() { Name = "Entertainment", IconGlyph = "🎬", ColorHex = "#96CEB4", IsSystem = true },
            new() { Name = "Bills & Utilities", IconGlyph = "💡", ColorHex = "#FFEAA7", IsSystem = true },
            new() { Name = "Healthcare", IconGlyph = "🏥", ColorHex = "#DDA0DD", IsSystem = true },
            new() { Name = "Income", IconGlyph = "💰", ColorHex = "#00C4B4", IsSystem = true },
            new() { Name = "Transfer", IconGlyph = "🔄", ColorHex = "#95A5A6", IsSystem = true },
            new() { Name = "Other", IconGlyph = "📦", ColorHex = "#BDC3C7", IsSystem = true }
        };

        context.Categories.AddRange(categories);
        await context.SaveChangesAsync();
    }

    public string GetCategoryIcon(int categoryId)
    {
        using var context = _contextFactory();
        var category = context.Categories.Find(categoryId);
        return category?.IconGlyph ?? "📦";
    }

    public string GetCategoryColor(int categoryId)
    {
        using var context = _contextFactory();
        var category = context.Categories.Find(categoryId);
        return category?.ColorHex ?? "#BDC3C7";
    }
}
