using System;
using System.Collections.Generic;
using System.Text;

namespace Bastina3D.Domain.Entities
{
    public class SiteMedia
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid SiteId { get; set; }
        public string Url { get; set; } = default!;
        public string? Caption { get; set; }
        public string? Attribution { get; set; }          // Wikimedia Commons licenses require this
        public string? License { get; set; }
    }
}
