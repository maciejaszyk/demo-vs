using Raven.Client.Documents.Indexes;
using Raven.Client.Documents.Queries.Timings;
using TasksVs.Backend;

namespace TasksVs;

public partial class StaticIndexesExercises
{
    public void TextualSimpleIndex()
    {
        const string searchedPhrase = "italian food";

        using var session = DocumentStoreHolder.Store.OpenSession();
        
        // 1. Check the index definition.
        new TextualStaticIndex().Execute(DocumentStoreHolder.Store);

        // 2. Fill in the missing part of VectorSearch.
        var results = session.Query<Product, TextualStaticIndex>()
            //.VectorSearch() // Fill to get items related to Italian food (use the searchedPhrase constant).
            .ToList();
        
        ValidateTestTextual(results);
    }

    // HINT: https://ravendb.net/docs/article-page/7.0/csharp/ai-integration/vector-search/vector-search-using-static-index?#indexing-vector-data---text
    private class TextualStaticIndex : AbstractIndexCreationTask<Product>
    {
        public TextualStaticIndex()
        {
            Map = products => from product in products
                select new
                {
                    Id = product.Id,
                    Name = product.Name // TODO: Make this field searchable via vector.search.
                };
            
            // The index MUST use the Corax search engine 
            SearchEngineType = Raven.Client.Documents.Indexes.SearchEngineType.Corax;
        }
    }

    public void NumericalSimpleIndexWithInt8Quantization()
    {
        QueryTimings timings = null;
        float[] vector = [0.5f, 0.5f];
        
        using var session = DocumentStoreHolder.Store.OpenSession();
        
        // 1. Check the index definition.
        new NumericalQuantizedStaticIndex().Execute(DocumentStoreHolder.Store);
        
        // 2. Fill in the missing part of VectorSearch.
        var results = session.Query<MyOwnProduct, NumericalQuantizedStaticIndex>()
            .Customize(x => x.Timings(out timings))
            //.VectorSearch() // Search for vectors related to `vector`.
            .ToList();

        ValidateNumericalSimpleIndexWithInt8Quantization(timings, vector, results);
    }
    
    private class NumericalQuantizedStaticIndex : AbstractIndexCreationTask<MyOwnProduct>
    {
        public NumericalQuantizedStaticIndex()
        {
            Map = products => from product in products
                select new
                {
                    Id = product.Id,
                    MyVector = product.MyVector // Create a vector field from the array stored in the document.
                };
            
            // Set quantization to Int8.
            //Vector(todo);
            // HINT: https://ravendb.net/docs/article-page/7.0/csharp/ai-integration/vector-search/vector-search-using-static-index?#indexing-vector-data---numerical
            
            // The index MUST use the Corax search engine 
            SearchEngineType = Raven.Client.Documents.Indexes.SearchEngineType.Corax;
        }
    }
}