using b2c_test_sample.Framework;
using System.Net;
using Xunit;

namespace b2c_test_sample
{
    [Collection(nameof(ApplicationCollection))]
    public class UnitTest1
    {
        private readonly ApplicationFixture _fixture;
        private readonly Service _service;

        public UnitTest1(ApplicationFixture fixture)
        {
            _fixture = fixture;
            _service = new Service(_fixture.Client);
        }

        [Fact]
        public async Task OpenWithoutUser()
        {
            using (var result = await _service.GetValueOpenAsync())
            {
                Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            }
        }

        [Fact]
        public async Task UserWithUser()
        {
            var user = await _fixture.CreateUserAsync();

            using (var result = await _service.GetValueUserAsync(user))
            {
                Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            }
        }

        [Fact]
        public async Task UserWithoutUser()
        {
            using (var result = await _service.GetValueUserAsync())
            {
                Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
            }
        }

        [InlineData(true, HttpStatusCode.OK)]
        [InlineData(false, HttpStatusCode.Forbidden)]
        [Theory]
        public async Task AdminWithAdminUser(bool isAdmin, HttpStatusCode expected)
        {
            var user = await _fixture.CreateUserAsync(new TestUserState { IsAdmin = isAdmin });

            using (var result = await _service.GetValueAdminAsync(user))
            {
                Assert.Equal(expected, result.StatusCode);
            }
        }

        [Fact]
        public async Task AdminWithNoUser()
        {
            using (var result = await _service.GetValueAdminAsync())
            {
                Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
            }
        }
    }
}
