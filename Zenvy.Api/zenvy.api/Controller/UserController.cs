using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using zenvy.application.DTOs.Users;
using zenvy.application.Interfaces.Repositories;
using zenvy.application.Interfaces.Services;

namespace zenvy.api.Controller;
[Authorize(Roles = "Admin")]
[Route("api/v{version:apiVersion}/users")]
[ApiController]
public class UserController(IUserService userService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        return Ok(await userService.GetUsersAsync());
    }

    [HttpGet("{id}")]
     public async Task<IActionResult> GetUserById(string id) {
         if (!Guid.TryParse(id, out var userId)) return BadRequest("id must be a valid GUID.");
         var user = await userService.GetByIdAsync(userId);
         if (user is null) return NotFound();
        return Ok(user);
    }

    [HttpPost]
    public async Task<IActionResult> RegisterUser([FromBody] RegisterUserDto dto)
    {
        var user = await userService.RegisterAsync(dto);
        return Ok(user);
    }
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserDto dto)
    {
        if (!Guid.TryParse(id, out var userId)) return BadRequest("id must be a valid GUID.");
        var user = await userService.UpdateAsync(userId, dto);
        if (user is null) return NotFound();
        return Ok(user);
    }
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        if (!Guid.TryParse(id, out var userId)) return BadRequest("id must be a valid GUID.");
        await userService.DeleteAsync(userId);
        return Ok();
    }
}
