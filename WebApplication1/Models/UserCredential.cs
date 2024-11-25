using System;
using System.Collections.Generic;

namespace WebApplication1.Models;

public partial class UserCredential
{
    public int UserId { get; set; }

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public bool IsAuthorized { get; set; }
}
