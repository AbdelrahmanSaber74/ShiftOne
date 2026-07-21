namespace ShiftOne.Shared.Responses.User
{
    public class GetAllUsersResponse
    {
        public List<GetUserByIdResponse> Users { get; set; } = new();
    }
}


