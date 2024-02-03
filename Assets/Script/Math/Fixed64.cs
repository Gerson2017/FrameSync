
    using System;

    [Serializable]
    public struct Fixed64 : IEquatable<Fixed64>,IComparable,IComparable<Fixed64>
    {

        public long MValue;
        /// <summary>
        /// 小数部分占的位数
        /// </summary>
        private const int FRACIONALBITS = 12;

        private const long ONE = 1L << FRACIONALBITS;

        public static Fixed64 Zero = new Fixed64(0);


        /// <summary>
        /// 直接对MValue 进行赋值
        /// </summary>
        /// <param name="value"></param>
        Fixed64(long value)
        {
            this.MValue = value;
        }

        /// <summary>
        /// 传入具体数字的构造函数 *ONE
        /// </summary>
        /// <param name="value"></param>
        public Fixed64(int value)
        {
            MValue = value * ONE;
        }

        public static Fixed64 operator +(Fixed64 a,Fixed64 b)
        {
            return new Fixed64(a.MValue + b.MValue);
        }
        
        public static Fixed64 operator -(Fixed64 a,Fixed64 b)
        {
            return new Fixed64(a.MValue - b.MValue);
        }
        
        public static Fixed64 operator *(Fixed64 a,Fixed64 b)
        {
            //乘法会多出FRACIONALBITS位
            return new Fixed64((a.MValue * b.MValue)>>FRACIONALBITS);
        }
        
        public static Fixed64 operator /(Fixed64 a,Fixed64 b)
        {
            //乘法会少掉FRACIONALBITS位
            return new Fixed64((a.MValue<<FRACIONALBITS) / b.MValue);
        }

        
        public static bool operator ==(Fixed64 a,Fixed64 b)
        {
        
            return a.MValue == b.MValue;
        }

        public static bool operator !=(Fixed64 a,Fixed64 b)
        {
            return !(a == b);
        }
        
        public static bool operator >(Fixed64 a,Fixed64 b)
        {
            return a.MValue > b.MValue;
        }
        
        public static bool operator <(Fixed64 a,Fixed64 b)
        {
            return a.MValue < b.MValue;
        }
        public static bool operator >=(Fixed64 a,Fixed64 b)
        {
            return a.MValue >= b.MValue;
        }
        public static bool operator <=(Fixed64 a,Fixed64 b)
        {
            return a.MValue <= b.MValue;
        }

        /// <summary>
        /// 转成long类型
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static explicit operator long(Fixed64 value)
        {
          return  value.MValue >> FRACIONALBITS;
        }
        
        
        /// <summary>
        /// long转成 fixed64
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static explicit operator Fixed64(long value)
        {
            return new Fixed64(value);
        }
        
        
        public static explicit operator float(Fixed64 value)
        {
            return  (float)value.MValue / ONE;
        }
        
        
        public static explicit operator Fixed64(float value)
        {
            return new Fixed64((long)(value * ONE));
        }

        
        public bool Equals(Fixed64 other)
        {
            return MValue == other.MValue;
        }


        public int CompareTo(object obj)
        {
            return MValue.CompareTo(obj);
        }

        public int CompareTo(Fixed64 other)
        {
            return MValue.CompareTo(other.MValue);
        }


        public override bool Equals(object obj)
        {
            return obj is Fixed64 && ((Fixed64)obj).MValue == MValue;
        }

        public override int GetHashCode()
        {
            return MValue.GetHashCode();
        }


        public override string ToString()
        {
            return ((float)this).ToString(); 
        }
    }
