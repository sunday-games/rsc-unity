using DictSO = System.Collections.Generic.Dictionary<string, object>;

namespace SG
{
    public class Result<T>
    {
        public T Value;
        public string Error;
        public bool? Executed;
        public bool Success => Executed == true;

        public Result() { }
        public Result(string error) => SetError(error);
        public Result(T value) => SetSuccess(value);

        public Result<T> SetError(string error)
        {
            Error = error;
            Executed = false;
            return this;
        }
        public Result<T> SetSuccess(T value)
        {
            Value = value;
            Executed = true;
            return this;
        }
    }

    public class Result
    {
        public static Result Success(DictSO data = null) => new Result().SetSuccess(data);
        public static Result Success(object value, string key = "value") => Success(new DictSO { [key] = value });
        public static Result Error(string error = null) => new Result().SetError(error);

        public bool success;
        public DictSO data;
        public string error;
        public object Value { get => data["value"]; set => data["value"] = value; }

#if SG_LOCALIZATION
        string _errorLocalized;
        public string errorLocalized
        {
            get
            {
                if (_errorLocalized.IsNotEmpty())
                    return _errorLocalized;
                else if (error.IsEmpty())
                    return null;
                else if (Localization.IsKey(error))
                    return error.Localize();
                else
                    return null;
            }
            set => _errorLocalized = value;
        }
#endif

        public Result() { }
        public Result(string json) => Parse(Json.Deserialize(json) as DictSO);
        public Result(DictSO json) => Parse(json);

        private const string successKey = "success";
        private const string statusKey = "status";
        private const string dataKey = "data";
        private const string resultKey = "result";
        private const string errorKey = "error";
        public const string RAW_ERROR_KEY = "raw_error";

        void Parse(DictSO input)
        {
            Log.Debug(Json.Serialize(input));

            if (input.IsValue(successKey))
            {
                success = input[successKey].ToBool();
                input.Remove(successKey);
            }
            else
            {
                success = !input.IsValue(errorKey) && !input.IsValue(RAW_ERROR_KEY);
            }

            if (input.IsValue(errorKey))
            {
                var errorData = input[errorKey] as DictSO;

#if SG_BLOCKCHAIN
                if (errorData != null && errorData.IsValue(statusKey) && errorData[statusKey].ToString() == "ABORTED")
                    error = BlockchainPlugin.Wallet.Errors.OperationCanceled;
                else
#endif
                    error = input[errorKey].ToString();
                input.Remove(errorKey);
            }

#if SG_BLOCKCHAIN
            // TODO: There should be base Result.HandleError handler and specific handler like ResultEOS.HandleError. I believe some errors are the same for any wallet.
            if (input.IsValue(RAW_ERROR_KEY) && input[RAW_ERROR_KEY].ToString().Contains("System.Net.WebException"))
            {
                error = BlockchainPlugin.Wallet.Errors.ConnectionError;
                input.Remove(RAW_ERROR_KEY);
            }
#endif

            if (input.IsValue(dataKey))
            {
                data = input[dataKey] as DictSO;
                input.Remove(dataKey);

                if (data.IsValue(resultKey))
                    data = data[resultKey] as DictSO;
            }

            if (input.Count > 0)
            {
                if (data == null)
                    data = new DictSO(input);
                else
                    foreach (var pair in input)
                        data.Add(pair.Key, pair.Value);
            }
        }

        public Result SetError(string error)
        {
            this.error = error;
            success = false;
            return this;
        }
        public Result SetSuccess(DictSO data = null)
        {
            this.data = data ?? new DictSO();
            success = true;
            return this;
        }

        public static implicit operator bool(Result r) => r != null;
    }
}