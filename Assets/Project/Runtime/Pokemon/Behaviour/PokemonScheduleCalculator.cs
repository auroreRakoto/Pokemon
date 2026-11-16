
/// <summary>
/// Pure schedule calculations used by an individual PokemonAgent.
/// Unity time acquisition and schedule state remain owned by PokemonAgent.
/// </summary>
public static class PokemonScheduleCalculator
{
    /// <summary>
    /// Calculates the individual's local hour from the supplied Unity time.
    /// Returns false when the day duration is invalid so the caller can
    /// preserve its previously stored hour, matching the original behaviour.
    /// </summary>
    public static bool TryCalculateCurrentHour(
        float timeNow,
        float secondsPerDay,
        float dayStartHour,
        float individualScheduleOffset,
        out float currentHour
    )
    {
        currentHour = 0f;

        if (secondsPerDay <= 0f)
            return false;

        float elapsedDayFraction =
            (timeNow / secondsPerDay) % 1f;

        currentHour =
            (dayStartHour +
             elapsedDayFraction * 24f +
             individualScheduleOffset + 24f) % 24f;

        return true;
    }

    /// <summary>
    /// Determines whether the supplied hour is within the individual's
    /// configured active period.
    /// </summary>
    public static bool IsActivePeriod(
        PokemonAgent.ActivityPattern activityPattern,
        float currentHour,
        float dayStartHour,
        float nightStartHour
    )
    {
        if (activityPattern == PokemonAgent.ActivityPattern.Diurnal)
        {
            return IsWithinHours(
                currentHour,
                dayStartHour,
                nightStartHour
            );
        }

        return IsWithinHours(
            currentHour,
            nightStartHour,
            dayStartHour
        );
    }

    /// <summary>
    /// Uses an inclusive start and exclusive end. Intervals crossing midnight
    /// retain the original OR-based behaviour, including equal endpoints.
    /// </summary>
    public static bool IsWithinHours(
        float hour,
        float startHour,
        float endHour
    )
    {
        if (startHour < endHour)
        {
            return hour >= startHour &&
                   hour < endHour;
        }

        return hour >= startHour ||
               hour < endHour;
    }
}
