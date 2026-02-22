using Microsoft.AspNetCore.Mvc;
using InsureX.Domain.Entities;

namespace InsureX.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SampleController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        var policy = new Policy
        {
            Id = 1,
            PolicyNumber = "POL123456",
            HolderName = "John Doe",
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddYears(1)
        };
        return Ok(policy);
    }
}