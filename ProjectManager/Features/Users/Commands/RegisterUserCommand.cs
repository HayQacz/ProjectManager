using MediatR;
using Microsoft.AspNetCore.Identity;
using ProjectManager.Entities;
using ProjectManager.Persistence;
using ProjectManager.Features.Users.Models;


namespace ProjectManager.Features.Users.Commands;

public record RegisterUserCommand(string Email, string FullName, string Password) : IRequest<UserDto>;

public class RegisterUserHandler : IRequestHandler<RegisterUserCommand, UserDto>
{
    private readonly AppDbContext _db;
    private readonly IPasswordHasher<User> _hasher;

    public RegisterUserHandler(AppDbContext db, IPasswordHasher<User> hasher)
    {
        _db = db;
        _hasher = hasher;
    }

    public async Task<UserDto> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var user = new User
        {
            Email = request.Email,
            FullName = request.FullName
        };

        user.PasswordHash = _hasher.HashPassword(user, request.Password);

        _db.Users.Add(user);
        await _db.SaveChangesAsync(cancellationToken);

        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            ProjectMemberId = user.ProjectMemberId
        };
    }
}
