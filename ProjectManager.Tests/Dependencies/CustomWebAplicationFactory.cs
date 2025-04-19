using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProjectManager.Persistence;
using ProjectManager.Features.Users.Commands;
using ProjectManager.Features.Projects.Commands;
using ProjectManager.Features.ProjectMembers.Commands;
using ProjectManager.Features.ProjectTasks.Commands;
using ProjectManager.Services.Interfaces;
using ProjectManager.Services.Implementations;
using ProjectManager.Services.Authorization;

namespace ProjectManager.Tests.Dependencies;

public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

            if (descriptor != null)
                services.Remove(descriptor);

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase("TestDb_" + Guid.NewGuid());
            });

            services.AddScoped<IUserContext, FakeUserContext>();
            services.AddScoped<IProjectAuthorizationService, ProjectAuthorizationService>();

            services.AddScoped<RegisterUserHandler>();
            services.AddScoped<LoginUserHandler>();
            services.AddScoped<CreateProjectHandler>();
            services.AddScoped<CreateProjectMemberHandler>();
            services.AddScoped<AddProjectMemberToProjectHandler>();
            services.AddScoped<ChangeProjectMemberRoleHandler>();
            services.AddScoped<RemoveProjectMemberFromProjectHandler>();
            services.AddScoped<UpdateProjectTaskHandler>();
            services.AddScoped<DeleteProjectTaskHandler>();
        });
    }
}
