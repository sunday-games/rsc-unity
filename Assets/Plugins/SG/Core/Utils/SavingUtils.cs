namespace SG
{
    public static class SavingUtils
    {
        public static string CheckFileName(string filepath, string filename)
        {
            var path = System.IO.Path.Combine(filepath, filename);
            if (System.IO.File.Exists(path))
            {
                var newName = System.IO.Path.GetFileNameWithoutExtension(filename);
                var ext = System.IO.Path.GetExtension(filename);

                var numbers = newName.Split(char.Parse("_"));
                var number = numbers[^1].Split(char.Parse("."));
                var num = int.Parse(number[0]);
                number[0] = FormatNumberLength(++num, 3);
                numbers[^1] = string.Join(".", number);
                newName = string.Join("_", numbers);
                filename = CheckFileName(filepath, newName + ext);
            }
            return filename;
        }
        
        private static string FormatNumberLength(int num, int length)
        {
            var r = "" + num;
            while (r.Length < length)
            {
                r = "0" + r;
            }
            return r;
        }
    }
}