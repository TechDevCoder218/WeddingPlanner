#pragma warning disable CS8618
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace WeddingPlanner.Models;
public class User
{
    [Key]
    [Required]
    public int UserId { get; set; }
    [Required]
    public string FirstName { get; set; }
    [Required]
    public string LastName { get; set; }
    [EmailAddress]
    [Required]
    public string Email {get;set;}
    [Required]
    [MinLength(8)]
    [DataType(DataType.Password)]
    public string Password {get; set;}
    [NotMapped]
    [Compare("Password")]
    [DataType(DataType.Password)]
    public string PassConfirm {get;set;}
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
    public List<GuestList> WeddingsAttending {get;set;} = new List<GuestList>();
}