namespace RayvarzResend.Web.RuleEngine.Executor;

/// <summary>تقویم تعطیلات برای AddDateForHolidays — معادل GetHolidays_ByDate در Sara.</summary>
public interface IMember1388HolidayCalendar
{
    /// <summary>true اگر تاریخ شمسی yyyy/MM/dd تعطیل باشد.</summary>
    bool IsHoliday(string shamsiDate);
}

/// <summary>بدون تعطیل — هر روز کاری شمرده می‌شود.</summary>
public sealed class EmptyMember1388HolidayCalendar : IMember1388HolidayCalendar
{
    public static readonly EmptyMember1388HolidayCalendar Instance = new();
    public bool IsHoliday(string shamsiDate) => false;
}
