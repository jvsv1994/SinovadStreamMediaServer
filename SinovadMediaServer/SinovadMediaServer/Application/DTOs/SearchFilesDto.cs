using System.Collections.Generic;

#nullable disable

namespace SinovadMediaServer.Application.DTOs
{
    public class SearchFilesDto
    {
        public List<LibraryDto> ListLibraries { get; set; }
        public string LogIdentifier { get; set; }

    }
}
