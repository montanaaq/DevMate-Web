using Npgsql;
using SkillIssue.Models.Applications;
using SkillIssue.Models.Invitations;
using SkillIssue.Models.Teams;

namespace SkillIssue.Configuration;

public static class DatabaseSettings
{
    public static NpgsqlDataSource CreateDataSource(string connectionString)
    {
        var builder = new NpgsqlDataSourceBuilder(connectionString);

        builder.MapEnum<ClosureKind>("closure_kind");
        builder.MapEnum<ApplicationStatus>("application_status");
        builder.MapEnum<InvitationStatus>("invitation_status");

        return builder.Build();
    }
}
