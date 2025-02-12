namespace SG
{
    public static class EnvironmentVariable
    {
        public static bool TryGet(string variableName, out string variable)
        {
            variable = System.Environment.GetEnvironmentVariable(variableName);
            if (variable.IsEmpty())
            {
                Log.Warning("Fail to GetEnvironmentVariable: " + variableName);
                return false;
            }

            Log.Info("GetEnvironmentVariable " + variableName + ": " + variable);
            return true;
        }
    }
}