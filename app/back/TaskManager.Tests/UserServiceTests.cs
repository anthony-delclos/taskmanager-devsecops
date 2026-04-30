using FluentAssertions;
using Moq;
using TaskManager.API.DTOs.Requests;
using TaskManager.API.Entities;
using TaskManager.API.Repositories.Interfaces;
using TaskManager.API.Services;

namespace TaskManager.Tests;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _repoMock;
    private readonly UserService _service;

    public UserServiceTests()
    {
        _repoMock = new Mock<IUserRepository>();
        _service = new UserService(_repoMock.Object);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsMappedUsers_WithoutPasswordHash()
    {
        var users = new List<CmUser>
        {
            new() { Id = Guid.NewGuid(), Username = "alice", Email = "alice@test.com", PasswordHash = "secret", IsAdmin = false, CreationDate = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Username = "bob",   Email = "bob@test.com",   PasswordHash = "secret", IsAdmin = true,  CreationDate = DateTime.UtcNow }
        };
        _repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(users);

        var result = (await _service.GetAllAsync()).ToList();

        result.Should().HaveCount(2);
        result.Select(u => u.Username).Should().BeEquivalentTo(new[] { "alice", "bob" });
        result.Select(u => u.Email).Should().NotContain("secret");
    }

    [Fact]
    public async Task GetByIdAsync_WhenUserExists_ReturnsMappedResponse()
    {
        var id = Guid.NewGuid();
        var user = new CmUser { Id = id, Username = "alice", Email = "alice@test.com", PasswordHash = "hash", CreationDate = DateTime.UtcNow };
        _repoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(user);

        var result = await _service.GetByIdAsync(id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(id);
        result.Username.Should().Be("alice");
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ReturnsNull()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((CmUser?)null);

        var result = await _service.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_HashesPassword_NeverStoresPlaintext()
    {
        const string plainPassword = "P@ssw0rd123";
        _repoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((CmUser?)null);
        _repoMock.Setup(r => r.AddAsync(It.IsAny<CmUser>())).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveAsync()).Returns(Task.CompletedTask);

        var request = new CreateUserRequest { Username = "alice", Email = "alice@test.com", Password = plainPassword };

        var result = await _service.CreateAsync(request);

        result.Username.Should().Be("alice");
        _repoMock.Verify(r => r.AddAsync(It.Is<CmUser>(u =>
            u.PasswordHash != plainPassword &&
            !string.IsNullOrEmpty(u.PasswordHash))), Times.Once);
        _repoMock.Verify(r => r.SaveAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenEmailAlreadyExists_ThrowsInvalidOperationException()
    {
        _repoMock.Setup(r => r.GetByEmailAsync("alice@test.com"))
                 .ReturnsAsync(new CmUser { Email = "alice@test.com" });

        var request = new CreateUserRequest { Username = "alice2", Email = "alice@test.com", Password = "P@ssw0rd123" };

        await _service.Invoking(s => s.CreateAsync(request))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already*");
    }

    [Fact]
    public async Task UpdateAsync_WhenUserExists_UpdatesFieldsAndReturnsTrue()
    {
        var id = Guid.NewGuid();
        var user = new CmUser { Id = id, Username = "old", Email = "old@test.com", PasswordHash = "hash", CreationDate = DateTime.UtcNow };
        _repoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(user);
        _repoMock.Setup(r => r.Update(It.IsAny<CmUser>()));
        _repoMock.Setup(r => r.SaveAsync()).Returns(Task.CompletedTask);

        var result = await _service.UpdateAsync(id, new UpdateUserRequest { Username = "new", Email = "new@test.com" });

        result.Should().BeTrue();
        user.Username.Should().Be("new");
        user.Email.Should().Be("new@test.com");
    }

    [Fact]
    public async Task UpdateAsync_WhenNotFound_ReturnsFalse()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((CmUser?)null);

        var result = await _service.UpdateAsync(Guid.NewGuid(), new UpdateUserRequest());

        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_WhenPasswordProvided_ReHashesPassword()
    {
        var id = Guid.NewGuid();
        var user = new CmUser { Id = id, Username = "alice", Email = "alice@test.com", PasswordHash = "oldhash", CreationDate = DateTime.UtcNow };
        _repoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(user);
        _repoMock.Setup(r => r.Update(It.IsAny<CmUser>()));
        _repoMock.Setup(r => r.SaveAsync()).Returns(Task.CompletedTask);

        await _service.UpdateAsync(id, new UpdateUserRequest { Password = "NewP@ss123!" });

        user.PasswordHash.Should().NotBe("oldhash");
        user.PasswordHash.Should().NotBe("NewP@ss123!");
    }

    [Fact]
    public async Task UpdateAsync_WhenNoPasswordProvided_KeepsExistingHash()
    {
        var id = Guid.NewGuid();
        var user = new CmUser { Id = id, Username = "alice", Email = "alice@test.com", PasswordHash = "existinghash", CreationDate = DateTime.UtcNow };
        _repoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(user);
        _repoMock.Setup(r => r.Update(It.IsAny<CmUser>()));
        _repoMock.Setup(r => r.SaveAsync()).Returns(Task.CompletedTask);

        await _service.UpdateAsync(id, new UpdateUserRequest { Username = "updated" });

        user.PasswordHash.Should().Be("existinghash");
    }

    [Fact]
    public async Task DeleteAsync_WhenUserExists_CallsDeleteAndReturnsTrue()
    {
        var id = Guid.NewGuid();
        var user = new CmUser { Id = id, Username = "alice", Email = "alice@test.com", PasswordHash = "hash", CreationDate = DateTime.UtcNow };
        _repoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(user);
        _repoMock.Setup(r => r.Delete(It.IsAny<CmUser>()));
        _repoMock.Setup(r => r.SaveAsync()).Returns(Task.CompletedTask);

        var result = await _service.DeleteAsync(id);

        result.Should().BeTrue();
        _repoMock.Verify(r => r.Delete(user), Times.Once);
        _repoMock.Verify(r => r.SaveAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenNotFound_ReturnsFalse()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((CmUser?)null);

        var result = await _service.DeleteAsync(Guid.NewGuid());

        result.Should().BeFalse();
        _repoMock.Verify(r => r.Delete(It.IsAny<CmUser>()), Times.Never);
    }
}
