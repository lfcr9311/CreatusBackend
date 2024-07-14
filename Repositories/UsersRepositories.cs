using System.Globalization;
using System.Text;
using CreatusBackend.Data;
using CreatusBackend.Services;
using CreatusBackend.Users;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using PdfSharpCore.Drawing;
using PdfSharpCore.Drawing.Layout;
using PdfSharpCore.Pdf;


public static class UsersRepositories
{
    public static void RegisterUsersEndPoint(this WebApplication app)
    {

        var endPoint = app.MapGroup("/users");
        endPoint.MapPost("", async (AddUserReq request, AppDbContext context) =>
        {
            try
            {
                var hasUser = await context.Users
                    .AnyAsync(user => user.Name == request.Name || user.Email == request.Email);
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
                return Results.BadRequest();
            }
        });

        endPoint.MapGet("/list", async (AppDbContext context) =>
        {
            try
            {
                var users = await context.Users.ToListAsync();
                if(users.Count == 0)
                {
                    return Results.NotFound();
                }
                return Results.Ok(users);
            }
            catch (Exception e)
            {
                return Results.BadRequest();
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
                return Results.BadRequest();
            }
        });

        endPoint.MapDelete("{id}", async (Guid id, AppDbContext context, HttpContext httpContext) =>
        {
            var user = httpContext.User;
            if (!user.Identity.IsAuthenticated)
            {
                return Results.Unauthorized();
            }

            var levelClaim = user.Claims.FirstOrDefault(c => c.Type == "level");
            if (levelClaim == null || !int.TryParse(levelClaim.Value, out int userLevel))
            {
                return Results.Forbid();
            }

            try
            {
                if (userLevel < 4)
                {
                    return Results.Forbid();
                }

                var userToDelete = await context.Users.FindAsync(id);
                if (userToDelete == null)
                {
                    return Results.NotFound();
                }

                context.Users.Remove(userToDelete);
                await context.SaveChangesAsync();
                return Results.Ok(new { message = "User deleted successfully" });
            }
            catch (Exception e)
            {
                return Results.BadRequest(e.Message);
            }
        });

        endPoint.MapPut("{id}", async (Guid id, UpdateUserReq request, AppDbContext context, HttpContext httpContext) =>
        {
            try
            {
                // Obtém o ID do usuário a partir do token JWT
                var userIdFromToken = httpContext.User.FindFirst("id")?.Value;

                if (userIdFromToken == null)
                {
                    return Results.Unauthorized();
                }

                // Converte o ID do token para Guid
                var userId = Guid.Parse(userIdFromToken);

                // Verifica se o ID do usuário no token corresponde ao ID do perfil que está sendo atualizado
                if (userId != id)
                {
                    return Results.Forbid();
                }

                var user = await context.Users.FindAsync(id);

                if (user == null)
                {
                    return Results.NotFound();
                }

                user.Name = request.Name;
                user.Email = request.Email;
                user.Password = request.Password;
                await context.SaveChangesAsync();

                return Results.Ok(new { message = "User updated successfully" });
            }
            catch (Exception e)
            {
                return Results.BadRequest();
            }
        });



        endPoint.MapPut("level/{id}", async (Guid id, UserLevelReq request, AppDbContext context, HttpContext httpContext) =>
        {
            var user = httpContext.User;
            if (!user.Identity.IsAuthenticated)
            {
                return Results.Unauthorized();
            }
            var levelClaim = user.Claims.FirstOrDefault(c => c.Type == "level");
            if (levelClaim == null || !int.TryParse(levelClaim.Value, out int userLevel))
            {
                return Results.Forbid();
            }

            try
            {
                if (userLevel < 4)
                {
                    return Results.Forbid();
                }

                var userToUpdate = await context.Users.FindAsync(id);
                
                if (userToUpdate == null)
                {
                    return Results.NotFound();
                }

                userToUpdate.Level = request.Level;
                await context.SaveChangesAsync();

                return Results.Ok(new { message = "Nível do usuário atualizado com sucesso" });
            }
            catch (Exception e)
            {
                return Results.BadRequest(e.Message);
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
                    var level = user.Level;
                    return Results.Ok(new { token });
                }
                catch (Exception e)
                {
                    return Results.BadRequest(e.Message);
                }
            });

                endPoint.MapGet("/report", async (AppDbContext context, HttpContext httpContext) =>
        {
            var user = httpContext.User;
            if (!user.Identity.IsAuthenticated)
            {
                return Results.Unauthorized();
            }

            var levelClaim = user.Claims.FirstOrDefault(c => c.Type == "level");
            if (levelClaim == null || !int.TryParse(levelClaim.Value, out int userLevel))
            {
                return Results.Forbid();
            }

            try
            {
                if (userLevel < 4)
                {
                    return Results.Forbid();
                } 
                var users = await context.Users.ToListAsync();
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Delimiter = ",",
                    HasHeaderRecord = true,
                };

                var csvFilePath = "CSV/users.csv";
                if (!File.Exists(csvFilePath))
                {
                    using (var memoryStream = new MemoryStream())
                    using (var streamWriter = new StreamWriter(memoryStream, Encoding.UTF8))
                    using (var csvWriter = new CsvWriter(streamWriter, config))
                    {
                        csvWriter.WriteRecords(users);
                        streamWriter.Flush();
                        memoryStream.Position = 0;
                        var fileBytes = memoryStream.ToArray();
                        await File.WriteAllBytesAsync(csvFilePath, fileBytes);
                    }
                }

                using (var reader = new StreamReader(csvFilePath))
                using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)))
                {
                    var records = csv.GetRecords<dynamic>().ToList();

                    var pdfDocument = new PdfDocument();
                    var pdfPage = pdfDocument.AddPage();
                    pdfPage.Orientation = PdfSharpCore.PageOrientation.Landscape;
                    var graphics = XGraphics.FromPdfPage(pdfPage);
                    var textFormatter = new XTextFormatter(graphics);

                    var font = new XFont("Arial", 12, XFontStyle.Regular);
                    var margin = 40;
                    var currentY = margin;

                    foreach (var record in records)
                    {
                        StringBuilder sb = new StringBuilder();
                        foreach (KeyValuePair<string, object> pair in record)
                        {
                            sb.Append($"{pair.Key}: {pair.Value} ");
                        }
                        textFormatter.DrawString(sb.ToString(), font, XBrushes.Black, new XRect(margin, currentY, pdfPage.Width - 2 * margin, pdfPage.Height - 2 * margin), XStringFormats.TopLeft);
                        currentY += 20;
                    }

                    var pdfFilePath = "CSV/users.pdf";
                    pdfDocument.Save(pdfFilePath);

                    return Results.Ok(new { message = "Arquivo CSV e PDF gerados com sucesso.", csvFilePath, pdfFilePath });
                }
            }
            catch (Exception e)
            {
                return Results.BadRequest(e.Message);
            }
        });
    }
}