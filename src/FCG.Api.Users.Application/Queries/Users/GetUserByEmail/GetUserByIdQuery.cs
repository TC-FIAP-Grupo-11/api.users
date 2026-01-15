using MediatR;
using FCG.Api.Users.Domain.Entities;
using FCG.Lib.Shared.Application.Common.Models;

namespace FCG.Api.Users.Application.Queries.Users.GetUserByEmail;

public record GetUserByEmailQuery(string Email) : IRequest<Result<User>>;
