using Npgsql;
namespace ResearchHub.Api.Data;
public static class ReaderExtensions
{
    public static string? GetNullableString(this NpgsqlDataReader r, string name)
    { var i=r.GetOrdinal(name); return r.IsDBNull(i) ? null : r.GetString(i); }
    public static long? GetNullableInt64(this NpgsqlDataReader r, string name)
    { var i=r.GetOrdinal(name); return r.IsDBNull(i) ? null : r.GetInt64(i); }
    public static DateOnly? GetNullableDateOnly(this NpgsqlDataReader r, string name)
    { var i=r.GetOrdinal(name); return r.IsDBNull(i) ? null : r.GetFieldValue<DateOnly>(i); }
}
