// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("S9S70AZ420i0KDaslEUdwFIf/OFf3NLd7V/c199f3NzdDLh151RLMjsaoH7eSr9oTxUCsn4+SWW8TSkx//pywyV/ljdnd/qfCrncskeGUFR+S+4MPXJzpn7vJ3QUKUfLvZYLTS/Mt4KvFOt//5v3GZK90PHlIui+bWdyOwLhjSnAqFuiw7ZtdwfpAh89AWjnXChtLY5+zpZHMzbLycT8nyIKi02cD09IQ/LvsolAdoU7fd+ub1lwOcfjIbz+Cb1kp4BzPs93EcaUbauJGtA8xcqkTgAeoTVEsEsG413dzA4LkBjLtGYHw/0nj2vzLmOd3rNQUJiH2tNrIJi0/y3qAbkidqLtX9z/7dDb1PdblVsq0Nzc3Njd3m9nN0mxhIa1wt/e3N3c");
        private static int[] order = new int[] { 5,1,9,4,13,13,6,12,8,11,12,12,12,13,14 };
        private static int key = 221;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
