using Raven.Client.Documents;
using Raven.Client.ServerWide.Operations;
using Raven.Embedded;

namespace TasksVs.Backend;

public static class DocumentStoreHolder
{
    public static string DatabaseName = "VectorSearchExercises";
    private static readonly Lazy<IDocumentStore> _store = new Lazy<IDocumentStore>(CreateDocumentStore);

    private static IDocumentStore CreateDocumentStore()
    {
        EmbeddedServer.Instance.StartServer(new ServerOptions
        {
            ServerUrl = "http://127.0.0.1:8080",
        });
        
        string serverURL = "http://127.0.0.1:8080";
        if (string.IsNullOrEmpty(DatabaseName))
            throw new Exception("Database name is not set");

        IDocumentStore documentStore = new DocumentStore
        {
            Urls = new[] { serverURL },
            Database = DatabaseName
        };
        documentStore.OnBeforeQuery += (sender, beforeQueryExecutedArgs) =>
        {
            beforeQueryExecutedArgs.QueryCustomization.WaitForNonStaleResults();
        };
        documentStore.Initialize();

        try
        {
            var op = new CreateDatabaseOperation(x => x.Regular(DatabaseName).WithReplicationFactor(1));
            documentStore.Maintenance.Server.Send(op);
        }
        catch
        {
            // Database already exists, ignore
        }
        
        return documentStore;
    }

    public static IDocumentStore Store
    {
        get { return _store.Value; }
    }
}