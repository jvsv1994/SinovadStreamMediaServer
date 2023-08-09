namespace SinovadMediaServer.Application.Builder
{
    public class SearchMediaLogBuilder
    {
        private List<SearchMediaLog> _listSearchMediaLogs;

        public SearchMediaLogBuilder()
        {
            _listSearchMediaLogs = new List<SearchMediaLog>();
        }

        public void resetMediaLogs()
        {
            _listSearchMediaLogs = new List<SearchMediaLog>();
        }

        public SearchMediaLog getSearchMediaLog(string identifier)
        {
            var searchMediaLog = _listSearchMediaLogs.Find(item => item._logIdentifier == identifier);
            return searchMediaLog;
        }
        public void AddSearchMediaLog(SearchMediaLog searchMediaLog)
        {
            _listSearchMediaLogs.Add(searchMediaLog);
        }
        public void RemoveSearchMediaLog(string logIdentifier)
        {
            var searchMediaLog = _listSearchMediaLogs.Find(item => item._logIdentifier == logIdentifier);
            _listSearchMediaLogs.Remove(searchMediaLog);
        }

    }
}
