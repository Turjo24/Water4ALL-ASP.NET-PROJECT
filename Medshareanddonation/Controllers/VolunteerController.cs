using Medshareanddonation.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

[Route("api/[controller]")]
[ApiController]
public class VolunteerController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;

    public VolunteerController(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager)
    {
        _dbContext = dbContext;
        _userManager = userManager;
    }

    // Existing endpoint you showed (kept for reference)
    [HttpGet("VolunteerInfo/{volunteerId}")]
    [Authorize]
    public async Task<ActionResult<object>> GetVolunteerInfo(string volunteerId)
    {
        try
        {
            if (string.IsNullOrEmpty(volunteerId) || volunteerId == "NULL")
            {
                return NotFound("No volunteer assigned");
            }

            var volunteer = await _userManager.Users
                .Where(u => u.Id == volunteerId && u.Role == "Volunteer")
                .Select(u => new
                {
                    Id = u.Id,
                    Name = u.name,
                    Email = u.Email
                })
                .FirstOrDefaultAsync();

            if (volunteer == null)
            {
                return NotFound("Volunteer not found");
            }

            return Ok(volunteer);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error fetching volunteer info", error = ex.Message });
        }
    }

    // NEW: Get all donation requests assigned to a volunteer

    [HttpGet("{volunteerId}/AssignedDonations")]
    [Authorize] 
    public async Task<ActionResult<IEnumerable<object>>> GetAssignedDonations(string volunteerId)
    {
        try
        {
            if (string.IsNullOrEmpty(volunteerId) || volunteerId == "NULL")
                return NotFound("No volunteer specified");

            // verify that user exists and is a Volunteer
            var volunteerExists = await _userManager.Users
                .AnyAsync(u => u.Id == volunteerId && u.Role == "Volunteer");

            if (!volunteerExists)
                return NotFound("Volunteer not found");

            // get all donation requests assigned to this volunteer
            var donations = await _dbContext.DonationRequests
                .AsNoTracking()
                .Where(d => d.AssignedVolunteerId == volunteerId)
                .OrderByDescending(d => d.CreatedAt)
                .Select(d => new
                {
                    d.Id,
                    d.Name,                // requester name
                    d.PhoneNumber,
                    d.LocationAddress,
                    d.WaterLiters,
                    d.Reason,
                    d.Status,
                    d.CreatedAt
                })
                .ToListAsync();

            return Ok(donations);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error fetching assigned donations", error = ex.Message });
        }
    }


    // GET: api/Volunteer/AllReport
    [HttpGet("AllReport")]
    [AllowAnonymous]  
    public async Task<ActionResult<IEnumerable<object>>> AllRequests()
    {
        var requests = await _dbContext.DonationRequests
            .AsNoTracking()  // readonly
            .Where(d => d.Status == "Approved")
            .Select(d => new
            {
                d.Id,
                d.Name,
                d.PhoneNumber,
                d.LocationAddress,
                d.WaterLiters,
                d.Reason,
                d.Status,
                AssignedVolunteerId = d.AssignedVolunteerId,
                d.CreatedAt
            })
            .ToListAsync();

        return Ok(requests);
    }





}
