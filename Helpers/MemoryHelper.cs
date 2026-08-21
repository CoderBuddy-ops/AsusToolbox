namespace Asus.Helpers
{
    /// <summary>
    /// Reclaims memory that the app no longer references (e.g. closed forms,
    /// charts, large images). This is an honest managed-heap collection — it
    /// does NOT fake the working-set number with SetProcessWorkingSetSize,
    /// which only pages memory out and hurts performance when it is re-faulted.
    /// </summary>
    public static class MemoryHelper
    {
        public static void TrimAfter(Task? prerequisite = null, TimeSpan? timeout = null)
        {
            Task.Run(async () =>
            {
                if (prerequisite != null)
                {
                    try
                    {
                        await prerequisite.WaitAsync(timeout ?? TimeSpan.FromSeconds(3));
                    }
                    catch { }
                }

                Trim();
            });
        }

        private static void Trim()
        {
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Optimized);
            GC.WaitForPendingFinalizers();
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Optimized);
        }
    }
}
