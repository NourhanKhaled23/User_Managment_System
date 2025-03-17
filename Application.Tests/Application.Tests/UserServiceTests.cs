



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
