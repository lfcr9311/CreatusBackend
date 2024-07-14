namespace CreatusBackend.Users;

public record UpdateUserReq(Guid Id, string Name, string Email, string Password);