using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using zenvy.application.DTOs.Users;
using zenvy.application.Interfaces.Repositories;
using zenvy.application.Interfaces.Services;

namespace zenvy.api.Controller;
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
       var user = await userService.GetByIdAsync(Guid.Parse(id));
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
        var user = await userService.UpdateAsync(Guid.Parse(id), dto);
        return Ok(user);
    }
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        await userService.DeleteAsync(Guid.Parse(id));
        return Ok();
    }
}
