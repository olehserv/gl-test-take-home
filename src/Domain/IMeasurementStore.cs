namespace Domain;

public interface IMeasurementStore
{
    Task AddAsync(Measurement m);

    Task<IEnumerable<Measurement>> QueryAsync(string? type, DateTimeOffset since);
}
