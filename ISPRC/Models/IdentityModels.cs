using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;
using System.Security.Principal;
using System.ComponentModel.DataAnnotations.Schema;

namespace ISPRC.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public string GivenName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }

        public string Address { get; set; }
        public string MobileNumber { get; set; }
        public string LoftLatitudeCoordinate { get; set; }
        public string LoftLongitudeCoordinate { get; set; }

        public virtual List<Bird> Birds { get; set; }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            
            // Add custom user claims here
            // userIdentity.AddClaim(new Claim("UserId", Id));

            return userIdentity;
        }
        
        public int? ClubId { get; set; }
        [ForeignKey("ClubId")]
        public Club Club { get; set; }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public DbSet<Race> Races { get; set; }
        public DbSet<Bird> Birds { get; set; }
        public DbSet<BirdRace> BirdsRace { get; set; }
        public DbSet<ReleasePoint> ReleasePoints { get; set; }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        public System.Data.Entity.DbSet<ISPRC.Models.Club> Clubs { get; set; }
    }

    /* supposedly custom claim
    public static class IdentityExtensions
    {
        public static string GetId(this IIdentity identity)
        {
            var claim = ((ClaimsIdentity)identity).FindFirst("UserId");
            // Test for null to avoid issues during local testing
            return (claim != null) ? claim.Value : string.Empty;
        }
    }
    */
}