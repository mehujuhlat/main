using System;
using System.Collections.Generic;

namespace client.Models;

public partial class Event
{
    public int EventId { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public bool? Active { get; set; }

    public DateTime? Date { get; set; }

    public string? Program { get; set; }

    public DateTime? DateEnd { get; set; }

    public virtual ICollection<Image> Images { get; set; } = new List<Image>();

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
