using System.Numerics.Tensors;
using Raven.Client.Documents;
using TasksVs.Backend;

// ReSharper disable once CheckNamespace
namespace TasksVs;

public partial class AutoIndexesExercises
{
    private void ValidateTestTextual(List<Product> products)
    {
        var printCount = Math.Min(products.Count, 16);
        for (int i = 0; i < printCount; ++i)
        {
            Console.WriteLine($"{i}: '{products[i].Name}'");
        }

        bool check = products[0].Name.Contains("mozzarella", StringComparison.CurrentCultureIgnoreCase);
        Console.ForegroundColor = check ? ConsoleColor.Green : ConsoleColor.Red;
        Console.WriteLine(check ? "### PASSED ###" : "### FAILED ###");
        Console.ResetColor();
    }

    private void ValidateTestNumerical(float[] vector, List<MyOwnProduct> products)
    {
        using var session = DocumentStoreHolder.Store.OpenSession();
        var allDocs = session.Query<MyOwnProduct>().ToList();

        var allDocsWithCosSim = allDocs
            .Select(x => new { x.Id, Distance = 1f - TensorPrimitives.CosineSimilarity(vector, x.MyVector) }).ToArray();
        allDocsWithCosSim = allDocsWithCosSim.OrderBy(x => x.Distance).ToArray();

        bool check = products.Count == 8;
        if (check == false)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("### FAILED ###.");
            Console.WriteLine($"Expected count: 8. Received: {products.Count}");
            Console.ResetColor();
        }

        Console.WriteLine($"Expected documents: {string.Join(", ", allDocsWithCosSim.Take(8).Select(x => x.Id))}");
        Console.WriteLine($"Retrieved documents: {string.Join(", ", products.Select(x => x.Id))}");

        check = products.Count >= 8 && allDocsWithCosSim.Length >= 8;
        for (int i = 0; i < 8 && check; ++i)
        {
            check &= products[i].Id == allDocsWithCosSim[i].Id;
        }

        Console.ForegroundColor = check ? ConsoleColor.Green : ConsoleColor.Red;
        Console.WriteLine(check ? "### PASSED ###" : "### FAILED ###");
        Console.ResetColor();
    }

    private void ValidateTestRavenVector(RavenVector<float>? vector, List<MyOwnProductWithRavenVector> products)
    {
        using var session = DocumentStoreHolder.Store.OpenSession();
        var allDocs = session.Query<MyOwnProductWithRavenVector>().ToList();

        var allDocsWithCosSim = allDocs
            .Select(x => new
                { x.Id, Distance = 1f - TensorPrimitives.CosineSimilarity(vector.Embedding, x.MyVector.Embedding) })
            .ToArray();
        allDocsWithCosSim = allDocsWithCosSim.OrderBy(x => x.Distance).ToArray();

        Console.WriteLine($"Expected documents: {string.Join(", ", allDocsWithCosSim.Take(8).Select(x => x.Id))}");
        Console.WriteLine($"Retrieved documents: {string.Join(", ", products.Select(x => x.Id))}");

        var check = true;
        for (int i = 0; i < products.Count && check; ++i)
        {
            check &= products[i].Id == allDocsWithCosSim[i].Id;
        }

        Console.ForegroundColor = check ? ConsoleColor.Green : ConsoleColor.Red;
        Console.WriteLine(check ? "### PASSED ###" : "### FAILED ###");
        Console.ResetColor();
    }

    private void ValidateRavenVector(RavenVector<float>? vector)
    {
        var check = vector is not null
                    && (vector.Embedding[0] - 0.5f) <= float.Epsilon
                    && (vector.Embedding[1] - 0.5f) <= float.Epsilon;

        Console.ForegroundColor = check ? ConsoleColor.Green : ConsoleColor.Red;
        Console.WriteLine(check ? "### PASSED ###" : "### FAILED ###");
        Console.WriteLine($"Vector: [{string.Join(", ", vector.Embedding ?? [])}]");
        Console.ResetColor();
    }
}