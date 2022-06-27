#pragma warning disable CS8618
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace WeddingPlanner.Models;
public class GuestList
{
    [Key]
    public int GuestListId {get;set;}
    // Connection to the User table
    public int UserId {get;set;}
    public User? User {get;set;}
    // Connection to the Wedding table
    public int WeddingId {get;set;}
    public Wedding? Wedding {get;set;}
}