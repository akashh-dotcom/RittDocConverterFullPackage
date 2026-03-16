namespace R2Utilities.DataAccess
{
    public enum TrialNotice
    {
        First = 9,
        Second = 3,
        Final = 0,
        Extension = -1
    }

    public static class TrialNoticeExtensions
    {
        public static string ToTitle(this TrialNotice trialNotice)
        {
            switch (trialNotice)
            {
                case TrialNotice.First:
                    return "9 day Trial Notification";
                case TrialNotice.Second:
                    return "3 day Trial Notification";
                case TrialNotice.Final:
                    return "Final Trial Notification";
                case TrialNotice.Extension:
                    return "Extension Trial Notification";
            }

            return null;
        }
    }
}