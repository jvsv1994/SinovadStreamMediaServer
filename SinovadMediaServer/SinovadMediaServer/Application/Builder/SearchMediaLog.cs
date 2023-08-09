namespace SinovadMediaServer.Application.Builder
{
    public class SearchMediaLog
    {
        public List<string> _textLines { get; set; }
        public string _logIdentifier { get; set; }

        public SearchMediaLog(string logIdentifier)
        {
            _logIdentifier = logIdentifier;
            _textLines = new List<string>();
        }

        public string GetLogWithSeparationLines()
        {
            if (_textLines == null || _textLines.Count == 0)
            {
                return "";
            }
            else if (_textLines.Count == 1)
            {
                return _textLines[0];
            }
            else
            {
                return _textLines.Aggregate((accum, current) => accum + "<br>" + current);
            }
        }

    }
}
