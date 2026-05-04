using db_final_proj.Data;
using db_final_proj.Models;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace db_final_proj.Services;

public class AuthService
{
    private readonly GameDbContext _context;

    public AuthService(GameDbContext context)
    {
        _context = context;
    }

    public async Task<(bool Success, string Message, User? User)> RegisterAsync(string username, string email, string password)
    {
        if (await _context.Users.AnyAsync(u => u.Username == username))
            return (false, "Username already exists.", null);

        var user = new User
        {
            Username = username,
            UserEmail = email,
            // BCrypt handles the salt automatically
            PassHash = BCrypt.Net.BCrypt.HashPassword(password)
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return (true, "Registration successful!", user);
    }

    public async Task<(bool Success, string Message, User? User)> LoginAsync(string username, string password)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        
        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PassHash))
        {
            return (false, "Invalid username or password.", null);
        }

        return (true, "Login successful!", user);
    }
}