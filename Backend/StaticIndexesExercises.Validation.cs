using System.Numerics.Tensors;
using Raven.Client.Documents.Queries.Timings;
using TasksVs.Backend;

// ReSharper disable once CheckNamespace
namespace TasksVs;

public partial class StaticIndexesExercises
{
    private void ValidateTestTextual(List<Product> products)
    {
        var printCount = Math.Min(products.Count, 16);
        for (int i = 0; i < printCount; ++i)
            Console.WriteLine($"{i}: '{products[i].Name}'");

        bool check = products[0].Name.Contains("mozzarella", StringComparison.CurrentCultureIgnoreCase);
        Console.ForegroundColor = check ? ConsoleColor.Green : ConsoleColor.Red;
        Console.WriteLine(check ? "### PASSED ###" : "### FAILED ###");
        Console.ResetColor();
    }

    private void ValidateNumericalSimpleIndexWithInt8Quantization(QueryTimings timings, float[] vector,
        List<MyOwnProduct> products)
    {
        using var session = DocumentStoreHolder.Store.OpenSession();
        var allDocs = session.Query<MyOwnProduct>().ToList();

        var allDocsWithDistance = allDocs
            .Select(x => new { x.Id, Distance = 1f - TensorPrimitives.CosineSimilarity(vector, x.MyVector) }).ToArray();
        allDocsWithDistance = allDocsWithDistance.OrderBy(x => x.Distance).ToArray();

        bool check = products.Count == 8;
        if (check == false)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("### FAILED ###");
            Console.WriteLine($"Expected count: 8, but got: {products.Count}");
            Console.ResetColor();
        }

        var print = products.Select(x => new
            { Id = x.Id, FloatDistance = 1f - TensorPrimitives.CosineSimilarity(vector, x.MyVector) });
        Console.WriteLine($"Received documents: {string.Join(", ", print.Select(x => $"(Id: {x.Id})"))}");
        Console.WriteLine(
            $"Original order: {string.Join(", ", allDocsWithDistance.Take(products.Count).Select(x => $"(Id: {x.Id}, D: {x.Distance})"))}");

        var method = ((QueryInspectionNode)timings.QueryPlan).Parameters["SimilarityMethod"];
        check = ((QueryInspectionNode)timings.QueryPlan).Parameters["SimilarityMethod"] == "CosineSimilarityI8";
        Console.ForegroundColor = check ? ConsoleColor.Green : ConsoleColor.Red;
        Console.WriteLine(check ? "### PASSED ###" : "### FAILED ###");
        if (check == false)
            Console.WriteLine($"Expected 'CosineSimilarityI8' but got '{method ?? string.Empty}'");

        Console.ResetColor();
    }
}