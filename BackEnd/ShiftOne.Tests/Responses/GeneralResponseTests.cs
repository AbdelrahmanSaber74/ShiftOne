using ShiftOne.Shared.Responses;

namespace ShiftOne.Tests.Responses
{
    public class GeneralResponseTests
    {
        [Fact]
        public void Ok_WithPagination_ComputesTotalPages()
        {
            var response = GeneralResponse.Ok("loaded", page: 2, pageSize: 20, totalCount: 45);

            Assert.True(response.Success);
            Assert.Equal(200, response.StatusCode);
            Assert.Equal(3, response.TotalPages);
        }

        [Fact]
        public void Unauthorized_UsesExpectedStatusCode()
        {
            var response = GeneralResponse.Unauthorized("login required");

            Assert.False(response.Success);
            Assert.Equal(401, response.StatusCode);
            Assert.Equal("login required", response.Message);
        }
    }
}
