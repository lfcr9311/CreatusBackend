using CreatusBackend.Data;

namespace CreatusBackend.Users;

public record AddUserReq(string Name, string Email, string Password, int Level);
