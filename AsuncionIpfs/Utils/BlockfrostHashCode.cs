﻿namespace AsuncionIpfs.Utils
{
    internal struct BlockfrostHashCode
    {
        private int _hashcode;

        internal void Add<T>(T value)
        {
            if (value == null)
            {
                return;
            }

            unchecked
            {
                _hashcode += value.GetHashCode();
            }
        }

        internal int ToHashCode()
        {
            return _hashcode;
        }
    }
}
