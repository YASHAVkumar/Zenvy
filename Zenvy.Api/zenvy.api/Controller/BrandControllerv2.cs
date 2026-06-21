using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using zenvy.application.DTOs.Brands;
using zenvy.Application.Interfaces.Services;

namespace zenvy.Api.Controllers.V1;

[Authorize]
[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/brands")]
public class BrandsV2Controller : ControllerBase
{
    private readonly IBrandService _brandService;

    public BrandsV2Controller(IBrandService brandService)
    {
        _brandService = brandService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _brandService.GetAllBrandsAsync());
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var brand = await _brandService.GetBrandByIdAsync(id);

        if (brand == null)
            return NotFound();

        return Ok(brand);
    }

    [HttpPost]
    public async Task<IActionResult> Create(BrandRequest request)
    {
        var id = await _brandService.CreateBrandAsync(request);

        return Ok(new { BrandId = id });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
        int id,
        UpdateBrandRequest request)
    {
        var success = await _brandService.UpdateBrandAsync(id, request);

        return Ok(new { Success = success });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _brandService.DeleteBrandAsync(id);

        return Ok(new { Success = success });
    }
}