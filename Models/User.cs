using System.ComponentModel.DataAnnotations;

public class User {
    public Guid Id { get; init; } = Guid.NewGuid();
    
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string Name { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; set; }

    [Range(0, 5)]
    public int Level { get; set; }
    
    public User(string name, string email, string password, int level)
    {
        Name = name;
        Email = email;
        Password = password;
        Level = level;
    }
    
 
}