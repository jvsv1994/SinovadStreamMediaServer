using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#nullable disable

namespace SinovadMediaServer.Application.DTOs
{
    public class TranscodingProcessDto
    {
        public int Id { get; set; }
        public Guid RequestGuid { get; set; }
        public int SystemProcessIdentifier { get; set; }
        public int? AdditionalSystemProcessIdentifier { get; set; }
        public string GeneratedTemporaryFolder { get; set; } = null!;
        public int MediaServerId { get; set; }
        public int VideoId { get; set; }
        public bool PendingDeletion { get; set; }
        public DateTime Created { get; set; }

    }
}
