/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.DTOs;
using Application.Services;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Application.Tests
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<ILogger<UserService>> _loggerMock;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            // Initialize mocks for repository and logger.
            _userRepositoryMock = new Mock<IUserRepository>();
            _loggerMock = new Mock<ILogger<UserService>>();
            _userService = new UserService(_userRepositoryMock.Object, _loggerMock.Object);
        }

        #region GetUserProfileAsync Tests

        [Fact]
        public async Task GetUserProfileAsync_UserNotFound_ThrowsUserNotFoundException()
        {
            // Arrange: Setup repository to return null (as User?) for any user ID.
            _userRepositoryMock
                .Setup(repo => repo.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);

            // Act & Assert: Expect an exception when the user is not found.
            await Assert.ThrowsAsync<Exception>(() => _userService.GetUserProfileAsync(1, CancellationToken.None));

            // Verify that a warning log is written when the user is not found.
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("User with ID 1 not found")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task GetUserProfileAsync_UserFound_ReturnsProfileDto()
        {
            // Arrange: Create a sample user.
            var user = new User
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                Role = "Student"
            };

            // Setup repository to return the sample user.
            _userRepositoryMock.Setup(repo => repo.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                               .ReturnsAsync(user);

            // Act: Call the service method.
            var profileDto = await _userService.GetUserProfileAsync(1, CancellationToken.None);

            // Assert: Verify that the returned DTO matches the user.
            Assert.NotNull(profileDto);
            Assert.Equal(user.Id, profileDto.Id);
            Assert.Equal(user.FirstName, profileDto.FirstName);
            Assert.Equal(user.LastName, profileDto.LastName);
            Assert.Equal(user.Email, profileDto.Email);
            Assert.Equal(user.Role, profileDto.Role);
        }

        #endregion

        #region UpdateUserProfileAsync Tests

        [Fact]
        public async Task UpdateUserProfileAsync_UserExists_ReturnsTrue()
        {
            // Arrange: Create a sample user and an update DTO.
            var user = new User
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                Role = "Student"
            };

            // Using positional constructor for record UpdateProfileDto
            var updateDto = new UpdateProfileDto("Jane", "Smith");

            _userRepositoryMock.Setup(repo => repo.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                               .ReturnsAsync(user);
            _userRepositoryMock.Setup(repo => repo.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
                               .Returns(Task.CompletedTask);

            // Act: Update the profile.
            var result = await _userService.UpdateUserProfileAsync(1, updateDto, CancellationToken.None);

            // Assert: The update should succeed and user's properties should be updated.
            Assert.True(result);
            Assert.Equal("Jane", user.FirstName);
            Assert.Equal("Smith", user.LastName);
        }

        [Fact]
        public async Task UpdateUserProfileAsync_UserNotFound_ReturnsFalse()
        {
            // Arrange: Repository returns null.
            _userRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                               .ReturnsAsync((User?)null);

            var updateDto = new UpdateProfileDto("Jane", "Smith");

            // Act: Try to update.
            var result = await _userService.UpdateUserProfileAsync(1, updateDto, CancellationToken.None);

            // Assert: Should return false.
            Assert.False(result);
        }

        #endregion

        #region ChangePasswordAsync Tests

        [Fact]
        public async Task ChangePasswordAsync_SuccessfulChange_ReturnsTrue()
        {
            // Arrange: Create a sample user with a known password.
            var originalPassword = "oldpassword";
            var newPassword = "newpassword";
            var user = new User
            {
                Id = 1,
                PasswordHash = Utilities.Security.PasswordHelper.HashPassword(originalPassword)
            };

            // Using positional constructor for record ChangePasswordDto.
            var changeDto = new ChangePasswordDto(originalPassword, newPassword);

            _userRepositoryMock.Setup(repo => repo.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                               .ReturnsAsync(user);
            _userRepositoryMock.Setup(repo => repo.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
                               .Returns(Task.CompletedTask);

            // Act: Change the password.
            var result = await _userService.ChangePasswordAsync(1, changeDto, CancellationToken.None);

            // Assert: The change should succeed.
            Assert.True(result);
            Assert.True(Utilities.Security.PasswordHelper.VerifyPassword(newPassword, user.PasswordHash));
        }

        [Fact]
        public async Task ChangePasswordAsync_WrongOldPassword_ReturnsFalse()
        {
            // Arrange: Create a user with a specific password.
            var user = new User
            {
                Id = 1,
                PasswordHash = Utilities.Security.PasswordHelper.HashPassword("oldpassword")
            };

            // Using positional constructor for ChangePasswordDto.
            var changeDto = new ChangePasswordDto("wrongpassword", "newpassword");

            _userRepositoryMock.Setup(repo => repo.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                               .ReturnsAsync(user);

            // Act: Attempt to change the password.
            var result = await _userService.ChangePasswordAsync(1, changeDto, CancellationToken.None);

            // Assert: The change should fail.
            Assert.False(result);
        }

        [Fact]
        public async Task ChangePasswordAsync_UserNotFound_ReturnsFalse()
        {
            // Arrange: Repository returns null.
            _userRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                               .ReturnsAsync((User?)null);

            var changeDto = new ChangePasswordDto("oldpassword", "newpassword");

            // Act.
            var result = await _userService.ChangePasswordAsync(1, changeDto, CancellationToken.None);

            // Assert.
            Assert.False(result);
        }

        #endregion

        #region DeleteUserAsync Tests

        [Fact]
        public async Task DeleteUserAsync_UserExists_ReturnsTrue()
        {
            // Arrange: Create a sample user.
            var user = new User { Id = 1 };

            _userRepositoryMock.Setup(repo => repo.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                               .ReturnsAsync(user);
            _userRepositoryMock.Setup(repo => repo.DeleteAsync(1, It.IsAny<CancellationToken>()))
                               .Returns(Task.CompletedTask);

            // Act: Delete the user.
            var result = await _userService.DeleteUserAsync(1, CancellationToken.None);

            // Assert: Deletion should succeed.
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteUserAsync_UserNotFound_ReturnsFalse()
        {
            // Arrange: Repository returns null.
            _userRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                               .ReturnsAsync((User?)null);

            // Act.
            var result = await _userService.DeleteUserAsync(1, CancellationToken.None);

            // Assert: Should return false.
            Assert.False(result);
        }

        #endregion

        #region GetAllUsersAsync Tests

        [Fact]
        public async Task GetAllUsersAsync_ReturnsUserList()
        {
            // Arrange: Create a list of sample users.
            var users = new List<User>
            {
                new User { Id = 1, Email = "user1@example.com", Role = "Student" },
                new User { Id = 2, Email = "user2@example.com", Role = "Instructor" }
            };

            _userRepositoryMock.Setup(repo => repo.GetAllUsersAsync(It.IsAny<CancellationToken>()))
                               .ReturnsAsync(users);

            // Act.
            var result = await _userService.GetAllUsersAsync(CancellationToken.None);

            // Assert.
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetAllUsersAsync_ReturnsEmptyList()
        {
            // Arrange: Return an empty list.
            var users = new List<User>();
            _userRepositoryMock.Setup(repo => repo.GetAllUsersAsync(It.IsAny<CancellationToken>()))
                               .ReturnsAsync(users);

            // Act.
            var result = await _userService.GetAllUsersAsync(CancellationToken.None);

            // Assert.
            Assert.Empty(result);
        }

        #endregion

        #region SetUserRoleAsync Tests

        [Fact]
        public async Task SetUserRoleAsync_UserExists_ReturnsTrue()
        {
            // Arrange: Create a sample user.
            var user = new User { Id = 1, Role = "Student" };

            _userRepositoryMock.Setup(repo => repo.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                               .ReturnsAsync(user);
            _userRepositoryMock.Setup(repo => repo.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
                               .Returns(Task.CompletedTask);

            // Act: Update the user's role.
            var result = await _userService.SetUserRoleAsync(1, "Admin", CancellationToken.None);

            // Assert: Verify update success and role change.
            Assert.True(result);
            Assert.Equal("Admin", user.Role);
        }

        [Fact]
        public async Task SetUserRoleAsync_UserNotFound_ReturnsFalse()
        {
            // Arrange: Repository returns null.
            _userRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                               .ReturnsAsync((User?)null);

            // Act: Attempt to update the role.
            var result = await _userService.SetUserRoleAsync(1, "Admin", CancellationToken.None);

            // Assert: Should return false.
            Assert.False(result);
        }

        #endregion

        #region Cancellation Tests

        [Fact]
        public async Task GetUserProfileAsync_CancellationRequested_ThrowsTaskCanceledException()
        {
            // Arrange: Create a CancellationTokenSource and cancel immediately.
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();

            // Setup repository to simulate a delay, then honor cancellation.
            _userRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                               .Returns(async (int id, CancellationToken token) =>
                               {
                                   await Task.Delay(1000, token);
                                   return new User(); // This won't execute if cancellation is honored.
                               });

            // Act & Assert: Expect TaskCanceledException due to cancellation.
            await Assert.ThrowsAsync<TaskCanceledException>(() =>
                _userService.GetUserProfileAsync(1, cancellationTokenSource.Token));
        }

        #endregion
    }
}
*/



using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.DTOs;
using Application.Services;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Application.Tests
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<ILogger<UserService>> _loggerMock;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _loggerMock = new Mock<ILogger<UserService>>();
            _userService = new UserService(_userRepositoryMock.Object, _loggerMock.Object);
        }

        #region GetUserProfileAsync Tests

        [Fact]
      
        public async Task GetUserProfileAsync_UserNotFound_ThrowsUserNotFoundException()
        {
            // Arrange: Setup repository to return null (explicitly cast to User?) for any user ID.
            _userRepositoryMock
                .Setup(repo => repo.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);

            // Act & Assert: Expect an exception when the user is not found.
            await Assert.ThrowsAsync<Exception>(() => _userService.GetUserProfileAsync(1, CancellationToken.None));

            // Verify that a warning log is written when the user is not found.
            // Use It.Is<It.IsAnyType> to match the state parameter regardless of its underlying type.
            _loggerMock.Verify(
    x => x.Log(
        LogLevel.Warning,
        It.IsAny<EventId>(),
        It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("User with ID 1 not found")),
        null,
        It.IsAny<Func<It.IsAnyType, Exception?, string>>() // Use Exception? here
    ),
    Times.Once
);
        }


        [Fact]
        public async Task GetUserProfileAsync_UserFound_ReturnsProfileDto()
        {
            var user = new User
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                Role = "Student"
            };

            _userRepositoryMock.Setup(repo => repo.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                               .ReturnsAsync(user);

            var profileDto = await _userService.GetUserProfileAsync(1, CancellationToken.None);

            Assert.NotNull(profileDto);
            Assert.Equal(user.Id, profileDto.Id);
            Assert.Equal(user.FirstName, profileDto.FirstName);
            Assert.Equal(user.LastName, profileDto.LastName);
            Assert.Equal(user.Email, profileDto.Email);
            Assert.Equal(user.Role, profileDto.Role);
        }

        #endregion

        #region UpdateUserProfileAsync Tests

        [Fact]
        public async Task UpdateUserProfileAsync_UserExists_ReturnsTrue()
        {
            var user = new User
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                Role = "Student"
            };

            var updateDto = new UpdateProfileDto("Jane", "Smith");

            _userRepositoryMock.Setup(repo => repo.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                               .ReturnsAsync(user);
            _userRepositoryMock.Setup(repo => repo.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
                               .Returns(Task.CompletedTask);

            var result = await _userService.UpdateUserProfileAsync(1, updateDto, CancellationToken.None);

            Assert.True(result);
            Assert.Equal("Jane", user.FirstName);
            Assert.Equal("Smith", user.LastName);
        }

        [Fact]
        public async Task UpdateUserProfileAsync_UserNotFound_ReturnsFalse()
        {
            _userRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                               .ReturnsAsync((User?)null);

            var updateDto = new UpdateProfileDto("Jane", "Smith");

            var result = await _userService.UpdateUserProfileAsync(1, updateDto, CancellationToken.None);

            Assert.False(result);
        }

        #endregion
    }
}
