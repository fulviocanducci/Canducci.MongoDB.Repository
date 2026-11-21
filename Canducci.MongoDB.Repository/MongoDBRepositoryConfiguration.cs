namespace Canducci.MongoDB.Repository
{
    public sealed class MongoDBRepositoryConfiguration
    {
        public string Database { get; set; }
        public string ConnectionString { get; set; }
        public RepositoryRegistrationMode RegistrationMode { get; set; } = RepositoryRegistrationMode.Manual;
    }
}
