using Microsoft.AspNetCore.Mvc;
using WebApi.MinimalApi.Domain;
using WebApi.MinimalApi.Models;
using AutoMapper;

namespace WebApi.MinimalApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController : Controller
{
    // Чтобы ASP.NET положил что-то в userRepository требуется конфигурация

    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public UsersController(IUserRepository userRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    [HttpGet("{userId}", Name = nameof(GetUserById))]
    [Produces("application/json", "application/xml")]
    public ActionResult<UserDto> GetUserById([FromRoute] Guid userId)
    {
        var user = _userRepository.FindById(userId);
        if (user is null)
            return NotFound();
        var dto = _mapper.Map<UserDto>(user);
        return Ok(dto);
    }

    [HttpPost]
    [Produces("application/json", "application/xml")]
    public IActionResult CreateUser([FromBody] CreateUserDto user)
    {
        if (user is null)
            return BadRequest();

        if (user.Login != null && user.Login.Any(c => !char.IsLetterOrDigit(c)))
            ModelState.AddModelError("Login", "Login should contain only letters or digits");

        if (!ModelState.IsValid)
            return UnprocessableEntity(ModelState);

        var entity = _mapper.Map<UserEntity>(user);
        var createdUser = _userRepository.Insert(entity);

        return CreatedAtRoute(
            nameof(GetUserById),
            new { userId = createdUser.Id },
            createdUser.Id
        );
    }
}