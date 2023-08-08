using System.Collections.Generic;

#nullable disable

namespace SinovadMediaServer.DTOs
{
    public class UpdateLibraryVideosDto
    {
        public List<LibraryDto> ListLibraries { get; set; }
        public string LogIdentifier { get; set; }

    }
}
