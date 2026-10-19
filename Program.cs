using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var users = new List<User>
{
    new() { Id = 1, Name = "Alice", Age = 25 },
    new() { Id = 2, Name = "Bob", Age = 30 },
    new() { Id = 3, Name = "Charlie", Age = 22 },
    new() { Id = 4, Name = "David", Age = 28 }
};

app.MapPost("/users", ([FromBody] User newUser) =>
{
    newUser.Id = users.Count > 0 ? users.Max(u => u.Id) + 1 : 1;
    users.Add(newUser);
    return Results.Created($"/users/{newUser.Id}", newUser);
});

app.MapDelete("/users/{id:int}", (int id) =>
{
    var user = users.FirstOrDefault(u => u.Id == id);
    if (user == null) return Results.NotFound();
    
    users.Remove(user);
    return Results.NoContent();
});

app.MapPut("/users/{id:int}", (int id, [FromBody] User updatedUser) =>
{
    var user = users.FirstOrDefault(u => u.Id == id);
    if (user == null) return Results.NotFound();
    
    user.Name = updatedUser.Name;
    user.Age = updatedUser.Age;
    return Results.Ok(user);
});

app.MapGet("/users", (
    [FromQuery] string? name,
    [FromQuery] string? sortBy,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10) =>
{
    var query = users.AsQueryable();

    if (!string.IsNullOrWhiteSpace(name))
    {
        query = query.Where(u => u.Name.Contains(name, StringComparison.OrdinalIgnoreCase));
    }

    if (!string.IsNullOrWhiteSpace(sortBy))
    {
        query = sortBy.ToLower() switch
        {
            "name" => query.OrderBy(u => u.Name),
            "age" => query.OrderBy(u => u.Age),
            _ => query
        };
    }

    var totalCount = query.Count();
    var items = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

    return Results.Ok(new { TotalCount = totalCount, Page = page, PageSize = pageSize, Items = items });
});

app.Run();

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
}
