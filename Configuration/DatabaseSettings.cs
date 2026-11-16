using DevMate.Models.Applications;
using DevMate.Models.Invitations;
using DevMate.Models.Teams;
using DevMate.Models.Users;
using Npgsql;

namespace DevMate.Configuration;

public static class DatabaseSettings
{
    public static NpgsqlDataSource CreateDataSource(string connectionString)
    {
        var builder = new NpgsqlDataSourceBuilder(connectionString);

        builder.MapEnum<SocialNetwork>("social_network");
        builder.MapEnum<ClosureKind>("closure_kind");
        builder.MapEnum<ApplicationStatus>("application_status");
        builder.MapEnum<InvitationStatus>("invitation_status");

        return builder.Build();
    }
}
