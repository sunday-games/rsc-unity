using Unity.Jobs.LowLevel.Unsafe;

namespace SG
{
    public static class Multithreading
    {
        public static void Setup(bool use)
        {
            if (use)
                JobsUtility.ResetJobWorkerCount();
            else
                JobsUtility.JobWorkerCount = 0;

            Log.Info("JobWorkerCount: " + JobsUtility.JobWorkerCount);
        }
    }
}