namespace ME.ECS.Mathematics
{

    public static class Helper
    {
        public static sfloat FromFPToSFloat(long val)
        {
            return (float)val / (1L << 16);
        }

        public static void Convert<T>(T[] components)
        {
            foreach (var obj in components)
            {
                Convert(obj);
            }
        }

        public static void Convert<T>(T obj)
        {
            {
                var type = obj.GetType();
                var fields = type.GetFields(System.Reflection.BindingFlags.Default | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                foreach (var field in fields)
                {
                    if (field.FieldType == typeof(sfloat))
                    {
                        var from = (sfloat)field.GetValue(obj);
                        var val = ME.ECS.Mathematics.Helper.FromFPToSFloat(from.RawValue);
                        field.SetValue(obj, val);
                    } else if (field.FieldType == typeof(float2))
                    {
                        var from = (float2)field.GetValue(obj);
                        var x = ME.ECS.Mathematics.Helper.FromFPToSFloat(from.x.RawValue);
                        var y = ME.ECS.Mathematics.Helper.FromFPToSFloat(from.y.RawValue);
                        field.SetValue(obj, new float2(x, y));
                    } else if (field.FieldType == typeof(float3))
                    {
                        var from = (float3)field.GetValue(obj);
                        var x = ME.ECS.Mathematics.Helper.FromFPToSFloat(from.x.RawValue);
                        var y = ME.ECS.Mathematics.Helper.FromFPToSFloat(from.y.RawValue);
                        var z = ME.ECS.Mathematics.Helper.FromFPToSFloat(from.z.RawValue);
                        field.SetValue(obj, new float3(x, y, z));
                    } else if (field.FieldType == typeof(float4))
                    {
                        var from = (float4)field.GetValue(obj);
                        var x = ME.ECS.Mathematics.Helper.FromFPToSFloat(from.x.RawValue);
                        var y = ME.ECS.Mathematics.Helper.FromFPToSFloat(from.y.RawValue);
                        var z = ME.ECS.Mathematics.Helper.FromFPToSFloat(from.z.RawValue);
                        var w = ME.ECS.Mathematics.Helper.FromFPToSFloat(from.w.RawValue);
                        field.SetValue(obj, new float4(x, y, z, w));
                    } else if (field.FieldType == typeof(quaternion))
                    {
                        var from = (quaternion)field.GetValue(obj);
                        var x = ME.ECS.Mathematics.Helper.FromFPToSFloat(from.value.x.RawValue);
                        var y = ME.ECS.Mathematics.Helper.FromFPToSFloat(from.value.y.RawValue);
                        var z = ME.ECS.Mathematics.Helper.FromFPToSFloat(from.value.z.RawValue);
                        var w = ME.ECS.Mathematics.Helper.FromFPToSFloat(from.value.w.RawValue);
                        field.SetValue(obj, new quaternion(x, y, z, w));
                    }
                    else if (field.FieldType == typeof(pose))
                    {
                        var from = (pose)field.GetValue(obj);
                        var px = ME.ECS.Mathematics.Helper.FromFPToSFloat(from.position.x.RawValue);
                        var py = ME.ECS.Mathematics.Helper.FromFPToSFloat(from.position.y.RawValue);
                        var pz = ME.ECS.Mathematics.Helper.FromFPToSFloat(from.position.z.RawValue);
                        var rx = ME.ECS.Mathematics.Helper.FromFPToSFloat(from.rotation.value.x.RawValue);
                        var ry = ME.ECS.Mathematics.Helper.FromFPToSFloat(from.rotation.value.y.RawValue);
                        var rz = ME.ECS.Mathematics.Helper.FromFPToSFloat(from.rotation.value.z.RawValue);
                        var rw = ME.ECS.Mathematics.Helper.FromFPToSFloat(from.rotation.value.w.RawValue);
                        field.SetValue(obj, new pose(new float3(px, py, pz), new quaternion(rx, ry, rz, rw)));
                    }
                }
            }
        }

    }

}