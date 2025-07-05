using System;
using System.Collections.Generic;

namespace client.Models;

public partial class User
{
    public int UserId { get; set; }

    public string Firstname { get; set; } = null!;

    public string Lastname { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string Salt { get; set; } = null!;

    public string? Nickname { get; set; }

    public bool? Admin { get; set; }

    public virtual ICollection<Message> MessageReceivers { get; set; } = new List<Message>();

    public virtual ICollection<Message> MessageSenders { get; set; } = new List<Message>();

    public virtual ICollection<Pticket> Ptickets { get; set; } = new List<Pticket>();
}
