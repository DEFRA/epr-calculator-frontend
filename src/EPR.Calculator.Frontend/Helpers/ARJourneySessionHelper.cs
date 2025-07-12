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
        }
    }
}