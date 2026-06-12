namespace Orders.API.Clients;

public interface IUsersApiClient
{
    Task<bool> UserExistsAsync(Guid userId, CancellationToken cancellationToken = default);
}