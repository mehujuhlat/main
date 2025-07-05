using System;
using System.Collections.Generic;

namespace client.Models;

public partial class Ticket
{
    public int TicketId { get; set; }

    public int EventId { get; set; }

    public string? Description { get; set; }

    public int? Type { get; set; }

    public decimal? Price { get; set; }

    public int? Day { get; set; }

    public bool? Valid { get; set; }

    public virtual Event Event { get; set; } = null!;

    public virtual ICollection<Pticket> Ptickets { get; set; } = new List<Pticket>();
}
