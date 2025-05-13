using Raven.Client.Documents;

namespace TasksVs.Backend;

public static class Utils
{
    public static void PrintHelp()
    {
        string helpTable = @"
Usage:\n
0 - Generate documents\n
1 - Open Studio
2 - Test auto-textual index
3 - Test auto-numerical index
4 - Test auto-index with RavenVector
5 - Test static index
6 - Test static index with quantization
";

        Console.WriteLine(helpTable);
    }

    public static void GenerateDocuments()
    {
        var store = DocumentStoreHolder.Store;
        using var httpClient = new HttpClient();
        httpClient.Send(new HttpRequestMessage(HttpMethod.Post,
            $"{store.Urls[0]}/databases/{store.Database}/studio/sample-data"));

        using var bulkInsert = store.BulkInsert();
        for (int i = 0; i < 512; ++i)
        {
            bulkInsert.Store(new MyOwnProduct() { MyVector = [randomFloat(), randomFloat()] });
            bulkInsert.Store(new MyOwnProductWithRavenVector()
                { MyVector = new RavenVector<float>(new float[] { randomFloat(), randomFloat() }) });
        }

        float randomFloat() => Random.Shared.NextSingle() * (Random.Shared.Next() % 2 == 0 ? 1f : -1f);
    }
}