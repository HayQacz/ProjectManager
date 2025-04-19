using ProjectManager.Services.Interfaces;

namespace ProjectManager.Tests.Dependencies;

public class FakeUserContext : IUserContext
{
    public Guid UserId { get; set; } = Guid.NewGuid();
}
