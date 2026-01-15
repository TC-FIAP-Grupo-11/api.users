using FCG.Api.Users.Domain.Entities;
using FCG.Lib.Shared.Application.Contracts.Repositories;

namespace FCG.Api.Users.Application.Contracts.Repositories;

public interface IUserRepository : IBaseRepository<User>
{
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email,  CancellationToken cancellationToken = default);
}