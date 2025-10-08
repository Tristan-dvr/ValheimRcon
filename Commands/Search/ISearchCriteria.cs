namespace ValheimRcon.Commands.Search
{
    public interface ISearchCriteria
    {
        bool IsMatch(ZDO zdo);
    }
}
