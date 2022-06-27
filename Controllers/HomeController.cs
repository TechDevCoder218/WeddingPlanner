using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WeddingPlanner.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace WeddingPlanner.Controllers;

public class HomeController : Controller
{
    private MyContext _context;
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger, MyContext context)
    {
        _logger = logger;
        _context = context;
    }

    public IActionResult Index()
    {
        HttpContext.Session.Clear();
        return View();
    }

    [HttpPost("user/register")]
    public IActionResult Register(User newUser)
    {
        if(ModelState.IsValid){
            if(_context.Users.Any(a =>a.Email == newUser.Email))
            {
                ModelState.AddModelError("Email","Email is already in use");
                return View("Index");
            }
            PasswordHasher<User> Hasher = new PasswordHasher<User>();
            newUser.Password = Hasher.HashPassword(newUser, newUser.Password);
            _context.Add(newUser);
            _context.SaveChanges();
            HttpContext.Session.SetInt32("user", newUser.UserId);
            return RedirectToAction("Dashboard");
        } else{
            return View("Index");
        }
    }

    [HttpPost("user/login")]
    public IActionResult Login(LogUser loginUser)
    {
        if(ModelState.IsValid)
        {
            User userInDb = _context.Users.FirstOrDefault(a => a.Email == loginUser.LogEmail);
            if(userInDb == null)
            {
                ModelState.AddModelError("LogEmail","Invalid Login Attempt");
                return View("Index");
            }

            PasswordHasher<LogUser> hasher = new PasswordHasher<LogUser>();

            var result = hasher.VerifyHashedPassword(loginUser, userInDb.Password, loginUser.LogPassword);
            if(result == 0)
            {
                ModelState.AddModelError("LogEmail","Invalid login attempt");
                return View("Index");
            }
            HttpContext.Session.SetInt32("user", userInDb.UserId);
            return RedirectToAction("Dashboard");
        } else {
            return View("Index");
        }
    }

    [HttpGet("dashboard")]
    public IActionResult Dashboard()
    {
        if(HttpContext.Session.GetInt32("user") == null)
        {
            return RedirectToAction("Index");
        }
        ViewBag.AllWeddings = _context.Weddings.Include(m => m.GuestsInWedding).ThenInclude(d => d.User).ToList();
        ViewBag.SingleUser = _context.Users.Include(m => m.WeddingsAttending).ThenInclude(d => d.Wedding).FirstOrDefault(z => z.UserId == HttpContext.Session.GetInt32("user"));
        ViewBag.loggedInUser = HttpContext.Session.GetInt32("user");
        return View();
    }

    [HttpGet("unrsvp/{rsvpId}")]
    public IActionResult UnRsvp(int rsvpId)
    {
        GuestList RsvpToDelete = _context.GuestLists.SingleOrDefault(d => d.UserId == HttpContext.Session.GetInt32("user") && d.WeddingId == rsvpId);
        _context.GuestLists.Remove(RsvpToDelete);
        _context.SaveChanges();
        return RedirectToAction("Dashboard");
    }

    [HttpGet("deletewedding/{wedId}")]
    public IActionResult DeleteWedding(int wedId)
    {
        Wedding WeddingToDelete = _context.Weddings.SingleOrDefault(d => d.Creator == HttpContext.Session.GetInt32("user") && d.WeddingId == wedId);
        _context.Weddings.Remove(WeddingToDelete);
        _context.SaveChanges();
        return RedirectToAction("Dashboard");
    }

    [HttpGet("addrsvp/{rsvpId}")]
    public IActionResult AddRsvp(int rsvpId)
    {
        GuestList RsvpToAdd = new GuestList();
        RsvpToAdd.UserId = Convert.ToInt32(HttpContext.Session.GetInt32("user"));
        RsvpToAdd.WeddingId = rsvpId;
        _context.GuestLists.Add(RsvpToAdd);
        _context.SaveChanges();
        return RedirectToAction("Dashboard");
    }

    [HttpGet("create")]
    public IActionResult Create()
    {
        return View ();
    }

    [HttpPost("weddingadd")]
    public IActionResult WeddingAdd(Wedding newWedding)
    {
        if(ModelState.IsValid)
        {
            if(newWedding.WedDate <= DateTime.Now){
                ModelState.AddModelError("WedDate","Wedding Date is Invalid");
                return View("Create");
            }

            newWedding.Creator = Convert.ToInt32(HttpContext.Session.GetInt32("user"));
            _context.Add(newWedding);
            _context.SaveChanges();

            int latestWed = _context.Weddings.Max(a => a.WeddingId);
            return RedirectToAction("Latestwedding");
        } else {
            return View("Create");
        }
    }

    [HttpGet("showwedding/{oneWed}")]
    public IActionResult Showwedding(int oneWed)
    {
        Wedding WeddingToView = _context.Weddings.Include(d => d.GuestsInWedding).ThenInclude(c => c.User).FirstOrDefault(d => d.WeddingId == oneWed);
        string str = WeddingToView.WedderOne;
        string str2 = WeddingToView.WedderTwo;
        string firstname;
        string secondname;
        if(str.Contains(" "))
        {
            firstname = str.Remove(str.IndexOf(' '));
            WeddingToView.WedderOne = firstname;
        }

        if(str2.Contains(" "))
        {
            secondname = str2.Remove(str2.IndexOf(' '));
            WeddingToView.WedderTwo = secondname;
        }
    
        return View(WeddingToView);
    }

    [HttpGet("latestwedding")]
    public IActionResult Latestwedding()
    {
        int latestWed = _context.Weddings.Max(a => a.WeddingId);
        Wedding WeddingToView = _context.Weddings.Include(d => d.GuestsInWedding).ThenInclude(c => c.User).FirstOrDefault(d => d.WeddingId == latestWed);
        string str = WeddingToView.WedderOne;
        string str2 = WeddingToView.WedderTwo;
        string firstname;
        string secondname;
        if(str.Contains(" "))
        {
            firstname = str.Remove(str.IndexOf(' '));
            WeddingToView.WedderOne = firstname;
        }

        if(str2.Contains(" "))
        {
            secondname = str2.Remove(str2.IndexOf(' '));
            WeddingToView.WedderTwo = secondname;
        }
        
        return View("Showwedding", WeddingToView);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
