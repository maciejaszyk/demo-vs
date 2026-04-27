using Raven.Client.Documents;
using Raven.Client.Documents.Indexes;
using Raven.Client.Documents.Indexes.Vector;
using Raven.Client.Documents.Operations.AI;
using Raven.Client.Documents.Operations.ConnectionStrings;
using TasksVs.Backend;

namespace TasksVs;

public partial class EmbeddingsGenerationTasksExercises
{
    public void SimpleEmbeddingsGenerationTask()
    {
        var store = DocumentStoreHolder.Store;
        
        var connectionString = new AiConnectionString
        {
            Name = "ConnectionStringToEmbedded",
            Identifier = "connection-string-to-embedded",
            ModelType = AiModelType.TextEmbeddings,
            EmbeddedSettings = new EmbeddedSettings()
        };

        var putConnectionStringOperation = new PutConnectionStringOperation<AiConnectionString>(connectionString);
        store.Maintenance.Send(putConnectionStringOperation);

        // HINT: https://docs.ravendb.net/ai-integration/generating-embeddings/embeddings-generation-task#configure-an-embeddings-generation-task---define-source-using-paths
        var embeddingsTaskConfiguration = new EmbeddingsGenerationConfiguration
        {
            Name = "EmbeddingsGenerationTask",
            Identifier = "embeddings-generation-task",
            ConnectionStringName = connectionString.Name,
            Disabled = false,
            Collection = "Products",
            EmbeddingsPathConfigurations =
            [
                new EmbeddingPathConfiguration()
                {
                    Path = "Name",
                    // Add ChunkingOptions: use PlainTextSplitParagraphs method with 2048 max tokens per chunk and 128 overlap tokens.
                }
            ],

            // Add ChunkingOptionsForQuerying: use PlainTextSplit method with 2048 max tokens per chunk.
        };
        
        var addEmbeddingsGenerationTaskOperation = new AddEmbeddingsGenerationOperation(embeddingsTaskConfiguration);
        store.Maintenance.Send(addEmbeddingsGenerationTaskOperation);
        
        var index = new Products_ByNamePreMadeEmbeddings();
        index.Execute(store);
        
        using (var session = store.OpenSession())
        {
            // HINT: https://docs.ravendb.net/7.2/ai-integration/vector-search/vector-search-using-static-index#indexing-pre-made-text-embeddings-generated-by-tasks
            var similarProducts = session.Query<Products_ByNamePreMadeEmbeddings.IndexEntry, Products_ByNamePreMadeEmbeddings>()
                //.VectorSearch() // Fill to search for products related to "italian food".
                .Customize(x => x.WaitForNonStaleResults())
                .OfType<Product>()
                .ToList();

            ValidateSimpleEmbeddingsGenerationTask(similarProducts);
        }
    }
    
    // HINT: https://docs.ravendb.net/7.2/ai-integration/vector-search/vector-search-using-static-index#indexing-pre-made-text-embeddings-generated-by-tasks
    public class Products_ByNamePreMadeEmbeddings :
        AbstractIndexCreationTask<Product, Products_ByNamePreMadeEmbeddings.IndexEntry>
    {
        public class IndexEntry()
        {
            // This index field will hold the text embeddings
            // that were pre-made by the Embeddings Generation Task
            public object VectorFromTextEmbeddings { get; set; }
        }
  
        public Products_ByNamePreMadeEmbeddings()
        {
            Map = products => from product in products
                select new IndexEntry
                {
                    // Call 'LoadVector' to create a VECTOR FIELD. Pass:
                    // * The document field name to be indexed (as a string)
                    // * The identifier of the task that generated the embeddings
                    //   for the 'Name' field
                    VectorFromTextEmbeddings = null
                };

            VectorIndexes.Add(x => x.VectorFromTextEmbeddings, new VectorOptions());
      
            // The index MUST use the Corax search engine 
            SearchEngineType = Raven.Client.Documents.Indexes.SearchEngineType.Corax;
        }
    }
}