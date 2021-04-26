using System;
using System.Collections.Generic;

namespace Seoner.Utils
{
    public static class Math
    {
        public static T RandomFromList<T>(List<T> Objects)
        {
            if (Objects == null)
                return default;
            Random r = new Random();
            int randomNumber = r.Next(0, Objects.Count);
            return Objects[randomNumber];
        }
    }
}

