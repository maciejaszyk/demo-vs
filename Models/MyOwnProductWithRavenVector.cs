using Raven.Client.Documents;

namespace TasksVs;

public class MyOwnProductWithRavenVector
{
    public string Id { get; set; }
    public RavenVector<float> MyVector { get; set; }
}