using System;
using System.Collections.Generic;

namespace client.Models;

public partial class Image
{
    public int ImageId { get; set; }

    public int EventId { get; set; }

    public string Url { get; set; } = null!;

    public string? Description { get; set; }

    public string? Title { get; set; }

    public int? Width { get; set; }

    public int? Height { get; set; }

    public virtual Event Event { get; set; } = null!;
}
