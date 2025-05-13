using Raven.Embedded;
using TasksVs;
using TasksVs.Backend;

_ = DocumentStoreHolder.Store;
Utils.PrintHelp();


while (Console.ReadLine() is { } s)
{
    if (int.TryParse(s, out var result) == false)
    {
        Utils.PrintHelp();
        continue;
    }

    switch (result)
    {
        case 0:
            Console.Write("\nGenerating documents...");
            Utils.GenerateDocuments();
            Console.WriteLine("Done");
            break;

        case 1:
            EmbeddedServer.Instance.OpenStudioInBrowser();
            break;
        
        case 2:
            new AutoIndexesExercises().TestTextual();
            break;

        case 3:
            new AutoIndexesExercises().TestNumerical();
            break;

        case 4:
            new AutoIndexesExercises().TestRavenVector();
            break;

        case 5:
            new StaticIndexesExercises().TextualSimpleIndex();
            break;        
        
        case 6:
            new StaticIndexesExercises().NumericalSimpleIndexWithInt8Quantization();
            break;

        default:
            Console.WriteLine($"Unknown option {s}.");
            Utils.PrintHelp();
            break;
    }
}