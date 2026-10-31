using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InvoiceManagementSystem.API.Data;
using InvoiceManagementSystem.API.DTOs;
using InvoiceManagementSystem.API.DTOs.Customers;
using InvoiceManagementSystem.API.Models;

namespace InvoiceManagementSystem.API.Controllers;

[Route("api/customers")]
[Authorize(Roles = "Admin")]
public class CustomersController : BaseApiController
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AppDbContext _context;

    public CustomersController(
        UserManager<ApplicationUser> userManager,
        AppDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CustomerDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<IEnumerable<CustomerDto>>>> GetCustomers()
    {
        var customers = await _userManager.GetUsersInRoleAsync("Customer");

        var customerIds = customers.Select(c => c.Id).ToList();

        var invoiceCounts = await _context.Invoices
            .Where(i => customerIds.Contains(i.CustomerId))
            .GroupBy(i => i.CustomerId)
            .Select(g => new { CustomerId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.CustomerId, x => x.Count);

        var result = customers
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new CustomerDto
            {
                Id = c.Id,
                FullName = c.FullName,
                Email = c.Email ?? string.Empty,
                CreatedAt = c.CreatedAt,
                InvoiceCount = invoiceCounts.GetValueOrDefault(c.Id, 0)
            })
            .ToList();

        return OkResponse<IEnumerable<CustomerDto>>(result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<CustomerDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CustomerDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<CustomerDto>>> GetCustomerById(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null || !await _userManager.IsInRoleAsync(user, "Customer"))
        {
            return NotFoundResponse<CustomerDto>($"Klienti me ID '{id}' nuk u gjet.");
        }

        var count = await _context.Invoices.CountAsync(i => i.CustomerId == id);

        var dto = new CustomerDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            CreatedAt = user.CreatedAt,
            InvoiceCount = count
        };

        return OkResponse(dto);
    }
}

