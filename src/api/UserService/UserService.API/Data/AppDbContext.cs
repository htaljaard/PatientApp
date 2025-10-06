using System;

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

using UserService.API.Domain;

namespace UserService.API.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{

}
