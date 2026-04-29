// ReSharper disable once CheckNamespace
namespace TasksVs;

public partial class EmbeddingsGenerationTasksExercises
{
    private void ValidateSimpleEmbeddingsGenerationTask(List<Product> products)
    {
        var printCount = Math.Min(products.Count, 16);
        for (int i = 0; i < printCount; ++i)
            Console.WriteLine($"{i}: '{products[i].Name}'");

        bool check = products[0].Name.Contains("mozzarella", StringComparison.CurrentCultureIgnoreCase);
        Console.ForegroundColor = check ? ConsoleColor.Green : ConsoleColor.Red;
        Console.WriteLine(check ? "### PASSED ###" : "### FAILED ###");
        Console.ResetColor();
    }
}