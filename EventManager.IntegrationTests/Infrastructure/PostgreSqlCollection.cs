namespace EventManager.IntegrationTests.Infrastructure;

[CollectionDefinition("Postgres")]
public class PostgreSqlCollection : ICollectionFixture<PostgreSqlFixture>
{
}
