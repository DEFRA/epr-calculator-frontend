using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Helpers;
using Newtonsoft.Json;

namespace EPR.Calculator.Frontend.Extensions
{
    public static class SessionExtensions
    {
        public static void SetObject<T>(this ISession session, string key, T value)
        {
            var json = JsonConvert.SerializeObject(value);
            session.SetString(key, json);
        }

        public static T? GetObject<T>(this ISession session, string key)
        {
            var json = session.GetString(key);
            return json == null ? default : JsonConvert.DeserializeObject<T>(json);
        }

        public static bool GetBooleanFlag(this ISession session, string key)
        {
            ArgumentNullException.ThrowIfNull(session);
            return session.Keys.Contains(key) &&
                   bool.TryParse(session.GetString(key), out var result) &&
                   result;
        }

        public static void SetBooleanFlag(this ISession session, string key, bool value)
        {
            ArgumentNullException.ThrowIfNull(session);
            session.SetString(key, value.ToString());
        }

        public static void RemoveKeyIfExists(this ISession session, string key)
        {
            ArgumentNullException.ThrowIfNull(session);

            if (session.Keys.Contains(key))
            {
                session.Remove(key);
            }
        }

        public static void ClearAllSession(this ISession session)
        {
            ARJourneySessionHelper.ClearAllFromSession(session);
            session.RemoveKeyIfExists(SessionConstants.IsSelectAll);
            session.RemoveKeyIfExists(SessionConstants.IsSelectAllPage);
        }
    }
}
