using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectManager.Persistence;
using ProjectManager.Features.Users.Models;

namespace ProjectManager.Features.Users.Queries;

public record GetUserByIdQuery(Guid Id) : IRequest<UserDto?>;

public class GetUserByIdHandler : IRequestHandler<GetUserByIdQuery, UserDto?>
{
    private readonly AppDbContext _db;

    public GetUserByIdHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<UserDto?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _db.Users.FindAsync(new object?[] { request.Id }, cancellationToken);
        return user == null
            ? null
            : new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
            };
    }
}
