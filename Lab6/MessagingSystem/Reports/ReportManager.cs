namespace MessagingSystem.Reports;

public abstract class ReportManager
{
    public abstract Report CreateReport(Account account, DateTime dateFrom, DateTime dateTo);
}
