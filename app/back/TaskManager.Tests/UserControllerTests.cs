using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TaskManager.API.Controllers;
using TaskManager.API.DTOs.Requests;
using TaskManager.API.DTOs.Responses;
using TaskManager.API.Services.Interfaces;

namespace TaskManager.Tests;

public class UserControllerTests
{
    private readonly Mock<IUserService> _serviceMock;
    private readonly UsersController _controller;

    public UserControllerTests()
    {
        _serviceMock = new Mock<IUserService>();
        _controller = new UsersController(_serviceMock.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsOkWithUserList()
    {
        var users = new List<UserResponse>
        {
            new() { Id = Guid.NewGuid(), Username = "alice", Email = "alice@test.com" },
            new() { Id = Guid.NewGuid(), Username = "bob",   Email = "bob@test.com"   }
        };
        _serviceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(users);

        var result = await _controller.GetAll();

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeAssignableTo<IEnumerable<UserResponse>>()
          .Which.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetById_WhenFound_ReturnsOkWithUser()
    {
        var id = Guid.NewGuid();
        _serviceMock.Setup(s => s.GetByIdAsync(id))
                    .ReturnsAsync(new UserResponse { Id = id, Username = "alice", Email = "alice@test.com" });

        var result = await _controller.GetById(id);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeOfType<UserResponse>()
          .Which.Id.Should().Be(id);
    }

    [Fact]
    public async Task GetById_WhenNotFound_Returns404()
    {
        _serviceMock.Setup(s => s.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((UserResponse?)null);

        var result = await _controller.GetById(Guid.NewGuid());

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Create_WithValidData_Returns201WithLocationHeader()
    {
        var id = Guid.NewGuid();
        var created = new UserResponse { Id = id, Username = "alice", Email = "alice@test.com" };
        _serviceMock.Setup(s => s.CreateAsync(It.IsAny<CreateUserRequest>())).ReturnsAsync(created);

        var result = await _controller.Create(
            new CreateUserRequest { Username = "alice", Email = "alice@test.com", Password = "P@ssw0rd!" });

        var createdAt = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdAt.StatusCode.Should().Be(201);
        createdAt.ActionName.Should().Be(nameof(_controller.GetById));
        createdAt.Value.Should().Be(created);
    }

    [Fact]
    public async Task Create_WhenEmailDuplicate_Returns409Conflict()
    {
        _serviceMock.Setup(s => s.CreateAsync(It.IsAny<CreateUserRequest>()))
                    .ThrowsAsync(new InvalidOperationException("Email already registered."));

        var result = await _controller.Create(
            new CreateUserRequest { Username = "alice", Email = "dup@test.com", Password = "P@ssw0rd!" });

        var conflict = result.Should().BeOfType<ConflictObjectResult>().Subject;
        conflict.StatusCode.Should().Be(409);
    }

    [Fact]
    public async Task Update_WhenFound_Returns204NoContent()
    {
        _serviceMock.Setup(s => s.UpdateAsync(It.IsAny<Guid>(), It.IsAny<UpdateUserRequest>()))
                    .ReturnsAsync(true);

        var result = await _controller.Update(Guid.NewGuid(), new UpdateUserRequest { Username = "new" });

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Update_WhenNotFound_Returns404()
    {
        _serviceMock.Setup(s => s.UpdateAsync(It.IsAny<Guid>(), It.IsAny<UpdateUserRequest>()))
                    .ReturnsAsync(false);

        var result = await _controller.Update(Guid.NewGuid(), new UpdateUserRequest());

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Delete_WhenFound_Returns204NoContent()
    {
        _serviceMock.Setup(s => s.DeleteAsync(It.IsAny<Guid>())).ReturnsAsync(true);

        var result = await _controller.Delete(Guid.NewGuid());

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Delete_WhenNotFound_Returns404()
    {
        _serviceMock.Setup(s => s.DeleteAsync(It.IsAny<Guid>())).ReturnsAsync(false);

        var result = await _controller.Delete(Guid.NewGuid());

        result.Should().BeOfType<NotFoundResult>();
    }
}
