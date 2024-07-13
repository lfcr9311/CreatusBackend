namespace CreatusBackend.Users;

public class User
{
    public Guid Id { get; init; } = Guid.NewGuid();
    
    public string Name { get; set; }
    
    public string Email { get; set; }
       
    public string Password { get; set; }
       
    public int Level { get; set; }
    
    public User(string name, string email, string password, int level)
    {
        Name = name;
        Email = email;
        Password = password;
        Level = level;
    }
    
 
}