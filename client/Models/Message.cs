using System;
using System.Collections.Generic;

namespace client.Models;

public partial class Message
{
    public int MessageId { get; set; }

    public int SenderId { get; set; }

    public int? ReceiverId { get; set; }

    public string? Message1 { get; set; }

    public DateTime? Date { get; set; }

    public bool Private { get; set; }

    public bool NewMsg { get; set; }

    public virtual User? Receiver { get; set; }

    public virtual User Sender { get; set; } = null!;
}
