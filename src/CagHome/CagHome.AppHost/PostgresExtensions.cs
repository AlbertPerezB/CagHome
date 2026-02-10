namespace CagHome.AppHost;

public static class PostgresExtensions
{
    public static IResourceBuilder<PostgresServerResource> WithV18DataVolume(
        this IResourceBuilder<PostgresServerResource> builder, string? name = null, bool isReadOnly = false)
    {
        return builder.WithVolume(name ?? VolumeNameGenerator.Generate(builder, "data"),
            "/var/lib/postgresql/18/docker", isReadOnly);
    }
}
