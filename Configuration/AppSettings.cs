using DevMate.Shared.Constants;

namespace DevMate.Configuration;

public sealed record AppSettings(string Url)
{
    public static AppSettings FromEnvironment()
    {
        string value = "http://localhost:8080/";

        if (
            !Uri.TryCreate(value, UriKind.Absolute, out var url)
            || url.Scheme != Uri.UriSchemeHttp
            || url.AbsolutePath != "/"
            || url.UserInfo.Length != 0
            || url.Query.Length != 0
            || url.Fragment.Length != 0
        )
        {
            throw new ArgumentException(ErrorMessages.InvalidUrl);
        }

        return new AppSettings(url.AbsoluteUri);
    }
}
