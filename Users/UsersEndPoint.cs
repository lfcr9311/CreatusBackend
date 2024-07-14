using System.Globalization;
using System.Text;
using CreatusBackend.Data;
using CreatusBackend.Services;
using CreatusBackend.Users;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;


public static class UsersEndPoint
{
    public static void RegisterUsersEndPoint(this WebApplication app)
    {
        
        var endPoint = app.MapGroup("/users");
        endPoint.MapPost("", async (AddUserReq request, AppDbContext context) =>
        {
            try
            {
                var hasUser = await context.Users
                    .AnyAsync(user => user.Name == request.Name);
                if (hasUser)
                {
                    return Results.BadRequest("User already exists");
                }

                var newUser = new User(request.Name, request.Email,
                    request.Password, request.Level);
                await context.Users.AddAsync(newUser);
                await context.SaveChangesAsync();
                return Results.Created($"/user/{newUser.Id}", new
                {
                    id = newUser.Id,
                    message = "User created successfully"
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        });

        endPoint.MapGet("list", async (AppDbContext context) =>
        {
            try
            {
                var users = await context.Users.ToListAsync();
                return Results.Ok(users);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

        });
        
        endPoint.MapGet("{id}", async (Guid id, AppDbContext context) =>
        {
            try
            {
                var user = await context.Users.FindAsync(id);
                if (user == null)
                {
                    return Results.NotFound();
                }

                return Results.Ok(user);

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        });
        
        endPoint.MapDelete("{id}", async (Guid id, AppDbContext context) =>
        {
            try
            {
                var user = await context.Users.FindAsync(id);
                if (user == null)
                {
                    return Results.NotFound();
                }

                context.Users.Remove(user);
                await context.SaveChangesAsync();
                return Results.Ok(new { message = "User deleted successfully" });
            }
            catch (Exception e)
            {
                return Results.BadRequest(e.Message);
            }

        });


        endPoint.MapPut("{id}", async (Guid id, UpdateUserReq request, AppDbContext context) =>
        {
            var user = await context.Users.FindAsync(id);

            try
            {
                if (user == null)
                {
                    return Results.NotFound();
                }

                user.Name = request.Name;
                user.Email = request.Email;
                user.Password = request.Password;
                user.Level = request.Level;
                await context.SaveChangesAsync();
                return Results.Ok(new { message = "User updated successfully" });
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

        });

        
        endPoint.MapPost("/login", async (UserLoginReq login, AppDbContext context, AuthToken authToken) =>
        {
            try
            {
                var user = await context.Users
                    .FirstOrDefaultAsync(u => u.Email == login.Email 
                                              && u.Password == login.Password);
                if (user == null)
                {
                    return Results.NotFound("User not found");
                }

                var token = authToken.GenerateToken(user);
                return Results.Ok(new { token });
            }
            catch (Exception e)
            {
                return Results.BadRequest(e.Message);
            }
        });
        
        endPoint.MapGet("/report", async (AppDbContext context) =>
        {
            
            try
            {
                var users = await context.Users.ToListAsync();
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Delimiter = ",",
                    HasHeaderRecord = true,
                };

                using (var memoryStream = new MemoryStream())
                using (var streamWriter = new StreamWriter(memoryStream, Encoding.UTF8))
                using (var csvWriter = new CsvWriter(streamWriter, config))
                {
                    csvWriter.WriteRecords(users);
                    streamWriter.Flush();
                    memoryStream.Position = 0;
                    var fileBytes = memoryStream.ToArray();
                    return Results.File(fileBytes, "text/csv", "users.csv");
                }
            }
            catch (Exception e)
            {
                return Results.BadRequest(e.Message);
            }
        });
    }
}