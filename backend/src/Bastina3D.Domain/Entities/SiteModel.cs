using System;
using System.Collections.Generic;
using System.Text;

namespace Bastina3D.Domain.Entities
{
    public class SiteModel3D
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid SiteId { get; set; }
        public string Url { get; set; } = default!;       // .glb/.gltf in R2/S3 or wwwroot
        public double Scale { get; set; } = 1;
        public double HeadingDeg { get; set; }
        public double AltitudeOffsetM { get; set; }
        public long? FileSizeBytes { get; set; }          // lets the frontend decide on lazy-loading
    }
}
