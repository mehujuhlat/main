using System;
using System.Collections.Generic;

namespace client.Models;

public partial class Pticket
{
    public int PticketId { get; set; }

    public int UserId { get; set; }

    public int TicketId { get; set; }

    public decimal? Price { get; set; }

    public DateTime? Date { get; set; }

    public string? Firstname { get; set; }

    public string? Lastname { get; set; }

    public string? Address { get; set; }

    public string? Postalcode { get; set; }

    public string? City { get; set; }

    public string? Code { get; set; }

    public bool? Valid { get; set; }

    public bool? Cancel { get; set; }

    public virtual Ticket Ticket { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
