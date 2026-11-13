using System.Globalization;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Extensions;

namespace EPR.Calculator.Frontend.Helpers
{
    /// <summary>
    /// Provides helper methods for managing a collection of producer IDs
    /// in the user's session for the AR Journey workflow.
    /// </summary>
    public static class ARJourneySessionHelper
    {
        /// <summary>
        /// The session key used to store the producer IDs.
        /// </summary>
        private const string SessionKey = SessionConstants.ProducerIds;

        /// <summary>
        /// The session key used to store producer names, keyed by producer ID.
        /// </summary>
        private const string NamesSessionKey = SessionConstants.ProducerNames;

        /// <summary>
        /// Adds the specified producer IDs to the session, ensuring uniqueness.
        /// If the session already contains producer IDs, the new IDs are merged.
        /// </summary>
        /// <param name="session">The current user's session.</param>
        /// <param name="producerIds">The producer IDs to add.</param>
        public static void AddToSession(ISession session, IEnumerable<int> producerIds)
        {
            var existing = session.GetObject<IEnumerable<int>>(SessionKey) ?? [];
            var updated = new HashSet<int>(existing);
            updated.UnionWith(producerIds);
            session.SetObject(SessionKey, updated.ToList());
        }

        /// <summary>
        /// Removes the specified producer IDs from the session.
        /// If a producer ID does not exist in the session, it is ignored.
        /// </summary>
        /// <param name="session">The current user's session.</param>
        /// <param name="producerIds">The producer IDs to remove.</param>
        public static void RemoveFromSession(ISession session, IEnumerable<int> producerIds)
        {
            var existing = session.GetObject<IEnumerable<int>>(SessionKey) ?? [];
            var updated = new HashSet<int>(existing);
            updated.ExceptWith(producerIds);
            session.SetObject(SessionKey, updated.ToList());

            var names = session.GetObject<Dictionary<int, string>>(NamesSessionKey);
            if (names != null)
            {
                foreach (var id in producerIds)
                {
                    names.Remove(id);
                }

                session.SetObject(NamesSessionKey, names);
            }
        }

        /// <summary>
        /// Records producer names against their IDs so a selected organisation
        /// can be named even when it is not on the page being viewed.
        /// Existing names for the same IDs are replaced.
        /// </summary>
        /// <param name="session">The current user's session.</param>
        /// <param name="producerNames">The producer names, keyed by producer ID.</param>
        public static void AddNamesToSession(ISession session, IEnumerable<KeyValuePair<int, string>> producerNames)
        {
            var names = session.GetObject<Dictionary<int, string>>(NamesSessionKey) ?? [];
            var changed = false;
            foreach (var (id, name) in producerNames)
            {
                if (string.IsNullOrWhiteSpace(name) || (names.TryGetValue(id, out var existing) && existing == name))
                {
                    continue;
                }

                names[id] = name;
                changed = true;
            }

            if (changed)
            {
                session.SetObject(NamesSessionKey, names);
            }
        }

        /// <summary>
        /// Retrieves the stored name for a producer ID.
        /// </summary>
        /// <param name="session">The current user's session.</param>
        /// <param name="producerId">The producer ID.</param>
        /// <returns>The producer name, or <c>null</c> if none is stored.</returns>
        public static string? GetNameFromSession(ISession session, int producerId)
        {
            var names = session.GetObject<Dictionary<int, string>>(NamesSessionKey);
            return names != null && names.TryGetValue(producerId, out var name) ? name : null;
        }

        /// <summary>
        /// Describes the current selection for display, e.g. "3 organisations selected".
        /// </summary>
        /// <param name="session">The current user's session.</param>
        /// <returns>The selection summary text.</returns>
        public static string GetSelectionSummary(ISession session)
        {
            var ids = GetFromSession(session);
            switch (ids.Count)
            {
                case 0:
                    return "No organisations selected. Use search to find and select an organisation.";
                case 1:
                    // The ID is always known; the name only if it was recorded.
                    var id = ids.First();
                    var name = GetNameFromSession(session, id);
                    return name != null
                        ? $"1 organisation selected: {name} ({id})"
                        : $"1 organisation selected: {id}";
                default:
                    return $"{ids.Count.ToString("N0", CultureInfo.GetCultureInfo("en-GB"))} organisations selected";
            }
        }

        /// <summary>
        /// Retrieves the set of producer IDs currently stored in the session.
        /// </summary>
        /// <param name="session">The current user's session.</param>
        /// <returns>
        /// A <see cref="HashSet{Int32}"/> containing the producer IDs,
        /// or an empty set if none are stored.
        /// </returns>
        public static HashSet<int> GetFromSession(ISession session)
        {
            var existing = session.GetObject<IEnumerable<int>>(SessionKey) ?? [];
            return new HashSet<int>(existing);
        }

        /// <summary>
        /// Removes all producer IDs from the session.
        /// </summary>
        /// <param name="session">The current user's session.</param>
        public static void ClearAllFromSession(ISession session)
        {
            session.Remove(SessionKey);
            session.Remove(NamesSessionKey);
        }
    }
}